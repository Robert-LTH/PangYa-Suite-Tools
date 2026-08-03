using PangYa_Suite_Tools.Localization;
using PangYa_Suite_Tools.Logging;
using PangyaAPI.UI;
using System.ComponentModel;
using System.Xml;

namespace PangYa_Suite_Tools.Shop;

internal sealed class FrmPangyaUiEditor : Form
{
    private const string LogSource = "UI Editor";
    private readonly string _dataRoot;
    private readonly IReadOnlyList<string> _uiFiles;
    private readonly PangyaFileImageProvider _assets;
    private readonly ToolStrip _toolbar = new();
    private readonly ToolStripComboBox _fileSelector = new();
    private readonly ToolStripButton _openButton = new();
    private readonly ToolStripButton _saveButton = new();
    private readonly ToolStripButton _reloadButton = new();
    private readonly ToolStripLabel _stateLabel = new();
    private readonly ToolStripComboBox _stateSelector = new();
    private readonly ToolStripButton _debugButton = new();
    private readonly ToolStripButton _zoomOutButton = new();
    private readonly ToolStripLabel _zoomLabel = new();
    private readonly ToolStripButton _zoomInButton = new();
    private readonly ToolStripSeparator _ifdefSeparator = new() { Visible = false };
    private readonly ToolStripLabel _ifdefLabel = new() { Visible = false };
    private readonly List<ToolStripControlHost> _ifdefControls = [];
    private readonly HashSet<string> _enabledSymbols = new(StringComparer.OrdinalIgnoreCase);
    private readonly TreeView _elementTree = new();
    private readonly PangyaUiCanvas _canvas;
    private readonly PangyaUiPropertyPanel _properties;
    private readonly ToolStripStatusLabel _statusLabel = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly StatusStrip _status = new();
    private PangyaUiDocument? _document;
    private string? _currentPath;
    private bool _dirty;
    private bool _changingFile;

    private FrmPangyaUiEditor(string dataRoot, IReadOnlyList<string> uiFiles,
        PangyaFileImageProvider assets)
        : this(dataRoot, uiFiles, assets, PangyaUiResourceCatalog.Empty)
    {
    }

    private FrmPangyaUiEditor(string dataRoot, IReadOnlyList<string> uiFiles,
        PangyaFileImageProvider assets,
        PangyaUiResourceCatalog resourceCatalog)
    {
        _dataRoot = dataRoot;
        _uiFiles = uiFiles;
        _assets = assets;
        _canvas = new PangyaUiCanvas(assets, resourceCatalog) { Dock = DockStyle.None };
        _properties = new PangyaUiPropertyPanel(assets);

        Name = "frmPangyaUiEditor";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(1280, 800);
        MinimumSize = new Size(960, 640);
        KeyPreview = true;

        ConfigureToolbar();
        ConfigureWorkspace();
        _status.Items.Add(_statusLabel);
        Controls.Add(_status);
        Controls.Add(_toolbar);

        _canvas.SelectionChanged += (_, node) => SelectNode(node, fromCanvas: true);
        _canvas.ElementChanged += (_, e) =>
        {
            MarkDirty();
            _properties.SelectedNode = e.Node;
            RefreshSelectedTreeNode();
        };
        _properties.ElementChanged += (_, _) =>
        {
            MarkDirty();
            RefreshSelectedTreeNode();
            _canvas.Invalidate();
        };
        _elementTree.AfterSelect += (_, e) =>
        {
            if (e.Node?.Tag is PangyaUiNode node) SelectNode(node, fromCanvas: false);
        };
        FormClosing += FrmPangyaUiEditor_FormClosing;
        LocalizationManager.CultureChanged += LocalizationManager_CultureChanged;
        Disposed += (_, _) =>
        {
            LocalizationManager.CultureChanged -= LocalizationManager_CultureChanged;
            _assets.Dispose();
        };
        ApplyLocalization();
        PopulateFileSelector();
    }

