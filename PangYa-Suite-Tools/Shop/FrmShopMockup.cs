using PangYa_Suite_Tools.Localization;
using PangYa_Suite_Tools.Logging;
using PangyaAPI.PAK.Models;
using PangyaAPI.UI;

namespace PangYa_Suite_Tools.Shop;

internal sealed class FrmShopMockup : Form
{
    internal const float ZoomFactor = 1.4f;
    private readonly ShopCanvas _canvas;

    private FrmShopMockup(ShopLayout layout, PangyaFileImageProvider assets,
        IReadOnlyList<ShopCatalogItem> catalog, string iffPath,
        string schemaRegion)
    {
        Text = Strings.Shop_Title;
        ClientSize = new Size((int)Math.Ceiling(layout.Size.Width * ZoomFactor), (int)Math.Ceiling(layout.Size.Height * ZoomFactor));
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        _canvas = new ShopCanvas(layout, assets, catalog, iffPath, schemaRegion)
        {
            Dock = DockStyle.Fill,
        };
        Controls.Add(_canvas);
        LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;
        Disposed += (_, _) => LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
    }

    public static async Task<FrmShopMockup> CreateAsync(string dataRoot, CancellationToken cancellationToken = default)
    {
        AppLogger.Instance.Log("Shop",
            $"Opening the Shop Editor from '{dataRoot}'.");
        ShopDataFiles files = ShopDataFiles.Resolve(dataRoot);
        AppLogger.Instance.Log("Shop",
            $"Resolved shop.xml='{files.ShopXml}', predefined.xml='{files.PredefinedXml}', IFF='{files.IffPath}'.");

        ShopRegionProbe regionProbe = await Task.Run(
            () => ShopCatalogLoader.ProbeRegionAsync(files.IffPath,
                cancellationToken), cancellationToken);
        string? schemaRegion = regionProbe.Region;
        if (schemaRegion is null)
        {
            if (regionProbe.Document is null)
                throw new InvalidDataException(
                    "The IFF archive contains no entries from which to determine a region.");
            using var dialog = new IffUnknownRegionDialog(regionProbe.Document);
            if (dialog.ShowDialog() != DialogResult.OK)
                throw new OperationCanceledException(cancellationToken);
            schemaRegion = dialog.SelectedRegion;
        }
        AppLogger.Instance.Log("Shop",
            $"Using the '{schemaRegion}' IFF schema region for the shop catalog.");

        (ShopLayout layout, PangyaFileImageProvider assets,
            IReadOnlyList<ShopCatalogItem> catalog) = await Task.Run(async () =>
        {
            ShopLayout parsedLayout =
                ShopLayoutParser.Load(files.ShopXml, files.PredefinedXml);
            var resolver = new PangyaFileImageProvider(files.DataRoot);
            try
            {
                LogMissingSkinAssets(parsedLayout, resolver);
                ShopCatalogLoadResult loadedCatalog =
                    await ShopCatalogLoader.LoadAsync(files.IffPath, resolver,
                        schemaRegion,
                        cancellationToken);
                if (loadedCatalog.MissingIconCount != 0)
                    AppLogger.Instance.Log("Shop",
                        string.Format(LocalizationManager.CurrentCulture,
                            Strings.Shop_MissingIconsSkipped,
                            loadedCatalog.MissingIconCount), AppLogLevel.Warning);
                AppLogger.Instance.Log("Shop",
                    $"Loaded {loadedCatalog.Items.Count} catalog records from '{files.IffPath}'.");
                return (parsedLayout, resolver, loadedCatalog.Items);
            }
            catch
            {
                resolver.Dispose();
                throw;
            }
        }, cancellationToken);
        return new FrmShopMockup(layout, assets, catalog, files.IffPath,
            schemaRegion);
    }

    internal static void LogMissingSkinAssets(ShopLayout layout,
        PangyaFileImageProvider resolver)
    {
        string[] imageParameters = ["bgimg", "normal", "over", "selected", "below_over", "below_selected", "sepImg"];
        var checkedResources = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (ShopLayoutElement element in layout.Elements)
            foreach (string key in imageParameters)
                if (element.Parameters.TryGetValue(key, out string? value) &&
                    !string.IsNullOrWhiteSpace(value) &&
                    checkedResources.Add(value) &&
                    resolver.TryResolvePath(value) is null)
                    AppLogger.Instance.Log("Shop", string.Format(
                        LocalizationManager.CurrentCulture,
                        Strings.Shop_MissingSkinResourceSkipped, value),
                        AppLogLevel.Warning);
    }

