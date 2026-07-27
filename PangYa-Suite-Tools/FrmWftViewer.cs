using PangyaAPI.WFT;
using PangYa_Suite_Tools.Localization;
using PangYa_Suite_Tools.Logging;
using System.Globalization;

namespace PangYa_Suite_Tools;

internal sealed class FrmWftViewer : Form
{
    private const string LogSource = "WFT Viewer";
    private readonly ToolStrip _toolbar = new();
    private readonly ToolStripButton _openButton = new();
    private readonly ToolStripButton _reloadButton = new();
    private readonly ToolStripLabel _jumpLabel = new();
    private readonly ToolStripTextBox _jumpText = new() { Width = 82 };
    private readonly ToolStripButton _jumpButton = new();
    private readonly ToolStripLabel _zoomLabel = new();
    private readonly ToolStripButton _zoomOutButton = new() { Text = "−" };
    private readonly ToolStripLabel _zoomValue = new();
    private readonly ToolStripButton _zoomInButton = new() { Text = "+" };
    private readonly WftGlyphGrid _glyphGrid = new() { Dock = DockStyle.Fill };
    private readonly Label _metadata = new() { Dock = DockStyle.Fill, AutoSize = false };
    private readonly Label _glyphDetails = new() { Dock = DockStyle.Fill, AutoSize = false };
    private readonly NearestNeighborPictureBox _glyphPreview = new() { Dock = DockStyle.Fill };
    private readonly TextBox _sampleText = new()
    {
        Name = "txtWftSample",
        Dock = DockStyle.Fill,
        Multiline = true
    };
    private readonly NearestNeighborPictureBox _samplePreview = new() { Dock = DockStyle.Fill };
    private readonly ToolStripStatusLabel _statusLabel = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly StatusStrip _status = new();
    private WftFont? _font;
    private string? _currentPath;
    private CancellationTokenSource? _sampleCancellation;
    private int _sampleScale = 2;
    private int _loadGeneration;
    private bool _sampleInitialized;