    public static async Task<FrmPangyaUiEditor> CreateAsync(string dataRoot,
        CancellationToken cancellationToken = default)
    {
        string fullRoot = Path.GetFullPath(dataRoot);
        (IReadOnlyList<string> files, PangyaFileImageProvider assets,
            PangyaUiResourceCatalog catalog) =
            await Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<string> discovered = PangyaUiDocument.FindUiFiles(fullRoot);
            if (discovered.Count == 0) throw new InvalidDataException(Strings.UiEditor_NoXmlFiles);
            var resolver = new PangyaFileImageProvider(fullRoot);
            PangyaUiResourceCatalog catalog = PangyaUiResourceCatalog.Load(discovered);
            return (discovered, resolver, catalog);
        }, cancellationToken);
        var editor = new FrmPangyaUiEditor(fullRoot, files, assets, catalog);
        try
        {
            string initial = files.FirstOrDefault(path =>
                Path.GetFileName(path).Equals("shop.xml",
                    StringComparison.OrdinalIgnoreCase)) ?? files[0];
            await editor.LoadDocumentAsync(initial, cancellationToken);
            return editor;
        }
        catch
        {
            editor.Dispose();
            throw;
        }
    }

    private void ConfigureToolbar()
    {
        _toolbar.Name = "uiEditorToolbar";
        _toolbar.Dock = DockStyle.Top;
        _toolbar.GripStyle = ToolStripGripStyle.Hidden;
        _toolbar.Padding = new Padding(4, 2, 4, 2);
        _toolbar.Height = 32;

        _fileSelector.Name = "cboUiFiles";
        _fileSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        _fileSelector.AutoSize = false;
        _fileSelector.Width = 300;
        _fileSelector.SelectedIndexChanged += async (_, _) =>
        {
            if (_changingFile || _fileSelector.SelectedItem is not UiFileOption option) return;
            if (!ConfirmDiscard())
            {
                SelectCurrentFile();
                return;
            }
            await LoadDocumentAsync(option.Path, CancellationToken.None);
        };

        _openButton.Name = "btnOpenUiXml";
        _openButton.Click += async (_, _) => await BrowseForXmlAsync();
        _saveButton.Name = "btnSaveUiXml";
        _saveButton.Click += async (_, _) => await SaveAsync();
        _reloadButton.Name = "btnReloadUiXml";
        _reloadButton.Click += async (_, _) =>
        {
            if (_currentPath is null || !ConfirmDiscard()) return;
            await LoadDocumentAsync(_currentPath, CancellationToken.None);
        };
        _debugButton.Name = "btnDebugUiBounds";
        _debugButton.CheckOnClick = true;
        _debugButton.CheckedChanged += (_, _) =>
        {
            _canvas.ShowDebugBounds = _debugButton.Checked;
            _canvas.Invalidate();
        };
        _stateSelector.Name = "cboUiButtonState";
        _stateSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        _stateSelector.AutoSize = false;
        _stateSelector.Width = 110;
        _stateSelector.SelectedIndexChanged += (_, _) =>
        {
            _canvas.ButtonState = (PangyaUiButtonState)Math.Max(0, _stateSelector.SelectedIndex);
            _canvas.Invalidate();
        };
        _zoomOutButton.Text = "−";
        _zoomOutButton.Click += (_, _) => ChangeZoom(-0.1f);
        _zoomInButton.Text = "+";
        _zoomInButton.Click += (_, _) => ChangeZoom(0.1f);

        _toolbar.Items.AddRange([
            _fileSelector, new ToolStripSeparator(),
            _openButton, _saveButton, _reloadButton, new ToolStripSeparator(),
            _stateLabel, _stateSelector, _debugButton, new ToolStripSeparator(),
            _zoomOutButton, _zoomLabel, _zoomInButton,
            _ifdefSeparator, _ifdefLabel
        ]);
    }

    private void ConfigureWorkspace()
    {
        var canvasHost = new Panel
        {
            Name = "pnlUiCanvasHost",
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.FromArgb(30, 32, 36)
        };
        canvasHost.Controls.Add(_canvas);
        canvasHost.ClientSizeChanged += (_, _) => _canvas.ViewportSize = canvasHost.ClientSize;
        _canvas.ViewportSize = canvasHost.ClientSize;

        var rightSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel2,
            Size = new Size(900, 700),
            SplitterDistance = 650,
            Panel2MinSize = 260
        };
        rightSplit.Panel1.Controls.Add(canvasHost);
        rightSplit.Panel2.Controls.Add(_properties);

        _elementTree.Name = "treeUiElements";
        _elementTree.Dock = DockStyle.Fill;
        _elementTree.HideSelection = false;

        var mainSplit = new SplitContainer
        {
            Name = "splitUiEditor",
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            Size = new Size(1200, 700),
            SplitterDistance = 280,
            Panel1MinSize = 220
        };
        mainSplit.Panel1.Controls.Add(_elementTree);
        mainSplit.Panel2.Controls.Add(rightSplit);
        mainSplit.Location = new Point(0, _toolbar.Height);

        Controls.Add(mainSplit);
        mainSplit.BringToFront();
    }

    private void PopulateFileSelector()
    {
        _changingFile = true;
        try
        {
            _fileSelector.Items.Clear();
            foreach (string path in _uiFiles)
                _fileSelector.Items.Add(new UiFileOption(
                    Path.GetRelativePath(Path.Combine(_dataRoot, "ui"), path), path));
            SelectCurrentFile();
        }
        finally { _changingFile = false; }
    }

    private async Task BrowseForXmlAsync()
    {
        using var dialog = new OpenFileDialog
        {
            Title = Strings.UiEditor_OpenXml,
            Filter = Strings.UiEditor_XmlFilter,
            InitialDirectory = Path.Combine(_dataRoot, "ui"),
            CheckFileExists = true
        };
        if (dialog.ShowDialog(this) != DialogResult.OK || !ConfirmDiscard()) return;
        await LoadDocumentAsync(dialog.FileName, CancellationToken.None);
    }

    private async Task LoadDocumentAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            UseWaitCursor = true;
            _toolbar.Enabled = false;
            PangyaUiDocument document = await Task.Run(() => PangyaUiDocument.Load(path), cancellationToken);
            _document = document;
            _currentPath = document.Path;
            _dirty = false;
            ConfigureIfdefControls(document);
            _canvas.LoadDocument(document);
            _canvas.EnabledSymbols = _enabledSymbols;
            BuildElementTree();
            SelectCurrentFile();
            UpdateTitle();
            _statusLabel.Text = string.Format(LocalizationManager.CurrentCulture,
                Strings.UiEditor_LoadedFormat, Path.GetFileName(path), document.Nodes.Count);
            AppLogger.Instance.Log(LogSource, $"Loaded PangYa UI XML '{path}' with {document.Nodes.Count} elements.");
            foreach (string warning in document.Warnings)
                AppLogger.Instance.Log(LogSource, warning, AppLogLevel.Warning);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or XmlException
                                   or InvalidDataException or ArgumentException)
        {
            AppLogger.Instance.Log(LogSource, $"Could not load PangYa UI XML: {ex.Message}", AppLogLevel.Error);
            MessageBox.Show(this, ex.Message, Strings.UiEditor_Title,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            _toolbar.Enabled = true;
            UpdateActionState();
        }
    }

    private async Task SaveAsync()
    {
        if (_document is null || !_dirty) return;
        try
        {
            UseWaitCursor = true;
            _saveButton.Enabled = false;
            await _document.SaveAtomicAsync();
            _dirty = false;
            UpdateTitle();
            UpdateActionState();
            _statusLabel.Text = Strings.UiEditor_Saved;
            AppLogger.Instance.Log(LogSource, $"Saved PangYa UI XML '{_document.Path}'.");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            AppLogger.Instance.Log(LogSource, $"Could not save PangYa UI XML: {ex.Message}", AppLogLevel.Error);
            MessageBox.Show(this, ex.Message, Strings.UiEditor_Title,
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            UpdateActionState();
        }
    }

    private void BuildElementTree(PangyaUiNode? preferredSelection = null)
    {
        _elementTree.BeginUpdate();
        try
        {
            _elementTree.Nodes.Clear();
            if (_document is null) return;
            foreach (PangyaUiNode node in _document.GetVisibleRoots(_enabledSymbols))
                _elementTree.Nodes.Add(CreateTreeNode(node));
            _elementTree.ExpandAll();
            TreeNode? preferredTreeNode = preferredSelection is null
                ? null
                : FindTreeNode(_elementTree.Nodes, preferredSelection);
            if (preferredTreeNode is not null)
                _elementTree.SelectedNode = preferredTreeNode;
            else if (_elementTree.Nodes.Count > 0)
                _elementTree.SelectedNode = _elementTree.Nodes[0];
            else
                SelectNode(null, fromCanvas: false);
        }
        finally { _elementTree.EndUpdate(); }
    }

    private TreeNode CreateTreeNode(PangyaUiNode node)
    {
        var treeNode = new TreeNode(node.DisplayName) { Tag = node };
        foreach (PangyaUiNode child in node.Children.Where(child => child.IsVisible(_enabledSymbols)))
            treeNode.Nodes.Add(CreateTreeNode(child));
        return treeNode;
    }

    private void ConfigureIfdefControls(PangyaUiDocument document)
    {
        foreach (ToolStripControlHost host in _ifdefControls)
        {
            _toolbar.Items.Remove(host);
            host.Dispose();
        }
        _ifdefControls.Clear();
        _enabledSymbols.Clear();

        foreach (string symbol in document.ConditionalSymbols.OrderBy(value => value,
                     StringComparer.OrdinalIgnoreCase))
        {
            var checkBox = new CheckBox
            {
                Name = "chkUiIfdef_" + symbol,
                Text = symbol,
                AutoSize = true,
                Margin = Padding.Empty,
                Checked = false
            };
            checkBox.CheckedChanged += (_, _) =>
            {
                if (checkBox.Checked) _enabledSymbols.Add(symbol);
                else _enabledSymbols.Remove(symbol);
                RefreshConditionalView();
            };
            var host = new ToolStripControlHost(checkBox)
            {
                Name = "hostUiIfdef_" + symbol,
                AutoSize = true
            };
            _ifdefControls.Add(host);
            _toolbar.Items.Add(host);
        }

        bool hasConditions = _ifdefControls.Count > 0;
        _ifdefSeparator.Visible = hasConditions;
        _ifdefLabel.Visible = hasConditions;
    }

    private void RefreshConditionalView()
    {
        if (_document is null) return;
        PangyaUiNode? previous = _canvas.SelectedNode;
        PangyaUiNode? target = previous is not null && _document.IsVisible(previous, _enabledSymbols)
            ? previous
            : previous?.FindContainingForm();
        if (target is not null && !_document.IsVisible(target, _enabledSymbols)) target = null;

        _canvas.EnabledSymbols = _enabledSymbols;
        BuildElementTree(target);
        _canvas.Invalidate();
    }

    private void SelectNode(PangyaUiNode? node, bool fromCanvas)
    {
        _canvas.SelectedForm = node?.FindContainingForm();
        _canvas.SelectedNode = node;
        _properties.SelectedNode = node;
        if (fromCanvas && node is not null)
        {
            TreeNode? treeNode = FindTreeNode(_elementTree.Nodes, node);
            if (treeNode is not null) _elementTree.SelectedNode = treeNode;
        }
    }

    private static TreeNode? FindTreeNode(TreeNodeCollection nodes, PangyaUiNode target)
    {
        foreach (TreeNode node in nodes)
        {
            if (ReferenceEquals(node.Tag, target)) return node;
            TreeNode? nested = FindTreeNode(node.Nodes, target);
            if (nested is not null) return nested;
        }
        return null;
    }

    private void RefreshSelectedTreeNode()
    {
        if (_elementTree.SelectedNode?.Tag is PangyaUiNode node)
            _elementTree.SelectedNode.Text = node.DisplayName;
    }

    private void MarkDirty()
    {
        if (_document is null) return;
        _dirty = true;
        UpdateTitle();
        UpdateActionState();
    }

    private bool ConfirmDiscard() => !_dirty ||
        MessageBox.Show(this, Strings.IFFManager_DiscardChanges, Strings.UiEditor_Title,
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes;

    private void FrmPangyaUiEditor_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!ConfirmDiscard()) e.Cancel = true;
    }

    private void SelectCurrentFile()
    {
        _changingFile = true;
        try
        {
            _fileSelector.SelectedItem = _fileSelector.Items.Cast<UiFileOption>()
                .FirstOrDefault(option => option.Path.Equals(_currentPath, StringComparison.OrdinalIgnoreCase));
        }
        finally { _changingFile = false; }
    }

    private void ChangeZoom(float delta)
    {
        _canvas.Zoom = Math.Clamp(_canvas.Zoom + delta, 0.25f, 3f);
        _zoomLabel.Text = $"{_canvas.Zoom:P0}";
    }

    private void LocalizationManager_CultureChanged(object? sender, EventArgs e)
    {
        if (IsDisposed || Disposing) return;
        ApplyLocalization();
    }

    private void ApplyLocalization()
    {
        _openButton.Text = Strings.UiEditor_OpenXml;
        _saveButton.Text = Strings.IFFManager_Save;
        _reloadButton.Text = Strings.UiEditor_Reload;
        _debugButton.Text = Strings.UiEditor_DebugBounds;
        _ifdefLabel.Text = Strings.UiEditor_Defines;
        _stateLabel.Text = Strings.UiEditor_ButtonState;
        int state = Math.Max(0, _stateSelector.SelectedIndex);
        _stateSelector.Items.Clear();
        _stateSelector.Items.AddRange([
            Strings.UiEditor_StateNormal,
            Strings.UiEditor_StateHover,
            Strings.UiEditor_StateSelected
        ]);
        _stateSelector.SelectedIndex = Math.Min(state, _stateSelector.Items.Count - 1);
        _zoomLabel.Text = $"{_canvas.Zoom:P0}";
        _properties.ApplyLocalization();
        if (_document is null) _statusLabel.Text = Strings.UiEditor_Ready;
        UpdateTitle();
    }

    private void UpdateTitle() =>
        Text = Strings.UiEditor_Title + (_currentPath is null ? string.Empty : $" — {Path.GetFileName(_currentPath)}") +
               (_dirty ? " *" : string.Empty);

    private void UpdateActionState()
    {
        _saveButton.Enabled = _document is not null && _dirty && !UseWaitCursor;
        _reloadButton.Enabled = _document is not null && !UseWaitCursor;
    }

    private sealed record UiFileOption(string Label, string Path)
    {
        public override string ToString() => Label;
    }
}