    private void LocalizationManager_CultureChanged(object? sender, EventArgs e)
    {
        Text = Strings.Shop_Title;
        _canvas.Invalidate();
    }
}

internal sealed record ShopDataFiles(
    string DataRoot,
    string ShopXml,
    string PredefinedXml,
    string IffPath)
{
    public static ShopDataFiles Resolve(string selectedRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(selectedRoot);
        string fullRoot = Path.GetFullPath(selectedRoot);
        if (!Directory.Exists(fullRoot))
            throw new DirectoryNotFoundException(fullRoot);

        string[] iffCandidates = DiscoverPangyaIffFiles(fullRoot);
        if (iffCandidates.Length == 0)
            throw new FileNotFoundException(Strings.Shop_MissingRequiredFile,
                Path.Combine(fullRoot, "pangya_*.iff"));
        if (iffCandidates.Length > 1)
            throw new InvalidDataException(
                $"Multiple pangya_*.iff files were found: {string.Join(", ", iffCandidates)}");

        string iffPath = iffCandidates[0];
        var candidates = new List<(string Root, string IffPath)>();
        string? manifestRoot =
            PakExtractionSidecar.TryResolveExtractionRoot(iffPath);
        if (manifestRoot is not null && Directory.Exists(manifestRoot))
            candidates.Add((manifestRoot, iffPath));
        candidates.Add((fullRoot, iffPath));
        candidates.Add((Path.Combine(fullRoot, "data"), iffPath));
        string iffDirectory = Path.GetDirectoryName(iffPath)!;
        candidates.Add((iffDirectory, iffPath));
        if (Path.GetFileName(iffDirectory).Equals("data",
                StringComparison.OrdinalIgnoreCase) &&
            Directory.GetParent(iffDirectory) is { } parent)
            candidates.Add((parent.FullName, iffPath));

        string? firstMissingPath = null;
        foreach ((string candidate, string candidateIffPath) in candidates.DistinctBy(
                     value => $"{value.Root}\u001f{value.IffPath}",
                     StringComparer.OrdinalIgnoreCase))
        {
            string uiDirectory = Path.Combine(candidate, "ui");
            if (!Directory.Exists(uiDirectory))
            {
                firstMissingPath ??= uiDirectory;
                continue;
            }

            IReadOnlyList<string> uiFiles = PangyaUiDocument.FindUiFiles(candidate);
            string? shopXml = SelectUiFile(uiFiles, uiDirectory, "shop.xml");
            string? predefinedXml =
                SelectUiFile(uiFiles, uiDirectory, "predefined.xml");
            firstMissingPath ??= shopXml is null
                ? Path.Combine(uiDirectory, "shop.xml")
                : predefinedXml is null
                    ? Path.Combine(uiDirectory, "predefined.xml")
                    : candidateIffPath;
            if (shopXml is not null && predefinedXml is not null &&
                File.Exists(candidateIffPath))
                return new ShopDataFiles(candidate, shopXml, predefinedXml,
                    candidateIffPath);
        }

        throw new FileNotFoundException(Strings.Shop_MissingRequiredFile,
            firstMissingPath);
    }

    private static string[] DiscoverPangyaIffFiles(string selectedRoot)
    {
        string[] directories =
        [
            selectedRoot,
            Path.Combine(selectedRoot, "data"),
        ];
        return directories.Where(Directory.Exists)
            .SelectMany(directory => Directory.EnumerateFiles(directory, "*",
                SearchOption.TopDirectoryOnly))
            .Where(path => PakExtractionSidecar.IsPangyaIff(
                Path.GetFileName(path)))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? SelectUiFile(IReadOnlyList<string> uiFiles,
        string uiDirectory, string fileName)
    {
        string directPath = Path.Combine(uiDirectory, fileName);
        string? direct = uiFiles.FirstOrDefault(path =>
            path.Equals(directPath, StringComparison.OrdinalIgnoreCase));
        if (direct is not null) return direct;

        string[] matches = uiFiles.Where(path =>
                Path.GetFileName(path).Equals(fileName,
                    StringComparison.OrdinalIgnoreCase))
            .ToArray();
        return matches.Length switch
        {
            0 => null,
            1 => matches[0],
            _ => throw new InvalidDataException(
                $"Multiple '{fileName}' files were found: {string.Join(", ", matches)}"),
        };
    }
}

internal sealed class ShopCanvas : Control
{
    private readonly ShopLayout _layout;
    private readonly PangyaFileImageProvider _assets;
    private readonly ShopRenderer _renderer;
    private readonly IReadOnlyList<ShopCatalogItem> _catalog;
    private readonly ShopSession _emptySession = new();
    private readonly string _iffPath;
    private readonly string? _schemaRegion;
    private ShopRenderResult _renderResult = new(0, 0, 0, []);
    private string? _hoveredElement;
    private string _filter = string.Empty;
    private int _categoryIndex;
    private int _page;
    private bool _rental;
    private bool _editing;
    private Func<ShopCatalogItem, Task>? _editItemHandler;

    public ShopCanvas(ShopLayout layout, PangyaFileImageProvider assets,
        IReadOnlyList<ShopCatalogItem> catalog, string iffPath,
        string? schemaRegion = null)
    {
        _layout = layout;
        _assets = assets;
        _renderer = new ShopRenderer(assets);
        _catalog = catalog;
        _iffPath = iffPath;
        _schemaRegion = schemaRegion;
        DoubleBuffered = true;
        TabStop = true;
        SetStyle(ControlStyles.Selectable, true);
        Cursor = Cursors.Default;
        MouseEnter += (_, _) => Focus();
        MouseMove += ShopCanvas_MouseMove;
        MouseLeave += (_, _) => { _hoveredElement = null; Invalidate(); };
        MouseClick += ShopCanvas_MouseClick;
        MouseWheel += ShopCanvas_MouseWheel;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _assets.Dispose();
        base.Dispose(disposing);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(Color.FromArgb(25, 28, 33));
        e.Graphics.ScaleTransform(FrmShopMockup.ZoomFactor, FrmShopMockup.ZoomFactor);
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        _renderResult = _renderer.Render(e.Graphics, _layout, _catalog,
            new ShopRenderState(_categoryIndex, _page, _rental, _filter,
                _hoveredElement, _emptySession, ShopRenderMode.Editor),
            new ShopRenderText(Strings.Shop_NoItems, Strings.Shop_CartSummary,
                Strings.Shop_Balances, Strings.Shop_Filter, Strings.Shop_EditHint),
            LocalizationManager.CurrentCulture);
        _categoryIndex = _renderResult.CategoryIndex;
        _page = _renderResult.Page;
    }

    private ShopCatalogItem[] GetFilteredItems(string category)
    {
        IEnumerable<ShopCatalogItem> query = _catalog.Where(item => item.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (_filter.Length != 0) query = query.Where(item => item.Name.Contains(_filter, StringComparison.CurrentCultureIgnoreCase));
        return query.ToArray();
    }

    private int CurrentMaximumPage()
    {
        string[] categories = _catalog.Select(item => item.Category).Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();
        if (categories.Length == 0) return 0;
        int categoryIndex = Math.Clamp(_categoryIndex, 0, categories.Length - 1);
        return ShopRenderer.GetMaximumPage(GetFilteredItems(categories[categoryIndex]).Length);
    }

    internal static string? GetBannerResource(ShopCatalogItem item) =>
        ShopRenderer.GetBannerResource(item);

    private void ShopCanvas_MouseMove(object? sender, MouseEventArgs e)
    {
        Point logicalPoint = ToLogical(e.Location);
        string? hovered = HitElement(logicalPoint)?.Name;
        if (hovered == _hoveredElement) return;
        _hoveredElement = hovered;
        Cursor = hovered is null &&
                 !_renderResult.VisibleItems.Any(item => item.Bounds.Contains(logicalPoint))
            ? Cursors.Default
            : Cursors.Hand;
        Invalidate();
    }

    internal async void ShopCanvas_MouseClick(object? sender, MouseEventArgs e)
    {
        if (_editing) return;
        Point logicalPoint = ToLogical(e.Location);
        if (ShopRenderer.ScrollBarBounds.Contains(logicalPoint))
        {
            int maximumPage = CurrentMaximumPage();
            _page = ShopRenderer.ScrollPageFromPoint(logicalPoint, maximumPage);
            Invalidate();
            return;
        }
        ShopVisibleItem? visible = ShopRenderer.HitTestItem(_renderResult, logicalPoint);
        if (visible is not null)
        {
            if (e.Button is MouseButtons.Left or MouseButtons.Right)
            {
                if (_editItemHandler is not null)
                    await _editItemHandler(visible.Item);
                else
                    await EditItemAsync(visible.Item);
            }
            return;
        }
        switch (HitElement(logicalPoint)?.Name)
        {
            case "close_wnd": FindForm()?.Close(); break;
            case "sidetab_buy": _rental = false; _page = 0; Invalidate(); break;
            case "sidetab_rental": _rental = true; _page = 0; Invalidate(); break;
            case "scroll_up": ScrollByPage(-1); break;
            case "scroll_down": ScrollByPage(1); break;
            case "tab_main": CycleCategory(); break;
            case "item_search_btn":
            case "searchgoods": SetSearchFilter(); break;
        }
    }

    private void ShopCanvas_MouseWheel(object? sender, MouseEventArgs e)
    {
        Point logicalPoint = ToLogical(e.Location);
        if (!ShopRenderer.CatalogBounds.Contains(logicalPoint)) return;
        int steps = Math.Max(1, Math.Abs(e.Delta) / SystemInformation.MouseWheelScrollDelta);
        ScrollByPage(e.Delta > 0 ? -steps : steps);
    }

    protected override bool IsInputKey(Keys keyData) => keyData is Keys.Up or Keys.Down or Keys.PageUp or Keys.PageDown
        || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        int delta = e.KeyCode switch
        {
            Keys.Up or Keys.PageUp => -1,
            Keys.Down or Keys.PageDown => 1,
            _ => 0,
        };
        if (delta == 0) return;
        ScrollByPage(delta);
        e.Handled = true;
    }

    private void ScrollByPage(int delta)
    {
        _page = Math.Clamp(_page + delta, 0, CurrentMaximumPage());
        Invalidate();
    }

    private Point ToLogical(Point point) => new(
        (int)(point.X / FrmShopMockup.ZoomFactor), (int)(point.Y / FrmShopMockup.ZoomFactor));

    private async Task EditItemAsync(ShopCatalogItem item)
    {
        _editing = true;
        using var dialog = new ShopItemDialog(item, _assets.DataRoot);
        try
        {
            while (dialog.ShowDialog(FindForm()) == DialogResult.OK)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(dialog.IconPath))
                    {
                        using Image probe = PangyaImageLoader.Load(dialog.IconPath)
                            ?? throw new InvalidDataException(
                                Strings.Shop_InvalidIcon);
                    }
                    await ShopCatalogEditor.SaveAsync(_iffPath, item, dialog.IconId,
                        dialog.Price, dialog.DiscountPrice, dialog.RentalPrice,
                        dialog.ShopFlags, dialog.MoneyFlags, dialog.TimeFlag,
                        dialog.Time, dialog.StartDate, dialog.EndDate,
                        schemaRegion: _schemaRegion);
                    item.IconId = dialog.IconId;
                    item.IconPath = dialog.IconPath;
                    item.Price = dialog.Price;
                    item.DiscountPrice = dialog.DiscountPrice;
                    item.RentalPrice = dialog.RentalPrice;
                    item.ShopFlags = dialog.ShopFlags;
                    item.MoneyFlags = dialog.MoneyFlags;
                    item.TimeFlag = dialog.TimeFlag;
                    item.Time = dialog.Time;
                    item.StartDate = dialog.StartDate;
                    item.EndDate = dialog.EndDate;
                    MessageBox.Show(FindForm(), Strings.Shop_EditSaved,
                        Strings.Shop_Title, MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    break;
                }
                catch (Exception ex) when (ex is IOException or InvalidDataException
                           or UnauthorizedAccessException or ArgumentException
                           or InvalidOperationException or OutOfMemoryException)
                {
                    AppLogger.Instance.Log("Shop",
                        $"Could not save shop item {item.ItemId}: {ex.GetType().Name}: {ex.Message}",
                        AppLogLevel.Error);
                    MessageBox.Show(FindForm(), string.Format(
                            LocalizationManager.CurrentCulture,
                            Strings.Shop_EditFailed, ex.Message),
                        Strings.Common_Error, MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        finally { _editing = false; Invalidate(); }
    }

    private void CycleCategory()
    {
        int count = _catalog.Select(item => item.Category).Distinct(StringComparer.OrdinalIgnoreCase).Count();
        if (count != 0) _categoryIndex = (_categoryIndex + 1) % count;
        _page = 0;
        Invalidate();
    }

    private void SetSearchFilter()
    {
        using var prompt = new ShopSearchDialog(_filter);
        if (prompt.ShowDialog(FindForm()) != DialogResult.OK) return;
        _filter = prompt.Filter.Trim();
        _page = 0;
        Invalidate();
    }

    private ShopLayoutElement? HitElement(Point point) =>
        _renderer.HitTestElement(_layout, point);

    internal void SetEditItemHandler(Func<ShopCatalogItem, Task> handler) =>
        _editItemHandler = handler;
}

internal sealed class ShopItemDialog : Form
{
    private readonly string _dataRoot;
    private readonly PictureBox _iconPreview = new()
    {
        BorderStyle = BorderStyle.FixedSingle,
        SizeMode = PictureBoxSizeMode.Zoom,
    };
    private readonly NumericUpDown _price = CreatePriceEditor();
    private readonly NumericUpDown _discount = CreatePriceEditor();
    private readonly NumericUpDown _rental = CreatePriceEditor();
    private readonly FlagCheckBoxEditor _shopFlags;
    private readonly FlagCheckBoxEditor _moneyFlags;
    private readonly FlagCheckBoxEditor _timeFlag;
    private readonly NumericUpDown _time = CreateByteEditor();
    private readonly DateTimePicker _startDate = CreateDateEditor();
    private readonly DateTimePicker _endDate = CreateDateEditor();
    public uint Price => decimal.ToUInt32(_price.Value);
    public uint DiscountPrice => decimal.ToUInt32(_discount.Value);
    public uint RentalPrice => decimal.ToUInt32(_rental.Value);
    public byte ShopFlags => _shopFlags.Value;
    public byte MoneyFlags => _moneyFlags.Value;
    public byte TimeFlag => _timeFlag.Value;
    public byte Time => decimal.ToByte(_time.Value);
    public DateTime? StartDate => _startDate.Checked ? _startDate.Value : null;
    public DateTime? EndDate => _endDate.Checked ? _endDate.Value : null;
    public string IconId { get; private set; }
    public string IconPath { get; private set; }

    public ShopItemDialog(ShopCatalogItem item, string dataRoot)
    {
        _dataRoot = Path.GetFullPath(dataRoot);
        IconId = item.IconId;
        IconPath = item.IconPath;
        _shopFlags = new FlagCheckBoxEditor([
            Strings.Shop_FlagIsCash, Strings.Shop_FlagCanSendMailAndPersonalShop,
            Strings.Shop_FlagCanDuplicate, Strings.Shop_FlagShopSpecial,
            Strings.Shop_FlagBlockMailAndPersonalShop, Strings.Shop_FlagIsSaleable,
            Strings.Shop_FlagIsGift, Strings.Shop_FlagOnlyDisplay]);
        _moneyFlags = new FlagCheckBoxEditor([
            Strings.Shop_FlagInStock, Strings.Shop_FlagShowNew, Strings.Shop_FlagDisplayOnly,
            UnknownFlag(0x08), UnknownFlag(0x10), Strings.Shop_FlagShowHot,
            Strings.Shop_FlagShowSpecial, UnknownFlag(0x80)]);
        _timeFlag = new FlagCheckBoxEditor(Enumerable.Range(0, 8).Select(bit => UnknownFlag(1 << bit)));
        Text = Strings.Shop_EditItem;
        ClientSize = new Size(620, 700);
        AutoScroll = true;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        _price.Value = item.Price; _discount.Value = item.DiscountPrice; _rental.Value = item.RentalPrice;
        _shopFlags.Value = item.ShopFlags; _moneyFlags.Value = item.MoneyFlags;
        _timeFlag.Value = item.TimeFlag; _time.Value = item.Time;
        SetDate(_startDate, item.StartDate); SetDate(_endDate, item.EndDate);
        AddReadOnlyRow(Strings.Shop_ItemId, $"0x{item.ItemId:X8} / {item.ItemId}", 18);
        AddReadOnlyRow(Strings.Shop_ItemName, item.Name, 48);
        AddReadOnlyRow(Strings.Shop_Category, item.Category, 78);
        AddReadOnlyRow(Strings.Shop_SourceEntry,
            $"{item.EntryName} #{item.RecordIndex}", 108);
        _iconPreview.Location = new Point(448, 18);
        _iconPreview.Size = new Size(72, 72);
        if (!string.IsNullOrWhiteSpace(IconPath))
        {
            try
            {
                LoadIconPreview(IconPath);
            }
            catch (Exception ex) when (ex is IOException or InvalidDataException
                       or UnauthorizedAccessException or ArgumentException
                       or OutOfMemoryException)
            {
                _iconPreview.Image = null;
            }
        }
        var browseIcon = new Button
        {
            Text = Strings.Shop_ChangeIcon,
            Location = new Point(526, 40),
            Size = new Size(82, 28),
        };
        browseIcon.Click += BrowseIcon_Click;
        Controls.AddRange([_iconPreview, browseIcon]);
        AddRow(Strings.Shop_Price, _price, 148);
        AddRow(Strings.Shop_DiscountPrice, _discount, 188);
        AddRow(Strings.Shop_RentalPrice, _rental, 228);
        AddFlagRow(Strings.Shop_ShopFlags, _shopFlags, 268);
        AddFlagRow(Strings.Shop_MoneyFlags, _moneyFlags, 380);
        AddFlagRow(Strings.Shop_TimeFlag, _timeFlag, 492);
        AddRow(Strings.Shop_Time, _time, 604);
        AddRow(Strings.Shop_StartDate, _startDate, 644);
        AddRow(Strings.Shop_EndDate, _endDate, 684);
        var save = new Button { Text = Strings.IFFManager_Save, DialogResult = DialogResult.OK, Location = new Point(438, 728), Size = new Size(80, 28) };
        var cancel = new Button { Text = Strings.Options_Cancel, DialogResult = DialogResult.Cancel, Location = new Point(526, 728), Size = new Size(80, 28) };
        Controls.AddRange([save, cancel]);
        AcceptButton = save; CancelButton = cancel;
        AutoScrollMinSize = new Size(0, 770);
        Disposed += (_, _) => _iconPreview.Image?.Dispose();
    }

    private void AddReadOnlyRow(string text, string value, int y)
    {
        Controls.Add(new Label
        {
            Text = text,
            Location = new Point(12, y + 3),
            Size = new Size(150, 24),
        });
        Controls.Add(new TextBox
        {
            Text = value,
            ReadOnly = true,
            Location = new Point(170, y),
            Size = new Size(265, 27),
        });
    }

    private void BrowseIcon_Click(object? sender, EventArgs e)
    {
        using var dialog = FileDialogFactory.CreateIconOpenDialog(
            Path.GetDirectoryName(IconPath));
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        FileDialogFactory.RememberDirectory(FileDialogKind.Icon, dialog.FileName);
        string selectedPath = Path.GetFullPath(dialog.FileName);
        string rootPrefix = _dataRoot.TrimEnd(Path.DirectorySeparatorChar) +
                            Path.DirectorySeparatorChar;
        if (!selectedPath.StartsWith(rootPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, Strings.Shop_IconMustBeInData,
                Strings.Shop_Title, MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        try
        {
            LoadIconPreview(selectedPath);
            IconPath = selectedPath;
            IconId = Path.GetFileNameWithoutExtension(selectedPath);
        }
        catch (Exception ex) when (ex is IOException or InvalidDataException
                   or UnauthorizedAccessException or ArgumentException
                   or OutOfMemoryException)
        {
            MessageBox.Show(this, string.Format(LocalizationManager.CurrentCulture,
                    Strings.Shop_EditFailed, ex.Message),
                Strings.Common_Error, MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadIconPreview(string path)
    {
        Image image = PangyaImageLoader.Load(path)
                      ?? throw new InvalidDataException(Strings.Shop_InvalidIcon);
        Image? previous = _iconPreview.Image;
        _iconPreview.Image = image;
        previous?.Dispose();
    }

    private void AddRow(string text, Control editor, int y)
    {
        Controls.Add(new Label { Text = text, Location = new Point(12, y + 4), Size = new Size(175, 24) });
        editor.Location = new Point(210, y); editor.Size = new Size(326, 27); Controls.Add(editor);
    }

    private void AddFlagRow(string text, FlagCheckBoxEditor editor, int y)
    {
        Controls.Add(new Label { Text = text, Location = new Point(12, y), Size = new Size(185, 100), TextAlign = ContentAlignment.MiddleLeft });
        editor.Location = new Point(210, y); editor.Size = new Size(326, 104); Controls.Add(editor);
    }

    private static NumericUpDown CreatePriceEditor() => new()
    {
        Minimum = 0, Maximum = uint.MaxValue, ThousandsSeparator = true,
    };

    private static NumericUpDown CreateByteEditor() => new()
    {
        Minimum = byte.MinValue, Maximum = byte.MaxValue,
        Hexadecimal = true,
    };

    private static DateTimePicker CreateDateEditor() => new()
    {
        Format = DateTimePickerFormat.Custom,
        CustomFormat = "yyyy-MM-dd HH:mm:ss",
        ShowCheckBox = true,
    };

    private static void SetDate(DateTimePicker picker, DateTime? value)
    {
        picker.Checked = value.HasValue;
        picker.Value = value ?? DateTime.Today;
    }

    private static string UnknownFlag(int mask) => string.Format(LocalizationManager.CurrentCulture,
        Strings.Shop_FlagUnknown, mask);
}

internal sealed class FlagCheckBoxEditor : UserControl
{
    private readonly CheckBox[] _bits;

    public FlagCheckBoxEditor(IEnumerable<string> labels)
    {
        BorderStyle = BorderStyle.FixedSingle;
        var table = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 4,
            Margin = Padding.Empty,
            Padding = new Padding(3),
        };
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int row = 0; row < 4; row++) table.RowStyles.Add(new RowStyle(SizeType.Percent, 25));
        string[] labelArray = labels.Take(8).ToArray();
        if (labelArray.Length != 8) throw new ArgumentException("Exactly eight flag labels are required.", nameof(labels));
        _bits = Enumerable.Range(0, 8).Select(bit => new CheckBox
        {
            AutoSize = false,
            Dock = DockStyle.Fill,
            AutoEllipsis = true,
            Margin = new Padding(2, 0, 2, 0),
            Text = labelArray[bit],
            Tag = bit,
        }).ToArray();
        for (int bit = 0; bit < _bits.Length; bit++) table.Controls.Add(_bits[bit], bit % 2, bit / 2);
        Controls.Add(table);
    }

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public byte Value
    {
        get => (byte)_bits.Where(checkBox => checkBox.Checked)
            .Aggregate(0, (value, checkBox) => value | 1 << (int)checkBox.Tag!);
        set
        {
            foreach (CheckBox checkBox in _bits)
                checkBox.Checked = (value & (1 << (int)checkBox.Tag!)) != 0;
        }
    }
}

internal sealed class ShopSearchDialog : Form
{
    private readonly TextBox _text = new() { Dock = DockStyle.Top };
    public string Filter => _text.Text;
    public ShopSearchDialog(string current)
    {
        Text = Strings.Shop_Search;
        ClientSize = new Size(360, 85);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = MinimizeBox = false;
        _text.Text = current;
        var ok = new Button { Text = Strings.Common_OK, DialogResult = DialogResult.OK, Location = new Point(195, 42), Size = new Size(75, 28) };
        var cancel = new Button { Text = Strings.Options_Cancel, DialogResult = DialogResult.Cancel, Location = new Point(276, 42), Size = new Size(75, 28) };
        Controls.AddRange([_text, ok, cancel]);
        AcceptButton = ok;
        CancelButton = cancel;
    }
}