    public FrmWftViewer()
    {
        Name = "frmWftViewer";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(900, 620);
        AllowDrop = true;

        ConfigureToolbar();
        ConfigureLayout();
        _status.Items.Add(_statusLabel);
        Controls.Add(_status);
        Controls.Add(_toolbar);

        _glyphGrid.GlyphSelected += async (_, codePoint) => await SelectGlyphAsync(codePoint);
        _sampleText.TextChanged += (_, _) => QueueSampleRender();
        _openButton.Click += async (_, _) => await BrowseAsync();
        _reloadButton.Click += async (_, _) =>
        {
            if (_currentPath is not null) await LoadFileAsync(_currentPath);
        };
        _jumpButton.Click += (_, _) => JumpToCodePoint();
        _jumpText.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            JumpToCodePoint();
            e.SuppressKeyPress = true;
        };
        _zoomOutButton.Click += (_, _) => ChangeSampleScale(-1);
        _zoomInButton.Click += (_, _) => ChangeSampleScale(1);
        DragEnter += FrmWftViewer_DragEnter;
        DragDrop += async (_, e) =>
        {
            if (TryGetDroppedFile(e.Data, out string? path)) await LoadFileAsync(path);
        };
        LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;
        ApplyLocalization();
    }

    internal WftFont? LoadedFont => _font;
    internal ushort? SelectedCodePoint => _glyphGrid.SelectedCodePoint;
    internal string StatusText => _statusLabel.Text ?? string.Empty;

    private void ConfigureToolbar()
    {
        _toolbar.GripStyle = ToolStripGripStyle.Hidden;
        _toolbar.Dock = DockStyle.Top;
        _toolbar.Padding = new Padding(4, 2, 4, 2);
        _reloadButton.Enabled = false;
        _toolbar.Items.AddRange([
            _openButton, _reloadButton, new ToolStripSeparator(),
            _jumpLabel, _jumpText, _jumpButton, new ToolStripSeparator(),
            _zoomLabel, _zoomOutButton, _zoomValue, _zoomInButton
        ]);
    }

    private void ConfigureLayout()
    {
        var mainSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Size = new Size(1160, 700),
            SplitterDistance = 700,
            Panel1MinSize = 420,
            Panel2MinSize = 300
        };
        mainSplit.Panel1.Controls.Add(_glyphGrid);

        var details = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(8)
        };
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 34));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 62));
        details.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 66));
        details.Controls.Add(_metadata, 0, 0);
        details.Controls.Add(_glyphDetails, 0, 1);
        details.Controls.Add(_glyphPreview, 0, 2);
        var sampleLabel = new Label
        {
            Name = "lblWftSample",
            Dock = DockStyle.Bottom,
            AutoSize = true
        };
        details.Controls.Add(sampleLabel, 0, 3);
        details.Controls.Add(_sampleText, 0, 4);
        details.Controls.Add(_samplePreview, 0, 5);
        mainSplit.Panel2.Controls.Add(details);
        Controls.Add(mainSplit);
    }

    private async Task BrowseAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = Strings.WftViewer_OpenTitle,
            Filter = Strings.WftViewer_FileFilter,
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) == DialogResult.OK) await LoadFileAsync(dialog.FileName);
    }

    internal async Task LoadFileAsync(string path)
    {
        int generation = ++_loadGeneration;
        try
        {
            if (!Path.GetExtension(path).Equals(".wft", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException(Strings.WftViewer_InvalidExtension);
            UseWaitCursor = true;
            _toolbar.Enabled = false;
            _statusLabel.Text = Strings.WftViewer_Loading;
            WftFont loaded = await Task.Run(() => WftFontReader.Open(path));
            if (generation != _loadGeneration)
            {
                loaded.Dispose();
                return;
            }
            ReplaceFont(loaded);
            _currentPath = loaded.Path;
            UpdateMetadata();
            Text = $"{Strings.WftViewer_Title} — {Path.GetFileName(loaded.Path)}";
            _statusLabel.Text = string.Format(LocalizationManager.CurrentCulture,
                Strings.WftViewer_LoadedFormat, Path.GetFileName(loaded.Path));
            _reloadButton.Enabled = true;
            _glyphGrid.SelectCodePoint(0x0041);
            QueueSampleRender();
            AppLogger.Instance.Log(LogSource, $"Loaded WFT font '{loaded.Path}'.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or
                                   InvalidDataException or ArgumentException or OverflowException)
        {
            _statusLabel.Text = string.Format(LocalizationManager.CurrentCulture,
                Strings.WftViewer_LoadFailed, ex.Message);
            AppLogger.Instance.Log(LogSource,
                $"Could not load WFT font: {ex.GetType().Name}: {ex.Message}", AppLogLevel.Error);
            MessageBox.Show(this, _statusLabel.Text, Strings.Common_Error,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _toolbar.Enabled = true;
            _reloadButton.Enabled = _font is not null;
        }
    }

    private void ReplaceFont(WftFont font)
    {
        CancelSampleRender();
        _glyphGrid.LoadFont(null);
        _font?.Dispose();
        _font = font;
        _glyphGrid.LoadFont(font);
        _glyphPreview.Image = null;
        _samplePreview.Image = null;
        _glyphDetails.Text = string.Empty;
    }

    private async Task SelectGlyphAsync(ushort codePoint)
    {
        WftFont? font = _font;
        if (font is null) return;
        try
        {
            WftGlyph glyph = await Task.Run(() => font.ReadGlyph(codePoint));
            if (!ReferenceEquals(font, _font)) return;
            _glyphPreview.Image = WftGlyphRenderer.CreateBitmap(glyph, Color.White);
            string character = char.IsControl((char)codePoint) || char.IsSurrogate((char)codePoint)
                ? "—"
                : ((char)codePoint).ToString();
            _glyphDetails.Text = string.Format(LocalizationManager.CurrentCulture,
                Strings.WftViewer_GlyphFormat, character, codePoint, glyph.AdvanceWidth);
        }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidDataException)
        {
            AppLogger.Instance.Log(LogSource, $"Could not decode glyph U+{codePoint:X4}: {ex.Message}",
                AppLogLevel.Error);
        }
    }

    private void QueueSampleRender()
    {
        CancelSampleRender();
        WftFont? font = _font;
        if (font is null) return;
        var cancellation = new CancellationTokenSource();
        _sampleCancellation = cancellation;
        string text = _sampleText.Text;
        int scale = _sampleScale;
        _ = RenderSampleAsync(font, text, scale, cancellation.Token);
    }

    private void CancelSampleRender()
    {
        CancellationTokenSource? cancellation =
            Interlocked.Exchange(ref _sampleCancellation, null);
        if (cancellation is null) return;
        try
        {
            cancellation.Cancel();
        }
        finally
        {
            cancellation.Dispose();
        }
    }

    private async Task RenderSampleAsync(WftFont font, string text, int scale,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(150, cancellationToken);
            Bitmap bitmap = await Task.Run(() => WftGlyphRenderer.RenderText(font, text, scale,
                Color.White, Color.FromArgb(20, 22, 25), cancellationToken), cancellationToken);
            if (cancellationToken.IsCancellationRequested || !ReferenceEquals(font, _font))
            {
                bitmap.Dispose();
                return;
            }
            _samplePreview.Image = bitmap;
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) when (ex is IOException or ObjectDisposedException or
                                   InvalidDataException or OverflowException)
        {
            AppLogger.Instance.Log(LogSource, $"Could not render WFT sample: {ex.Message}",
                AppLogLevel.Error);
        }
    }

    private void JumpToCodePoint()
    {
        WftFont? font = _font;
        string value = _jumpText.Text.Trim();
        if (value.StartsWith("U+", StringComparison.OrdinalIgnoreCase)) value = value[2..];
        if (font is null ||
            !ushort.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture,
                out ushort codePoint) ||
            codePoint < WftFont.FirstCodePoint || codePoint > font.LastCodePoint)
        {
            _statusLabel.Text = Strings.WftViewer_InvalidCodePoint;
            return;
        }
        _glyphGrid.SelectCodePoint(codePoint);
    }

    private void ChangeSampleScale(int delta)
    {
        _sampleScale = Math.Clamp(_sampleScale + delta, 1, 16);
        _zoomValue.Text = $"{_sampleScale}×";
        QueueSampleRender();
    }

    private void FrmWftViewer_DragEnter(object? sender, DragEventArgs e)
    {
        e.Effect = TryGetDroppedFile(e.Data, out _) ? DragDropEffects.Copy : DragDropEffects.None;
    }

    private static bool TryGetDroppedFile(IDataObject? data, out string path)
    {
        path = string.Empty;
        if (data?.GetData(DataFormats.FileDrop) is not string[] { Length: 1 } files ||
            !files[0].EndsWith(".wft", StringComparison.OrdinalIgnoreCase))
            return false;
        path = files[0];
        return true;
    }

    private void LocalizationManager_CultureChanged(object? sender, EventArgs e)
    {
        if (IsDisposed || Disposing) return;
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        Text = _currentPath is null
            ? Strings.WftViewer_Title
            : $"{Strings.WftViewer_Title} — {Path.GetFileName(_currentPath)}";
        _openButton.Text = Strings.WftViewer_Open;
        _reloadButton.Text = Strings.UiEditor_Reload;
        _jumpLabel.Text = Strings.WftViewer_Jump;
        _jumpButton.Text = Strings.WftViewer_Go;
        _zoomLabel.Text = Strings.WftViewer_SampleZoom;
        _zoomValue.Text = $"{_sampleScale}×";
        Controls.Find("lblWftSample", true).OfType<Label>().Single().Text =
            Strings.WftViewer_SampleText;
        if (!_sampleInitialized)
        {
            _sampleInitialized = true;
            _sampleText.Text = Strings.WftViewer_DefaultSample;
        }
        if (_font is null)
        {
            _statusLabel.Text = Strings.WftViewer_Ready;
        }
        else
        {
            UpdateMetadata();
            _statusLabel.Text = string.Format(LocalizationManager.CurrentCulture,
                Strings.WftViewer_LoadedFormat, Path.GetFileName(_font.Path));
            if (_glyphGrid.SelectedCodePoint is ushort codePoint)
                _ = SelectGlyphAsync(codePoint);
        }
    }

    private void UpdateMetadata()
    {
        if (_font is null)
        {
            _metadata.Text = string.Empty;
            return;
        }
        _metadata.Text = string.Format(LocalizationManager.CurrentCulture,
            Strings.WftViewer_MetadataFormat, _font.CellSize,
            _font.CoverageMode == WftCoverageMode.Antialiased ? 4 : 1,
            _font.GlyphCount, _font.Reserved);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
            _loadGeneration++;
            CancelSampleRender();
            _glyphGrid.LoadFont(null);
            _font?.Dispose();
            _font = null;
        }
        base.Dispose(disposing);
    }
}