internal sealed class PangyaUiCanvas : Control
{
    internal const int FormPadding = 16;
    private readonly PangyaUiRenderer _renderer;
    private HashSet<string> _enabledSymbols = new(StringComparer.OrdinalIgnoreCase);
    private PangyaUiDocument? _document;
    private PangyaUiNode? _selectedForm;
    private PangyaUiNode? _selectedNode;
    private PangyaUiNode? _draggedNode;
    private Point? _dragStart;
    private Rectangle _dragStartBounds;
    private Size _viewportSize;
    private float _zoom = 1f;

    public PangyaUiCanvas(IPangyaImageProvider assets,
        PangyaUiResourceCatalog? resourceCatalog = null)
    {
        _renderer = new PangyaUiRenderer(assets, resourceCatalog);
        DoubleBuffered = true;
        BackColor = Color.FromArgb(36, 39, 44);
        Cursor = Cursors.Default;
        MouseDown += CanvasMouseDown;
        MouseMove += CanvasMouseMove;
        MouseUp += (_, _) =>
        {
            _draggedNode = null;
            _dragStart = null;
            Capture = false;
        };
    }

    public event EventHandler<PangyaUiNode?>? SelectionChanged;
    public event EventHandler<PangyaUiNodeEventArgs>? ElementChanged;
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ShowDebugBounds { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PangyaUiButtonState ButtonState { get; set; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;
            UpdateCanvasSize();
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PangyaUiNode? SelectedForm
    {
        get => _selectedForm;
        set
        {
            _selectedForm = value;
            if (_selectedNode is not null && value is not null && !_selectedNode.IsWithin(value))
                _selectedNode = null;
            UpdateCanvasSize();
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PangyaUiNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal IReadOnlySet<string> EnabledSymbols
    {
        get => _enabledSymbols;
        set
        {
            _enabledSymbols = new HashSet<string>(value, StringComparer.OrdinalIgnoreCase);
            if (_selectedNode is not null && _document is not null &&
                !_document.IsVisible(_selectedNode, _enabledSymbols))
                _selectedNode = null;
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal Size ViewportSize
    {
        get => _viewportSize;
        set
        {
            _viewportSize = new Size(Math.Max(0, value.Width), Math.Max(0, value.Height));
            UpdateCanvasSize();
        }
    }

    public void LoadDocument(PangyaUiDocument document)
    {
        _document = document;
        _selectedForm = null;
        _selectedNode = null;
        UpdateCanvasSize();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.Clear(Color.FromArgb(24, 26, 30));
        if (_document is null) return;
        using var transform = new System.Drawing.Drawing2D.Matrix(
            Zoom, 0f, 0f, Zoom, FormPadding * Zoom, FormPadding * Zoom);
        e.Graphics.Transform = transform;
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        _renderer.Render(e.Graphics, _document, SelectedForm, RenderOptions());
    }

    internal Rectangle GetRenderedBounds(PangyaUiNode node)
        => _renderer.GetRenderedBounds(node, RenderOptions());

    internal void CanvasMouseDown(object? sender, MouseEventArgs e)
    {
        if (_document is null || e.Button != MouseButtons.Left) return;
        Point logical = LogicalPoint(e.Location);
        PangyaUiNode? hit = _renderer.HitTest(_document, SelectedForm, logical,
            RenderOptions());
        _draggedNode = null;
        _dragStart = null;
        Capture = false;
        if (hit is null)
            return;
        SelectedNode = hit;
        SelectionChanged?.Invoke(this, hit);
        if (hit.IsEditable)
        {
            _draggedNode = hit;
            _dragStart = logical;
            _dragStartBounds = hit.Bounds;
            Capture = true;
        }
    }

    internal void CanvasMouseMove(object? sender, MouseEventArgs e)
    {
        if (_dragStart is not Point start || _draggedNode is null || e.Button != MouseButtons.Left) return;
        Point logical = LogicalPoint(e.Location);
        int dx = logical.X - start.X;
        int dy = logical.Y - start.Y;
        _draggedNode.SetBounds(new Rectangle(
            _dragStartBounds.X + dx, _dragStartBounds.Y + dy,
            _dragStartBounds.Width, _dragStartBounds.Height));
        ElementChanged?.Invoke(this, new PangyaUiNodeEventArgs(_draggedNode));
        Invalidate();
    }

    internal Point LogicalPoint(Point point) =>
        new((int)Math.Floor(point.X / Zoom - FormPadding),
            (int)Math.Floor(point.Y / Zoom - FormPadding));

    private void UpdateCanvasSize()
    {
        if (_document is null) return;
        Size logicalSize = _selectedForm?.Bounds.Size is { Width: > 0, Height: > 0 } formSize
            ? formSize
            : _document.CanvasSize;
        int contentWidth = (int)Math.Ceiling((logicalSize.Width + FormPadding * 2) * Zoom);
        int contentHeight = (int)Math.Ceiling((logicalSize.Height + FormPadding * 2) * Zoom);
        Size = new Size(
            Math.Max(Math.Max(1, contentWidth), _viewportSize.Width),
            Math.Max(Math.Max(1, contentHeight), _viewportSize.Height));
    }

    internal IReadOnlyList<PangyaUiNode> RenderOrderedNodes()
        => _document is null ? [] : _renderer.GetRenderOrder(_document, SelectedForm,
            SelectedNode, _enabledSymbols);

    private PangyaUiRenderOptions RenderOptions() => new(
        ButtonState, _enabledSymbols, ShowDebugBounds, SelectedNode, 1f / Zoom);
}

internal sealed class PangyaUiNodeEventArgs(PangyaUiNode node) : EventArgs
{
    public PangyaUiNode Node { get; } = node;
}

internal sealed class PangyaUiPropertyPanel : UserControl
{
    private readonly PangyaFileImageProvider _assets;
    private readonly GroupBox _group = new() { Dock = DockStyle.Fill };
    private readonly TableLayoutPanel _layout = new()
    {
        Dock = DockStyle.Top,
        AutoSize = true,
        ColumnCount = 2,
        Padding = new Padding(8)
    };
    private readonly Label _nameLabel = new();
    private readonly Label _typeLabel = new();
    private readonly Label _xLabel = new();
    private readonly Label _yLabel = new();
    private readonly Label _widthLabel = new();
    private readonly Label _heightLabel = new();
    private readonly Label _resourceLabel = new();
    private readonly TextBox _name = new();
    private readonly TextBox _type = new();
    private readonly NumericUpDown _x = Numeric();
    private readonly NumericUpDown _y = Numeric();
    private readonly NumericUpDown _width = Numeric(nonNegative: true);
    private readonly NumericUpDown _height = Numeric(nonNegative: true);
    private readonly TextBox _resource = new();
    private readonly Button _resourceBrowse = new() { Text = "…", Dock = DockStyle.Fill };
    private readonly TableLayoutPanel _resourceEditor = new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        ColumnCount = 2,
        Margin = Padding.Empty
    };
    private PangyaUiNode? _selectedNode;
    private bool _loading;

    public PangyaUiPropertyPanel(PangyaFileImageProvider assets)
    {
        _assets = assets;
        Name = "pnlUiProperties";
        Dock = DockStyle.Fill;
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _resourceEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        _resourceEditor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 34));
        _resourceEditor.Controls.Add(_resource, 0, 0);
        _resourceEditor.Controls.Add(_resourceBrowse, 1, 0);
        AddRow(_nameLabel, _name, 0);
        AddRow(_typeLabel, _type, 1);
        AddRow(_xLabel, _x, 2);
        AddRow(_yLabel, _y, 3);
        AddRow(_widthLabel, _width, 4);
        AddRow(_heightLabel, _height, 5);
        AddRow(_resourceLabel, _resourceEditor, 6);
        _group.Controls.Add(_layout);
        Controls.Add(_group);

        _name.TextChanged += (_, _) => ApplyName();
        _type.TextChanged += (_, _) => ApplyType();
        _resource.TextChanged += (_, _) => ApplyResource();
        _x.ValueChanged += (_, _) => ApplyBounds();
        _y.ValueChanged += (_, _) => ApplyBounds();
        _width.ValueChanged += (_, _) => ApplyBounds();
        _height.ValueChanged += (_, _) => ApplyBounds();
        _resourceBrowse.Click += (_, _) => BrowseForResource();
    }

    public event EventHandler? ElementChanged;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PangyaUiNode? SelectedNode
    {
        get => _selectedNode;
        set
        {
            _selectedNode = value;
            LoadSelectedNode();
        }
    }

    public void ApplyLocalization()
    {
        UpdateGroupText();
        _nameLabel.Text = Strings.UiEditor_Name;
        _typeLabel.Text = Strings.UiEditor_Type;
        _xLabel.Text = "X";
        _yLabel.Text = "Y";
        _widthLabel.Text = Strings.UiEditor_Width;
        _heightLabel.Text = Strings.UiEditor_Height;
        _resourceLabel.Text = Strings.UiEditor_Resource;
        _resourceBrowse.AccessibleName = Strings.UiEditor_SelectResource;
    }

    internal Point DisplayedLocation =>
        new(decimal.ToInt32(_x.Value), decimal.ToInt32(_y.Value));

    internal string GetResourceInitialDirectory()
    {
        string? resolvedPath = _assets.TryResolvePath(_resource.Text);
        string? resolvedDirectory = Path.GetDirectoryName(resolvedPath);
        return !string.IsNullOrWhiteSpace(resolvedDirectory) && Directory.Exists(resolvedDirectory)
            ? resolvedDirectory
            : _assets.DataRoot;
    }

    internal bool TrySetResourceFromPath(string selectedPath)
    {
        string fullPath = Path.GetFullPath(selectedPath);
        string relativePath = Path.GetRelativePath(_assets.DataRoot, fullPath);
        if (Path.IsPathRooted(relativePath) ||
            relativePath.Equals("..", StringComparison.Ordinal) ||
            relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) ||
            relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal))
            return false;

        _resource.Text = Path.GetFileName(fullPath);
        return true;
    }

    private void LoadSelectedNode()
    {
        _loading = true;
        try
        {
            bool enabled = _selectedNode?.IsEditable == true;
            _layout.Enabled = enabled;
            _name.Text = _selectedNode?.Name ?? string.Empty;
            _type.Text = _selectedNode?.Type ?? string.Empty;
            _resource.Text = _selectedNode?.GetResource(PangyaUiButtonState.Normal) ?? string.Empty;
            Rectangle bounds = _selectedNode?.Bounds ?? Rectangle.Empty;
            _x.Value = Clamp(_x, bounds.X);
            _y.Value = Clamp(_y, bounds.Y);
            _width.Value = Clamp(_width, bounds.Width);
            _height.Value = Clamp(_height, bounds.Height);
            UpdateGroupText();
        }
        finally { _loading = false; }
    }

    private void UpdateGroupText()
    {
        _group.Text = _selectedNode?.IsPreviewOnly == true
            ? $"{Strings.UiEditor_Properties} — {Strings.UiEditor_ConditionalPreview}"
            : Strings.UiEditor_Properties;
    }

    private void ApplyName()
    {
        if (_loading || _selectedNode is null) return;
        _selectedNode.SetName(_name.Text);
        ElementChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyType()
    {
        if (_loading || _selectedNode is null) return;
        _selectedNode.SetType(_type.Text);
        ElementChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyResource()
    {
        if (_loading || _selectedNode is null) return;
        _selectedNode.SetResource(_resource.Text);
        ElementChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BrowseForResource()
    {
        using OpenFileDialog dialog = FileDialogFactory.CreateUiResourceOpenDialog(GetResourceInitialDirectory());
        if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;
        FileDialogFactory.RememberDirectory(FileDialogKind.UiResource, dialog.FileName);
        if (!TrySetResourceFromPath(dialog.FileName))
            MessageBox.Show(FindForm(), Strings.UiEditor_ResourceMustBeInData, Strings.UiEditor_Title,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private void ApplyBounds()
    {
        if (_loading || _selectedNode is null) return;
        _selectedNode.SetBounds(new Rectangle(
            decimal.ToInt32(_x.Value), decimal.ToInt32(_y.Value),
            decimal.ToInt32(_width.Value), decimal.ToInt32(_height.Value)));
        ElementChanged?.Invoke(this, EventArgs.Empty);
    }

    private void AddRow(Label label, Control editor, int row)
    {
        label.Dock = DockStyle.Fill;
        label.TextAlign = ContentAlignment.MiddleLeft;
        editor.Dock = DockStyle.Fill;
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(label, 0, row);
        _layout.Controls.Add(editor, 1, row);
    }

    private static NumericUpDown Numeric(bool nonNegative = false) => new()
    {
        Minimum = nonNegative ? 0 : -8192,
        Maximum = 8192
    };

    private static decimal Clamp(NumericUpDown control, int value) =>
        Math.Clamp((decimal)value, control.Minimum, control.Maximum);
}
