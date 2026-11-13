using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Xml;

namespace PangyaAPI.UI;

public interface IPangyaUiBitmapTextPasses
{
    float GetDrawAdvance(string text);
    RectangleF GetOverlayBounds(string text, PointF origin, float fontScale = 1) => RectangleF.Empty;
    void DrawOverlays(Graphics graphics, string text, Brush brush, PointF origin, float fontScale = 1);
    void DrawGlyphs(Graphics graphics, string text, Brush brush, PointF origin);
}

public sealed record PangyaUiFrameText(string? Caption = null, string? Description = null,
    bool UseCaptionOffset = false, bool UseDescriptionEmoticons = true,
    bool UseCaptionFocus = true, byte Opacity = byte.MaxValue);

public sealed record PangyaUiCursorPreview(Point Position, int Index, int Frame = 0);

public sealed record PangyaUiTooltipPreview(string Text, int Style = 0, Point? Position = null);

public sealed record PangyaUiResourceInstance(PangyaUiNode Host, PangyaUiResourceInstance? Parent);

public readonly record struct PangyaUiTabInstance(PangyaUiNode Node, PangyaUiResourceInstance? Resource);

public enum PangyaUiWindowLifecycle
{
    Initialized,
    PendingInitialization,
    PendingDestruction
}

public sealed record PangyaUiTab(string Normal, string? Hover = null, string? Selected = null,
    PangyaUiButtonState State = PangyaUiButtonState.Normal, Size? Size = null)
{
    public uint? Identifier { get; init; }
    public string? Caption { get; init; }
    public float? LayoutWidth { get; init; }

    public static PangyaUiTab FromImagePrefix(string prefix, uint identifier, Size? size = null)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        return new PangyaUiTab(prefix + "_n", prefix + "_o", prefix + "_d", Size: size)
        {
            Identifier = identifier
        };
    }
}

public readonly record struct PangyaUiListItemState(int Index, int VisibleIndex, Rectangle Bounds,
    bool UnderCursor, bool Selected)
{
    public bool IsDummy { get; init; }
}

public sealed record PangyaUiListPreview(int ItemCount, int TopRow,
    Action<Graphics, int, Rectangle> DrawItem)
{
    public Action<Graphics, Rectangle>? DrawBackground { get; init; }
    public Action<Graphics, PangyaUiListItemState>? DrawItemState { get; init; }
    public IReadOnlySet<int>? SelectedItems { get; init; }
    public IReadOnlySet<int>? DummyItems { get; init; }
    public bool ExcludeDummyItemsFromHitTesting { get; init; }
    public int? ItemUnderCursor { get; init; }
    public IReadOnlySet<int>? UnderCursorItems { get; init; }
    public bool MouseEventsEnabled { get; init; } = true;
    public Size? ItemSize { get; init; }
    public int? ColumnCapacity { get; init; }
    public int? TopItem { get; init; }
    public float? ScrollThumbRow { get; init; }
    public bool FollowBottom { get; init; }
    public bool ScrollbarAtLeft { get; init; }
    public bool? ShowScrollbar { get; init; }
}

public sealed record PangyaUiMenuState(int SelectedRow = -1,
    IReadOnlySet<int>? DisabledRows = null, byte SelectionAlpha = byte.MaxValue)
{
    public IReadOnlySet<string>? DisabledTexts { get; init; }
    public IReadOnlyDictionary<string, string>? TextReplacements { get; init; }
    public byte Opacity { get; init; } = byte.MaxValue;
}

public sealed record PangyaUiImageButtonState(bool Enabled = true,
    double SecondsSincePress = double.PositiveInfinity, bool Blink = false)
{
    public int? Style { get; init; }
    public double? PushDelay { get; init; }
    public int? PushStyle { get; init; }
    public IReadOnlyDictionary<string, Image?>? BitmapImages { get; init; }
    public PangyaUiButtonState? Status { get; init; }
    public string? ReplacementMode { get; init; }
    public string? ReplacementImage { get; init; }
    public IReadOnlyDictionary<string, string>? Images { get; init; }
    public IReadOnlyDictionary<string, string?>? BelowImages { get; init; }
}

public sealed record PangyaUiGaugeRange(int Lower, int Upper, int Origin = 0);
public sealed record PangyaUiGaugeAppearance(Color? FillColor = null, int? BorderThickness = null,
    Color? ExpansionColor1 = null, Color? ExpansionColor2 = null);
public sealed record PangyaUiEditTextColors(Color? Foreground = null, Color? Outline = null);
public sealed record PangyaUiComboItem(string Text, string? Image = null);
public sealed record PangyaUiComboImageSelection(string? Image, byte CurrentIndex = 0);
public sealed record PangyaUiEditCaret(int Line, int Column, TimeSpan Elapsed)
{
    public bool Steady { get; init; }
}
public enum PangyaUiComboDirection
{
    Auto,
    Above,
    Below
}

public sealed record PangyaUiComboPreview(IReadOnlyList<PangyaUiComboItem> Items, bool IsOpen = false,
    int SelectedIndex = -1, int HoveredIndex = -1, int TopRow = 0,
    PangyaUiComboDirection Direction = PangyaUiComboDirection.Auto)
{
    public bool? AutomaticOpensAbove { get; init; }
    public int? IncreasedMaximumRows { get; init; }
    public float? ScrollThumbRow { get; init; }
    public float? ScrollThumbOffset { get; init; }
    public PangyaUiComboImageSelection? ImageSelection { get; init; }
    public int? StoredTextRowCount { get; init; }
    public IReadOnlyList<string>? TextureImages { get; init; }
}
public sealed record PangyaUiGaugeTransition(int From, int To, TimeSpan Elapsed, double Speed = 65,
    bool Achilles = false);

public sealed record PangyaUiRenderOptions(
    PangyaUiButtonState ButtonState = PangyaUiButtonState.Normal,
    IReadOnlySet<string>? EnabledSymbols = null,
    bool ShowDebugBounds = false,
    PangyaUiNode? SelectedNode = null,
    float StrokeScale = 1f)
{
    internal Size RenderCanvasSize { get; init; }
    public float GraphicsAlpha { get; init; } = 1;
    public IReadOnlySet<PangyaUiNode>? DeferredDisplayContainers { get; init; }
    public IReadOnlySet<PangyaUiNode>? ModalForms { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, uint>? FormFlags { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? ViewerTopRows { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? ViewerFollowBottom { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, byte>? ButtonOpacity { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Action<Graphics, Rectangle, PangyaUiButtonState>>? ButtonOwnerDraw { get; init; }
    /// <summary>Owner-drawn artwork bounds relative to the button origin.</summary>
    public IReadOnlyDictionary<PangyaUiNode, Rectangle>? ButtonOwnerDrawBounds { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? SelectedTabIndices { get; init; }
    public IReadOnlyDictionary<(PangyaUiNode Node, Point Origin), int>? SelectedTabInstances { get; init; }
    public IReadOnlyDictionary<PangyaUiTabInstance, int>? SelectedTabsByInstance { get; init; }
    public (PangyaUiTabInstance Instance, int Index)? HoveredTabInstance { get; init; }
    internal PangyaUiResourceInstance? ResourceInstance { get; init; }
    public IReadOnlySet<PangyaUiNode>? MakeRoomMatchForms { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, byte>? TabOpacity { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? TabSeparatorWidths { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? TabMouseEventsEnabled { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int?>? HoveredTabIndices { get; init; }
    public Action<PangyaUiNode, int, Rectangle>? OnTabRendered { get; init; }
    public Action<PangyaUiNode, int, Rectangle, Point>? OnTabInstanceRendered { get; init; }
    public Action<PangyaUiTabInstance, int, Rectangle, Point>? OnTabRenderedWithInstance { get; init; }
    public (PangyaUiNode Node, int Index, Rectangle Bounds)? HoveredTab { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Action<Graphics, int?, Rectangle>>? TabOwnerDraw { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Rectangle>? AreaClipOverrides { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? AreaClippingEnabled { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? AreaMouseEvents { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, byte>? AreaOpacity { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, string?>? AreaImages { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Image>? AreaBitmapImages { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, string>? AreaImageNames { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Action<Graphics, Rectangle>>? AreaOwnerDraw { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Action<Graphics, Rectangle>>? ImageGaugeOwnerDraw { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Action<Graphics, Rectangle>>? ViewerOwnerDraw { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, byte>? ViewerOpacity { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, byte>? EditOpacity { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, string>? ViewerImages { get; init; }
    public IReadOnlySet<PangyaUiNode>? HiddenViewerScrollbars { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? RetainedViewerScrollbarCapacities { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? WheelEvents { get; init; }
    public PangyaUiNode? WheelFocusedViewer { get; init; }
    public PangyaUiNode? WheelFocusedNode { get; init; }
    public (PangyaUiNode Node, float Offset)? DraggedViewerThumb { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? ViewerScrollRows { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? ViewerMouseEvents { get; init; }
    public IReadOnlySet<PangyaUiNode>? DisabledNodes { get; init; }
    public IReadOnlySet<PangyaUiNode>? ViewFocusedNodes { get; init; }
    public IReadOnlySet<PangyaUiNode>? FixedNodes { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? WindowAlpha { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? WindowAlpha2 { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? ScrollbarAlpha { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? ScrollbarAlpha2 { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? FormFrameAlpha { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? FormFrameAlpha2 { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? ComboButtonAlpha { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? ComboButtonAlpha2 { get; init; }
    public IReadOnlyDictionary<(PangyaUiNode Node, bool Right), float>? GaugeButtonAlpha { get; init; }
    public IReadOnlyDictionary<(PangyaUiNode Node, bool Right), float>? GaugeButtonAlpha2 { get; init; }
    public IReadOnlySet<PangyaUiNode>? HiddenNodes { get; init; }
    public IReadOnlySet<PangyaUiNode>? VisibleNodes { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiWindowLifecycle>? WindowLifecycles { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Point>? WindowOffsets { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Size>? WindowSizes { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? ScrollbarGuides { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? GaugeStepCounts { get; init; }
    public IReadOnlySet<PangyaUiNode>? TopmostNodes { get; init; }
    public IReadOnlyList<PangyaUiNode>? TopmostOrder { get; init; }
    public PangyaUiNode? TopFocusedNode { get; init; }
    public IReadOnlySet<PangyaUiNode>? PrivateNodes { get; init; }
    public bool HidePrivacy { get; init; }
    public string? StartupWallpaper { get; init; }
    public Image? StartupWallpaperImage { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, string?>? DesktopWallpapers { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Image>? DesktopWallpaperImages { get; init; }
    public IReadOnlySet<PangyaUiNode>? LockedDesktopWallpapers { get; init; }
    public Color DesktopWallpaperTint { get; init; } = Color.White;
    public IReadOnlyDictionary<PangyaUiNode, Color>? DesktopWallpaperTints { get; init; }
    public bool DesktopWallpaperVisible { get; init; } = true;
    public IReadOnlyDictionary<PangyaUiNode, bool>? DesktopWallpaperVisibility { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PointF>? DesktopAnimationOffsets { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiFrameText>? FrameText { get; init; }
    public bool ShowFrameBlink { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, TimeSpan>? FrameBlinkElapsed { get; init; }
    public bool IsViewFocused { get; init; } = true;
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiImageButtonState>? ImageButtonStates { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, long>? ButtonPressTicks { get; init; }
    public long CurrentTick { get; init; }
    public PangyaUiNode? PressedButton { get; init; }
    public IReadOnlySet<PangyaUiNode>? LatchedButtons { get; init; }
    public PangyaUiNode? HoveredNode { get; init; }
    public Point? Pointer { get; init; }
    public bool ShowTooltip { get; init; }
    public PangyaUiCursorPreview? CursorPreview { get; init; }
    public bool CursorVisible { get; init; } = true;
    public int CursorIndex { get; init; }
    public int TooltipStyle { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiTooltipPreview>? Tooltips { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? TooltipEnabled { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, string>? StaticCaptions { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, string>? TextButtonCaptions { get; init; }
    internal IReadOnlyDictionary<PangyaUiNode, string>? TruncatedStaticCaptions { get; init; }
    public IReadOnlySet<PangyaUiNode>? TextButtonCaptionGeometryNodes { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? TextButtonCaptionWidths { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, (Color Foreground, Color Outline)>? StaticTextColors { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? StaticTextStyles { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Rectangle>? CaptionClips { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? CaptionClippingEnabled { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, byte>? CaptionOpacity { get; init; }
    public IReadOnlySet<PangyaUiNode>? SmallFontCaptions { get; init; }
    public int? CaptionFontHeight { get; init; }
    public Func<string, float>? MeasureCaptionText { get; init; }
    public Action<Graphics, string, Brush, PointF>? DrawCaptionText { get; init; }
    public PangyaUiLanguageTable? LanguageTable { get; init; }
    public int? TooltipFontHeight { get; init; }
    public Func<string, float>? MeasureTooltipText { get; init; }
    public Action<Graphics, string, Brush, PointF>? DrawTooltipText { get; init; }
    public byte TooltipAlpha { get; init; } = 220;
    public PangyaUiNode? TooltipNode { get; init; }
    public Point? TooltipPointer { get; init; }
    public TimeSpan LayoutAnimationTime { get; init; }
    public bool SkipAreaEntranceEffects { get; init; }
    public bool PreserveSelectionDrawOrder { get; init; }
    public bool HideSelectionOutline { get; init; }
    public bool TintSelection { get; init; }
    public bool IncludeCaptionVisualBounds { get; init; }
    public bool AnchorCanvasToSelectedForm { get; init; }
    public bool IncludeStaticCaptionBounds { get; init; }
    public TimeSpan? EmoticonAnimationTime { get; init; }
    public TimeSpan? EditEmoticonAnimationTime { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, IReadOnlyList<long>>? EditLineAnimationStartTicks { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? CaptionScrollOffsets { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, string>? ContextMenuCaptions { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiMenuState>? MenuStates { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, IReadOnlyList<string>>? EditLines { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, IReadOnlyList<int>>? EditLineStyles { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiEditTextColors>? EditTextColors { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, IReadOnlyDictionary<int, string>>? ContextMenuItemTexts { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? EditKeyEvents { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? EditReadOnlyStyles { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? EditEmoticonStyles { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, uint>? SelectedTabIdentifiers { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, uint>? UnselectedTabIdentifiers { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, IReadOnlySet<int>>? JustifiedEditLines { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiEditCaret>? EditCarets { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? EditTopRows { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? EditScrollRows { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, bool>? EditFollowBottom { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? GaugeValues { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? GaugePositions { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, float>? GaugeColorPositions { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, Rectangle>? GaugeTracks { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, RectangleF>? GaugeBars { get; init; }
    public IReadOnlyDictionary<(PangyaUiNode Node, bool Left), Rectangle>? GaugeButtons { get; init; }
    internal IReadOnlyDictionary<PangyaUiNode, (PangyaUiGaugeState State, ulong Version)>? GaugeGeometryVersions { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? ExtendedGaugeSteps { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, int>? GaugeExpansions { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiGaugeRange>? GaugeRanges { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiGaugeAppearance>? GaugeAppearances { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiComboPreview>? Combos { get; init; }
    public int ComboViewportHeight { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiGaugeTransition>? GaugeTransitions { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, IReadOnlyList<PangyaUiTab>>? Tabs { get; init; }
    public IReadOnlyDictionary<PangyaUiNode, PangyaUiListPreview>? Lists { get; init; }
}

public sealed class PangyaUiRenderer
{
    private sealed class GraphicsAlphaState
    {
        public float Alpha = 1;
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Graphics, GraphicsAlphaState>
        GraphicsAlpha = new();

    private static float GetGraphicsAlpha(Graphics graphics) =>
        GraphicsAlpha.TryGetValue(graphics, out GraphicsAlphaState? state) ? state.Alpha : 1;
    private const int ViewerRowHeight = 20;
    private readonly IPangyaImageProvider _images;
    private readonly PangyaUiResourceCatalog _resources;

    public PangyaUiRenderer(IPangyaImageProvider images,
        PangyaUiResourceCatalog? resources = null)
    {
        _images = images ?? throw new ArgumentNullException(nameof(images));
        _resources = resources ?? PangyaUiResourceCatalog.Empty;
    }

    public void Render(Graphics graphics, PangyaUiDocument document,
        PangyaUiNode? selectedForm = null, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(document);
        options ??= new();
        if (!float.IsFinite(options.GraphicsAlpha))
            throw new ArgumentOutOfRangeException(nameof(options), "Graphics alpha must be finite.");
        GraphicsAlphaState state = GraphicsAlpha.GetValue(graphics, _ => new());
        float previous = state.Alpha;
        state.Alpha = Math.Clamp(options.GraphicsAlpha, 0, 1);
        try { RenderCore(graphics, document, selectedForm, options); }
        finally { state.Alpha = previous; }
    }

    private void RenderCore(Graphics graphics, PangyaUiDocument document,
        PangyaUiNode? selectedForm, PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(document);
        options ??= new PangyaUiRenderOptions();
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        using var canvasBrush = new SolidBrush(Color.FromArgb(46, 49, 55));
        Rectangle canvasBounds = GetPreviewCanvasBounds(document, selectedForm, options);
        Size canvasSize = canvasBounds.Size;
        options = options with { RenderCanvasSize = canvasSize };
        if (options.ComboViewportHeight <= 0)
            options = options with { ComboViewportHeight = canvasSize.Height };
        graphics.FillRectangle(canvasBrush, canvasBounds);
        if (options.DesktopWallpaperVisible && selectedForm?.IsLayout != true &&
            GetStartupWallpaper(options, Symbols(options)) is Image wallpaper)
            DrawDesktopWallpaper(graphics, wallpaper, canvasBounds, options.DesktopWallpaperTint);

        if (selectedForm is null && GetStandaloneFrame(options) is { } standaloneFrame)
        {
            DrawFrame(graphics, new Rectangle(Point.Empty, canvasSize), standaloneFrame, Symbols(options),
                showBlink: false, useAuthoredBaseLayer: true);
            return;
        }

        var dropdowns = new List<(PangyaUiNode Node, Rectangle Bounds, PangyaUiComboPreview Combo)>();
        var topmost = new List<(PangyaUiNode Node, Point Offset, HashSet<PangyaUiNode> Chain, PangyaUiResourceInstance? Instance)>();
        var deferred = new List<PangyaUiNode>();
        IReadOnlySet<string> symbols = Symbols(options);
        if (options.Combos is not null)
        {
            // The list is registered during combo initialization, independently of its owner.
            foreach (PangyaUiNode node in GetRenderOrder(document, selectedForm, null, options.EnabledSymbols, null, options))
            {
                if (!IsCombo(node) || !IsRenderVisible(node, options) || IsTransparent(node, options.ButtonState) ||
                    GetDeferredDisplayContainer(node, options) is not null ||
                    options.Combos.GetValueOrDefault(node) is not { IsOpen: true } combo) continue;
                Rectangle edit = GetEditBounds(node, GetAbsoluteBounds(node, options), symbols, options);
                dropdowns.Add((node, edit, combo));
            }
        }
        foreach (PangyaUiNode node in GetRenderOrder(document, selectedForm, options.SelectedNode,
                     options.EnabledSymbols, options.TopmostNodes, options))
        {
            if (IsRuntimeHidden(node, options)) continue;
            PangyaUiNode? deferredContainer = GetDeferredDisplayContainer(node, options);
            if (deferredContainer is not null)
            {
                deferred.Add(node);
                continue;
            }
            Point offset = GetParentOffset(node, options);
            var chain = new HashSet<PangyaUiNode>(ReferenceEqualityComparer.Instance);
            if (options.TopmostNodes is { } registered && IsTopmost(node, registered))
            {
                topmost.Add((node, offset, chain, options.ResourceInstance));
                continue;
            }
            DrawNode(graphics, node, offset, options, chain, dropdowns, topmost, queueDropdown: false);
            if (options.ShowDebugBounds)
                DrawDebugBounds(graphics, node, GetAbsoluteBounds(node, options), options.StrokeScale);
        }
        DrawPendingDropdowns();
        for (int index = 0; index < topmost.Count; index++)
        {
            if (options.TopmostOrder is not null)
            {
                int next = index;
                for (int candidate = index + 1; candidate < topmost.Count; candidate++)
                    if (GetTopmostRank(topmost[candidate].Node, options.TopmostOrder) <
                        GetTopmostRank(topmost[next].Node, options.TopmostOrder)) next = candidate;
                if (next != index)
                {
                    var nextEntry = topmost[next];
                    topmost.RemoveAt(next);
                    topmost.Insert(index, nextEntry);
                }
            }
            var entry = topmost[index];
            bool expanded = entry.Chain.Count > 0;
            DrawNode(graphics, entry.Node, entry.Offset, options with { ResourceInstance = entry.Instance }, entry.Chain, dropdowns, topmost,
                drawTopmost: true, queueDropdown: expanded);
            if (!expanded && options.ShowDebugBounds)
                DrawDebugBounds(graphics, entry.Node, GetAbsoluteBounds(entry.Node, options), options.StrokeScale);
        }
        DrawPendingDropdowns();

        void DrawPendingDropdowns()
        {
            foreach (var dropdown in dropdowns)
                DrawComboList(graphics, dropdown.Node, dropdown.Bounds, dropdown.Combo, Symbols(options), options);
            dropdowns.Clear();
        }

        // Fresh's desktop pass draws the children of direct flagged containers,
        // without drawing those containers themselves.
        var deferredDropdowns = new List<(PangyaUiNode Node, Rectangle Bounds, PangyaUiComboPreview Combo)>();
        foreach (PangyaUiNode node in deferred)
            DrawNode(graphics, node, GetParentOffset(node, options), options,
                new HashSet<PangyaUiNode>(ReferenceEqualityComparer.Instance), deferredDropdowns, topmost);
        foreach (var dropdown in deferredDropdowns)
            DrawComboList(graphics, dropdown.Node, dropdown.Bounds, dropdown.Combo, Symbols(options), options);

        if (!options.HideSelectionOutline && options.SelectedNode is not null &&
            !IsRuntimeHidden(options.SelectedNode, options))
        {
            Rectangle selectedBounds = GetRenderedBounds(options.SelectedNode, options);
            if (selectedBounds.Width <= 0 || selectedBounds.Height <= 0)
                selectedBounds = GetAbsoluteBounds(options.SelectedNode, options);
            if (options.TintSelection && selectedBounds.Width > 0 && selectedBounds.Height > 0)
            {
                using var tint = new SolidBrush(Color.FromArgb(48, Color.Orange));
                graphics.FillRectangle(tint, selectedBounds);
            }
            DrawOutline(graphics, selectedBounds, Color.Orange, 2f * options.StrokeScale);
        }
        if (options.ShowTooltip && options.TooltipAlpha > 0 &&
            (options.TooltipNode ?? options.HoveredNode) is { } tooltipNode &&
            options.TooltipEnabled?.GetValueOrDefault(tooltipNode, true) != false &&
            !IsRuntimeHidden(tooltipNode, options) && IsRenderVisible(tooltipNode, options) &&
            IsWindowViewFocused(tooltipNode, options))
        {
            PangyaUiTooltipPreview? tooltipState = options.Tooltips?.GetValueOrDefault(tooltipNode);
            string? tooltip = tooltipState?.Text ?? tooltipNode.Parameters.GetValueOrDefault("tooltip");
            Point? pointer = options.TooltipPointer ?? options.Pointer;
            if (tooltip is { Length: > 0 } && (pointer.HasValue || tooltipState?.Position is not null))
                DrawTooltip(graphics, tooltip, pointer ?? Point.Empty, canvasBounds, options, tooltipState);
        }
        if (options.CursorVisible && options.CursorPreview is { } cursor) DrawCursor(graphics, cursor, Symbols(options));
    }

    public PangyaUiRenderOptions SetFormMessage(Graphics metrics, PangyaUiNode form, string message,
        bool spaceAlign = false, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(metrics);
        ArgumentNullException.ThrowIfNull(form);
        ArgumentNullException.ThrowIfNull(message);
        options ??= new();
        PangyaUiNode? label = form.Children.LastOrDefault(child => child.Name == "message" &&
            child.Type.Equals("STATIC", StringComparison.OrdinalIgnoreCase));
        if (label is not null)
            options = SetStaticCaption(label, message, options: options);
        PangyaUiNode? edit = form.Children.LastOrDefault(child => child.Name == "message" &&
            child.Type.Equals("EDIT", StringComparison.OrdinalIgnoreCase));
        if (edit is null) return options;
        int Read(string name, int fallback) => int.TryParse(edit.Parameters.GetValueOrDefault(name),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;
        var model = new PangyaUiEditText(IsMultilineEdit(edit), Math.Max(0, Read("charlimit", 255)));
        var colors = GetEditTextColors(edit, options);
        model.FontColor = unchecked((uint)colors.Foreground.ToArgb());
        model.OutlineColor = unchecked((uint)colors.Outline.ToArgb());
        int margin = Read("leftmargin", 0);
        bool symmetry = string.Equals(edit.Parameters.GetValueOrDefault("symmetry", "true"),
            "true", StringComparison.OrdinalIgnoreCase);
        float width = GetWindowBounds(edit, options).Width - (long)margin * (symmetry ? 2 : 1) -
            (GetBitmap("scroll_n_1", Symbols(options))?.Width ?? 0);
        bool readOnly = options.EditReadOnlyStyles?.GetValueOrDefault(edit,
            edit.Parameters.GetValueOrDefault("readonly") == "true") ??
            edit.Parameters.GetValueOrDefault("readonly") == "true";
        var symbols = readOnly && (options.EditEmoticonStyles?.GetValueOrDefault(edit, true) ?? true)
            ? Symbols(options) : null;
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        model.AddText(message, width, text => MeasureCaptionLine(metrics, text,
            SystemFonts.DefaultFont, format, symbols, measureText: options.MeasureCaptionText), spaceAlign);
        options = model.ApplyTo(edit, options);
        var topRows = options.EditTopRows?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        topRows[edit] = (options.EditFollowBottom?.GetValueOrDefault(edit, true) ?? true)
            ? GetEditMaximumTopRow(edit, model.Lines.Count, options) : 0;
        var scrollRows = options.EditScrollRows?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        scrollRows.Remove(edit);
        return options with { EditTopRows = topRows, EditScrollRows = scrollRows };
    }

    public bool IsDesktopWallpaperVisible(PangyaUiNode node, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        Color tint = options.DesktopWallpaperTints?.GetValueOrDefault(node, options.DesktopWallpaperTint)
            ?? options.DesktopWallpaperTint;
        return GetDesktopWallpaper(node, options, out _) is not null && tint.ToArgb() != 0;
    }

    private Image? GetDesktopWallpaper(PangyaUiNode node, PangyaUiRenderOptions options, out bool show)
    {
        Image? bitmap = options.DesktopWallpaperImages?.GetValueOrDefault(node);
        bool hasFilename = options.DesktopWallpapers?.TryGetValue(node, out _) == true;
        string? filename = options.DesktopWallpapers?.GetValueOrDefault(node);
        show = (options.DesktopWallpaperVisibility?.GetValueOrDefault(node,
            options.DesktopWallpaperVisible) ?? options.DesktopWallpaperVisible) &&
            (bitmap is not null || !hasFilename || !string.IsNullOrEmpty(filename));
        if (!show) return null;
        if (bitmap is not null) return bitmap;
        if (hasFilename) return GetBitmap(filename ?? string.Empty, Symbols(options));
        if (node.Name.Equals("LOADING", StringComparison.OrdinalIgnoreCase) &&
            (options.StartupWallpaperImage is not null || !string.IsNullOrEmpty(options.StartupWallpaper)))
            return GetStartupWallpaper(options, Symbols(options));
        return GetBitmap(node.LayoutBase?.GetAttribute("background") ?? string.Empty, Symbols(options));
    }

    public static PangyaUiRenderOptions SetDesktopWallpaper(PangyaUiNode node, string? filename,
        bool show, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        if (options.LockedDesktopWallpapers?.Contains(node) == true) return options;
        var filenames = options.DesktopWallpapers?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var images = options.DesktopWallpaperImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var visibility = options.DesktopWallpaperVisibility?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        string current = filenames.GetValueOrDefault(node) ?? node.LayoutBase?.GetAttribute("background") ?? string.Empty;
        if (string.IsNullOrEmpty(filename) || !string.Equals(current, filename, StringComparison.OrdinalIgnoreCase))
        {
            filenames[node] = filename ?? string.Empty;
            images.Remove(node);
        }
        visibility[node] = !string.IsNullOrEmpty(filename) && show;
        return options with { DesktopWallpapers = filenames, DesktopWallpaperImages = images,
            DesktopWallpaperVisibility = visibility };
    }

    public static PangyaUiRenderOptions SetDesktopWallpaperFromBitmap(PangyaUiNode node, Image? image,
        bool show, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        var visibility = options.DesktopWallpaperVisibility?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        visibility[node] = show;
        if (image is null) return options with { DesktopWallpaperVisibility = visibility };
        var images = options.DesktopWallpaperImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var filenames = options.DesktopWallpapers?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        images[node] = image;
        filenames[node] = string.Empty;
        return options with { DesktopWallpapers = filenames, DesktopWallpaperImages = images,
            DesktopWallpaperVisibility = visibility };
    }

    private static byte GetWindowOpacity(PangyaUiNode node, PangyaUiRenderOptions options,
        byte opacity = byte.MaxValue, bool secondary = true) =>
        (byte)(opacity * GetWindowAlphaScale(node, options, secondary));

    private static double GetWindowAlphaScale(PangyaUiNode node, PangyaUiRenderOptions options,
        bool secondary = true)
    {
        float primary = options.WindowAlpha?.GetValueOrDefault(node, 1) ?? 1;
        float second = secondary ? options.WindowAlpha2?.GetValueOrDefault(node, 1) ?? 1 : 1;
        if (!float.IsFinite(primary)) primary = 1;
        if (!float.IsFinite(second)) second = 1;
        return (double)Math.Clamp(primary, 0, 1) * Math.Clamp(second, 0, 1);
    }

    private static byte GetScrollbarOpacity(PangyaUiNode node, PangyaUiRenderOptions options,
        byte opacity = byte.MaxValue)
    {
        float primary = options.ScrollbarAlpha?.GetValueOrDefault(node, 1) ?? 1;
        float secondary = options.ScrollbarAlpha2?.GetValueOrDefault(node, 1) ?? 1;
        if (!float.IsFinite(primary)) primary = 1;
        if (!float.IsFinite(secondary)) secondary = 1;
        return (byte)(opacity * (double)Math.Clamp(primary, 0, 1) * Math.Clamp(secondary, 0, 1));
    }

    public static PangyaUiRenderOptions SetWindowSecondaryAlphaToChildren(PangyaUiNode node, float alpha,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!float.IsFinite(alpha)) throw new ArgumentOutOfRangeException(nameof(alpha));
        options ??= new();
        var values = options.WindowAlpha2?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var scrollbars = options.ScrollbarAlpha2?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var frames = options.FormFrameAlpha2?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var comboButtons = options.ComboButtonAlpha2?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var gaugeButtons = options.GaugeButtonAlpha2?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var pending = new Stack<PangyaUiNode>();
        pending.Push(node);
        while (pending.TryPop(out PangyaUiNode? current))
        {
            values[current] = alpha;
            scrollbars[current] = alpha;
            if (current.IsForm && !current.IsLayout) frames[current] = alpha;
            if (IsCombo(current)) comboButtons[current] = alpha;
            if (IsExtendedGauge(current))
            {
                gaugeButtons[(current, false)] = alpha;
                gaugeButtons[(current, true)] = alpha;
            }
            foreach (PangyaUiNode child in current.Children) pending.Push(child);
        }
        return options with { WindowAlpha2 = values, ScrollbarAlpha2 = scrollbars,
            FormFrameAlpha2 = frames, ComboButtonAlpha2 = comboButtons, GaugeButtonAlpha2 = gaugeButtons };
    }

    public static bool IsWindowViewFocused(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (options.ViewFocusedNodes is null && options.FixedNodes is null) return options.IsViewFocused;
        for (PangyaUiNode? current = node; current is not null; current = current.Parent)
        {
            if (options.ViewFocusedNodes?.Contains(current) == true || options.FixedNodes?.Contains(current) == true)
                return true;
            if (current.IsForm && !current.IsLayout) return false;
        }
        return false;
    }

    public static PangyaUiRenderOptions SetWindowViewFocus(PangyaUiNode node, bool takeFromOthers = true,
        PangyaUiRenderOptions? options = null, PangyaUiNode? modalForm = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        if (modalForm is not null && !ReferenceEquals(modalForm, node)) return options;
        var focused = takeFromOthers ? new HashSet<PangyaUiNode>() : options.ViewFocusedNodes?.ToHashSet() ?? [];
        PangyaUiNode target = node;
        while (target.Parent is not null && (!target.IsForm || target.IsLayout)) target = target.Parent;
        focused.Add(target);
        return options with { ViewFocusedNodes = focused };
    }

    public static PangyaUiNode? FindViewFocusedChild(PangyaUiNode parent, PangyaUiRenderOptions options,
        bool requireEnabledAndVisible = true)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(options);
        IReadOnlySet<string> symbols = Symbols(options);
        for (int index = parent.Children.Count - 1; index >= 0; index--)
        {
            PangyaUiNode child = parent.Children[index];
            if (!child.IsVisible(symbols)) continue;
            if (options.TopmostNodes?.Contains(child) == true)
            {
                if (IsWindowTopFocused(child, options)) return child;
                continue;
            }
            if (options.ViewFocusedNodes?.Contains(child) == true &&
                (!requireEnabledAndVisible || (!IsRuntimeHidden(child, options) && IsRenderVisible(child, options) &&
                    options.DisabledNodes?.Contains(child) != true))) return child;
        }
        return null;
    }

    public static PangyaUiRenderOptions HandOverViewFocus(PangyaUiNode desktop,
        IReadOnlySet<PangyaUiNode> keyEventForms, PangyaUiRenderOptions options, out PangyaUiNode? recipient,
        PangyaUiNode? modalForm = null)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(keyEventForms);
        ArgumentNullException.ThrowIfNull(options);
        if (modalForm is not null)
        {
            recipient = null;
            return SetWindowViewFocus(modalForm, takeFromOthers: false, options);
        }
        IReadOnlySet<string> symbols = Symbols(options);
        for (int index = desktop.Children.Count - 1; index >= 0; index--)
        {
            PangyaUiNode form = desktop.Children[index];
            if (!form.IsForm || form.IsLayout || !keyEventForms.Contains(form) || !form.IsVisible(symbols) ||
                !IsRenderVisible(form, options) || IsRuntimeHidden(form, options)) continue;
            options = SetWindowViewFocus(form, takeFromOthers: false, options);
            if (options.FixedNodes?.Contains(form) != true)
            {
                recipient = form;
                return options;
            }
            break;
        }
        recipient = desktop;
        var focused = options.ViewFocusedNodes?.ToHashSet() ?? [];
        focused.Add(desktop);
        return options with { ViewFocusedNodes = focused };
    }

    public static PangyaUiRenderOptions ResetWindowViewFocus(PangyaUiNode node,
        PangyaUiRenderOptions? options = null, PangyaUiNode? stop = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        if (ReferenceEquals(node, stop)) return options;
        var focused = options.ViewFocusedNodes?.ToHashSet() ?? [];
        var pending = new Stack<PangyaUiNode>();
        pending.Push(node);
        while (pending.TryPop(out PangyaUiNode? current))
        {
            if (ReferenceEquals(current, stop)) continue;
            focused.Remove(current);
            foreach (PangyaUiNode child in current.Children) pending.Push(child);
        }
        return options with { ViewFocusedNodes = focused };
    }

    public static bool TryMoveCursorToChild(PangyaUiNode parent, string name, PangyaUiRenderOptions options,
        out PangyaUiRenderOptions updatedOptions, PangyaUiNode? modalForm = null)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(options);
        updatedOptions = options;
        if (modalForm is not null && !ReferenceEquals(parent, modalForm)) return false;
        PangyaUiNode? child = parent.Children.FirstOrDefault(candidate => candidate.Name == name);
        if (child is null) return false;
        Rectangle bounds = GetAbsoluteBounds(child, options);
        Point position = new(ClampCoordinate(((long)bounds.X * 2 + bounds.Width) / 2),
            ClampCoordinate(((long)bounds.Y * 2 + bounds.Height) / 2));
        PangyaUiCursorPreview cursor = options.CursorPreview is { } current
            ? current with { Position = position } : new(position, options.CursorIndex);
        updatedOptions = options with { CursorPreview = cursor, Pointer = position };
        return true;
    }

    public bool CanDrawCursor(int index, IReadOnlySet<string> symbols) => GetCursorFrameCount(index, symbols) > 0;

    public int GetCursorFrameCount(int index, IReadOnlySet<string> symbols) =>
        GetCursorSprite(index, symbols)?.Width / 32 ?? 0;

    private Image? GetCursorSprite(int index, IReadOnlySet<string> symbols)
    {
        ReadOnlySpan<string> suffixes = ["1", "2", "3", "4", "_up", "_down", "_zoom", "_both",
            "_move", "_move_no", "_rotate", "_rotate_no", "_caret", "_context", "_rotate2"];
        if ((uint)index >= (uint)suffixes.Length) return null;
        Image? sprite = GetBitmap("[s_pointer" + suffixes[index] + ".png", symbols);
        return sprite is { Width: >= 32, Height: >= 32 } ? sprite : null;
    }

    private void DrawCursor(Graphics graphics, PangyaUiCursorPreview cursor, IReadOnlySet<string> symbols)
    {
        if (GetCursorSprite(cursor.Index, symbols) is not Image sprite) return;
        const int size = 32;
        int frame = Math.Max(0, cursor.Frame) % (sprite.Width / size);
        int offset = cursor.Index switch { 4 => -4, 5 => -28, _ => 0 };
        var destination = new Rectangle(cursor.Position.X, ClampCoordinate((long)cursor.Position.Y + offset), size, size);
        graphics.DrawImage(sprite, destination, new Rectangle(frame * size, 0, size, size), GraphicsUnit.Pixel);
    }

    private void DrawTooltip(Graphics graphics, string text, Point pointer, Rectangle canvas,
        PangyaUiRenderOptions options, PangyaUiTooltipPreview? tooltipState)
    {
        const int cap = 8;
        IReadOnlySet<string> symbols = Symbols(options);
        int style = Math.Clamp(tooltipState?.Style ?? options.TooltipStyle, 0, 3);
        byte alpha = options.TooltipAlpha;
        const string tooltipFontFamily = "MS UI Gothic";
        using var gothicFont = new Font(tooltipFontFamily, 11, GraphicsUnit.Pixel);
        using Font? fallbackFont = gothicFont.Name.Equals(tooltipFontFamily, StringComparison.OrdinalIgnoreCase)
            ? null : new Font(SystemFonts.DefaultFont.FontFamily, 11, GraphicsUnit.Pixel);
        Font font = fallbackFont ?? gothicFont;
        const float lineSpacing = 2;
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        string[] lines = PangyaUiText.ComposeLines(text);
        if (lines.Length == 0) return;
        text = string.Join("\n", lines);
        float textWidth = PangyaUiText.GetLongestLineWidth(text, MeasureRun);
        float lineHeight = options.TooltipFontHeight is > 0 ? options.TooltipFontHeight.Value : font.GetHeight(graphics);
        float textHeight = lineHeight * lines.Length + lineSpacing * (lines.Length - 1);
        int width = (int)Math.Ceiling(textWidth) + cap * 2;
        int height = Math.Max(cap * 2, (int)Math.Ceiling(textHeight) + cap);
        int x = ClampCoordinate(Math.Min(Math.Max((long)pointer.X + 20, canvas.Left), (long)canvas.X + canvas.Width - width));
        int cursorOffsetY = (options.CursorPreview?.Index ?? options.CursorIndex) != 0 ? 30 : 20;
        int y = ClampCoordinate(Math.Min(Math.Max((long)pointer.Y + cursorOffsetY, canvas.Top), (long)canvas.Y + canvas.Height - height));
        if (tooltipState?.Position is Point fixedPosition)
        {
            x = fixedPosition.X;
            y = fixedPosition.Y;
        }
        var bounds = new Rectangle(x, y, width, height);
        GraphicsState state = graphics.Save();
        GraphicsAlphaState alphaState = GraphicsAlpha.GetValue(graphics, _ => new());
        float previousAlpha = alphaState.Alpha;
        // Native tooltip skins bypass the shared GDI alpha, and tooltip text
        // temporarily replaces it with the tooltip's own alpha. Brushes/tints
        // below already carry that tooltip alpha, so do not multiply it again.
        alphaState.Alpha = 1;
        try
        {
            graphics.SetClip(canvas, CombineMode.Intersect);
            Image?[] skin = Enumerable.Range(0, 9)
                .Select(index => GetBitmap(FormattableString.Invariant($"tooltip_frm{style:00}_{index:00}"), symbols))
                .ToArray();
            if (skin.All(image => image is null))
            {
                using var background = new SolidBrush(Color.FromArgb(alpha, 255, 255, 225));
                graphics.FillRectangle(background, bounds);
            }
            int[] widths = [Math.Min(cap, width), Math.Max(0, width - cap * 2), Math.Min(cap, Math.Max(0, width - cap))];
            int[] heights = [Math.Min(cap, height), Math.Max(0, height - cap * 2), Math.Min(cap, Math.Max(0, height - cap))];
            PixelOffsetMode previousPixelOffset = graphics.PixelOffsetMode;
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            int left = x;
            for (int column = 0; column < 3; column++)
            {
                int top = y;
                for (int row = 0; row < 3; row++)
                {
                    if (skin[row * 3 + column] is Image slice && widths[column] > 0 && heights[row] > 0)
                        DrawTintedImage(graphics, slice, new Rectangle(left, top, widths[column], heights[row]),
                            0, slice.Width, slice.Height, Color.FromArgb(alpha, Color.White));
                    top += heights[row];
                }
                left += widths[column];
            }
            graphics.PixelOffsetMode = previousPixelOffset;
            using var foreground = new SolidBrush(Color.FromArgb(alpha, Color.Black));
            using var outline = new SolidBrush(Color.FromArgb(alpha, Color.White));
            DrawTaggedCaption(graphics, text, foreground, outline, new PointF(x + cap, y + 5),
                format, 0, font, alpha, lineSpacing: lineSpacing, lineHeight: lineHeight,
                measureText: options.MeasureTooltipText, drawText: options.DrawTooltipText, preserveLiteralTokenBoundaries: true);
        }
        finally
        {
            alphaState.Alpha = previousAlpha;
            graphics.Restore(state);
        }

        float MeasureRun(string run) => options.MeasureTooltipText?.Invoke(run) ??
            graphics.MeasureString(run, font, PointF.Empty, format).Width;

    }

    public IReadOnlyList<PangyaUiNode> GetRenderOrder(PangyaUiDocument document,
        PangyaUiNode? selectedForm = null, PangyaUiNode? selectedNode = null,
        IReadOnlySet<string>? enabledSymbols = null, IReadOnlySet<PangyaUiNode>? topmostNodes = null) =>
        GetRenderOrderCore(document, selectedForm, selectedNode, enabledSymbols, topmostNodes, null);

    public IReadOnlyList<PangyaUiNode> GetRenderOrder(PangyaUiDocument document,
        PangyaUiNode? selectedForm, PangyaUiNode? selectedNode, IReadOnlySet<string>? enabledSymbols,
        IReadOnlySet<PangyaUiNode>? topmostNodes, PangyaUiRenderOptions options) =>
        GetRenderOrderCore(document, selectedForm, selectedNode, enabledSymbols, topmostNodes, options);

    private IReadOnlyList<PangyaUiNode> GetRenderOrderCore(PangyaUiDocument document,
        PangyaUiNode? selectedForm, PangyaUiNode? selectedNode, IReadOnlySet<string>? enabledSymbols,
        IReadOnlySet<PangyaUiNode>? topmostNodes, PangyaUiRenderOptions? options)
    {
        ArgumentNullException.ThrowIfNull(document);
        IReadOnlyList<PangyaUiNode> visible = selectedForm is null
            ? []
            : document.GetNodesForForm(selectedForm, enabledSymbols)
                .Where(node => HasVisibleParents(node, selectedForm, options) &&
                    !IsRuntimeHidden(node, options)).ToArray();
        if (options?.PreserveSelectionDrawOrder != true &&
            selectedNode is not null && !selectedNode.IsForm && visible.Contains(selectedNode))
            visible = [.. visible.Where(node => !ReferenceEquals(node, selectedNode)), selectedNode];
        if (topmostNodes is { Count: > 0 })
            visible = visible.OrderBy(node => IsTopmost(node, topmostNodes))
                .ThenBy(node => IsTopmost(node, topmostNodes)
                    ? GetTopmostRank(node, options?.TopmostOrder) : -1).ToArray();
        if (options?.DeferredDisplayContainers is not { Count: > 0 }) return visible;

        // Share the late display pass with hit testing and dialog-fit measurement.
        var normal = new List<PangyaUiNode>();
        var deferred = new List<PangyaUiNode>();
        foreach (PangyaUiNode node in visible)
        {
            PangyaUiNode? container = GetDeferredDisplayContainer(node, options);
            if (container is null) normal.Add(node);
            else if (!ReferenceEquals(node, container) && (selectedForm?.IsLayout == true
                         ? ReferenceEquals(container.Parent, selectedForm)
                         : ReferenceEquals(container, selectedForm)))
                deferred.Add(node);
        }
        normal.AddRange(deferred);
        return normal;
    }

    public static PangyaUiRenderOptions AdjustFormToViewport(PangyaUiNode form, Size viewport,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(form);
        if (!form.IsForm) throw new ArgumentException("The node must be a form.", nameof(form));
        if (viewport.Width < 0 || viewport.Height < 0) throw new ArgumentOutOfRangeException(nameof(viewport));
        options ??= new();
        Rectangle bounds = GetAbsoluteBounds(form, options);
        long x = (long)bounds.X + bounds.Width > viewport.Width
            ? (long)viewport.Width - bounds.Width : bounds.X;
        long y = (long)bounds.Y + bounds.Height > viewport.Height
            ? (long)viewport.Height - bounds.Height : bounds.Y;
        if (x == bounds.X && y == bounds.Y) return options;
        Point previous = GetWindowOffset(form, options);
        var offsets = options.WindowOffsets?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        offsets[form] = new Point(ClampCoordinate(previous.X + x - bounds.X),
            ClampCoordinate(previous.Y + y - bounds.Y));
        return options with { WindowOffsets = offsets };
    }

    public static PangyaUiRenderOptions SetWindowEnabled(PangyaUiNode node, bool enabled,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        var disabled = options.DisabledNodes?.ToHashSet() ?? [];
        if (enabled) disabled.Remove(node); else disabled.Add(node);
        options = options with { DisabledNodes = disabled };
        if (node.Type.Equals("BUTTON", StringComparison.OrdinalIgnoreCase) || IsCombo(node) || IsExtendedGauge(node))
        {
            var buttons = options.ImageButtonStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
            PangyaUiImageButtonState state = buttons.GetValueOrDefault(node) ?? new();
            PangyaUiButtonState status = state.Status ?? options.ButtonState;
            buttons[node] = state with { Enabled = enabled,
                Status = status == PangyaUiButtonState.Hover ? PangyaUiButtonState.Normal : status };
            options = options with { ImageButtonStates = buttons };
        }
        return options;
    }

    public static PangyaUiRenderOptions MoveWindow(PangyaUiNode node, Point position,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        if (IsTextButton(node) && options.TextButtonCaptionGeometryNodes?.Contains(node) != true)
        {
            using var bitmap = new Bitmap(1, 1);
            using Graphics metrics = Graphics.FromImage(bitmap);
            using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
            float Measure(string text) => options.MeasureCaptionText?.Invoke(text) ??
                metrics.MeasureString(text, SystemFonts.DefaultFont, PointF.Empty, format).Width;
            Rectangle bounds = GetWindowBounds(node, options);
            float width = MeasureTextButtonCaptionWidth(GetTextButtonCaption(node, options),
                GetTextButtonSeparator(options), bounds.Width, IsTextButtonCentered(node), Measure);
            var geometry = options.TextButtonCaptionGeometryNodes?.ToHashSet() ?? [];
            geometry.Add(node);
            var widths = options.TextButtonCaptionWidths?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
            widths[node] = width;
            var sizes = options.WindowSizes?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
            sizes[node] = new Size(Math.Max(0, (int)Math.Ceiling(width)), bounds.Height);
            options = options with { TextButtonCaptionGeometryNodes = geometry, TextButtonCaptionWidths = widths, WindowSizes = sizes };
        }
        Rectangle current = GetAbsoluteBounds(node, options);
        long dx = (long)position.X - current.X;
        long dy = (long)position.Y - current.Y;
        var offsets = options.WindowOffsets?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        Point oldOffset = GetWindowOffset(node, options);
        offsets[node] = new Point(ClampCoordinate(oldOffset.X + dx), ClampCoordinate(oldOffset.Y + dy));
        var moved = new List<PangyaUiNode> { node };
        var pending = new Stack<PangyaUiNode>(node.Children);
        while (pending.TryPop(out PangyaUiNode? child))
        {
            if (child.IsForm && !child.IsLayout)
            {
                Point previous = GetWindowOffset(child, options);
                offsets[child] = new Point(ClampCoordinate(previous.X - dx), ClampCoordinate(previous.Y - dy));
                continue;
            }
            moved.Add(child);
            foreach (PangyaUiNode descendant in child.Children) pending.Push(descendant);
        }
        var areaClips = options.AreaClipOverrides?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        foreach (PangyaUiNode child in moved)
            if (IsArea(child) && !areaClips.ContainsKey(child) && !child.Parameters.ContainsKey("clip_area"))
                areaClips[child] = GetAbsoluteBounds(child, options);
        var bars = options.GaugeBars?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        foreach (PangyaUiNode child in moved)
        {
            if (!IsGauge(child) || !bars.TryGetValue(child, out RectangleF bar)) continue;
            Rectangle track = options.GaugeTracks?.GetValueOrDefault(child) ??
                new Rectangle(Point.Empty, GetWindowBounds(child, options).Size);
            int border = GetGaugeBorder(child, track, options);
            bar.Location = new PointF(track.X + border, track.Y + border);
            bars[child] = bar;
        }
        options = options with { WindowOffsets = offsets, AreaClipOverrides = areaClips, GaugeBars = bars };
        var clips = options.CaptionClips?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var enabled = options.CaptionClippingEnabled?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        foreach (PangyaUiNode child in moved)
        {
            if (!child.Type.Equals("STATIC", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(child.Parameters.GetValueOrDefault("scrolling"), "true", StringComparison.OrdinalIgnoreCase))
                continue;
            clips[child] = GetAbsoluteBounds(child, options);
            enabled[child] = true;
        }
        return options with { CaptionClips = clips, CaptionClippingEnabled = enabled };
    }

    public static Rectangle GetWindowClientRectangle(PangyaUiNode node, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        Rectangle screen = GetAbsoluteBounds(node, options);
        return new Rectangle(ScreenToClient(node, screen.Location, options), screen.Size);
    }

    public PangyaUiRenderOptions SetWindowClientRectangle(PangyaUiNode node, Rectangle rectangle,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        return SetWindowRectangle(node, new Rectangle(ClientToScreen(node, rectangle.Location, options), rectangle.Size), options);
    }

    public static Point ClientToScreen(PangyaUiNode node, Point point, PangyaUiRenderOptions? options = null) =>
        TranslateWindowClientPoint(node, point, options, toScreen: true);

    public static Point ScreenToClient(PangyaUiNode node, Point point, PangyaUiRenderOptions? options = null) =>
        TranslateWindowClientPoint(node, point, options, toScreen: false);

    private static Point TranslateWindowClientPoint(PangyaUiNode node, Point point,
        PangyaUiRenderOptions? options, bool toScreen)
    {
        ArgumentNullException.ThrowIfNull(node);
        long x = point.X;
        long y = point.Y;
        int direction = toScreen ? 1 : -1;
        // FrWnd's client conversions walk the parent chain and use each
        // ancestor's stored screen origin. Create only adds the immediate parent.
        for (PangyaUiNode? parent = node.Parent; parent is not null; parent = parent.Parent)
        {
            if (IsMacroItem(parent)) continue;
            Point origin = GetAbsoluteBounds(parent, options).Location;
            x += (long)origin.X * direction;
            y += (long)origin.Y * direction;
        }
        return new Point(ClampCoordinate(x), ClampCoordinate(y));
    }

    public PangyaUiRenderOptions SetWindowRectangle(PangyaUiNode node, Rectangle rectangle,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (rectangle.Width < 0 || rectangle.Height < 0)
            throw new ArgumentOutOfRangeException(nameof(rectangle));
        options ??= new();
        if (IsGauge(node)) options = CacheGaugeGeometry(node, options);
        PangyaUiRenderOptions beforeResize = options;
        options = MoveWindow(node, rectangle.Location, options);
        var sizes = options.WindowSizes?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        sizes[node] = rectangle.Size;
        options = options with { WindowSizes = sizes };
        if (IsTextButton(node))
        {
            // SetRect replaces the window rectangle without running SetCaption.
            var geometry = options.TextButtonCaptionGeometryNodes?.ToHashSet() ?? [];
            geometry.Add(node);
            var captionWidths = options.TextButtonCaptionWidths?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
            captionWidths[node] = rectangle.Width;
            options = options with { TextButtonCaptionGeometryNodes = geometry, TextButtonCaptionWidths = captionWidths };
        }
        var rows = options.ViewerTopRows?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var fractionalRows = options.ViewerScrollRows?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var retainedCapacities = options.RetainedViewerScrollbarCapacities?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        // FrWnd::SetRect calls OnResize on itself and each immediate child.
        foreach (PangyaUiNode resized in node.Children.Prepend(node))
        {
            if (GetList(resized, options) is { } list && GetListItemSize(resized, list) is Size itemSize)
                options = WithListPreview(resized,
                    ResizeListScroll(resized, list, list, itemSize, beforeResize, options), options);
            if (IsMultilineEdit(resized) && beforeResize.EditLines?.GetValueOrDefault(resized) is { Count: > 0 } lines)
            {
                int previousCapacity = GetEditRowCapacity(resized, beforeResize);
                int previousMaximum = Math.Max(0, lines.Count - Math.Max(1, previousCapacity));
                float row = GetEditScrollRow(resized, beforeResize, beforeResize.EditCarets?.GetValueOrDefault(resized),
                    previousCapacity, previousMaximum);
                var scroll = PangyaUiScrollState.RestoreRows(previousCapacity, lines.Count, row,
                    beforeResize.EditFollowBottom?.GetValueOrDefault(resized, true) ?? true);
                scroll.ShowGuide = beforeResize.ScrollbarGuides?.GetValueOrDefault(resized) == true;
                scroll.Resize(GetEditRowCapacity(resized, options));
                options = scroll.ApplyToEdit(resized, options);
            }
            if (!IsViewer(resized) || resized.Parameters.GetValueOrDefault("use_wheel") != "true") continue;
            if (GetViewerImage(resized, Symbols(options), options) is not Image image)
            {
                // FrScrollBar::OnResize follows the new parent rectangle but
                // keeps its row capacity until the viewer resizes with an image.
                retainedCapacities.TryAdd(resized, GetViewerScrollbarCapacity(resized, beforeResize));
                continue;
            }
            retainedCapacities.Remove(resized);
            options = options with { RetainedViewerScrollbarCapacities = retainedCapacities };
            int top = options.ViewerFollowBottom?.GetValueOrDefault(resized) == true
                ? GetViewerScrollRange(resized, image, options) : 0;
            rows[resized] = top;
            fractionalRows[resized] = top;
            if (options.DraggedViewerThumb is { } drag && ReferenceEquals(drag.Node, resized))
                options = options with { DraggedViewerThumb = null };
        }
        return options with { ViewerTopRows = rows, ViewerScrollRows = fractionalRows, RetainedViewerScrollbarCapacities = retainedCapacities };
    }

    public static PangyaUiRenderOptions SetWindowWheelEvent(PangyaUiNode node, bool enabled,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        var events = options.WheelEvents?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        events[node] = enabled;
        return options with { WheelEvents = events };
    }

    public static bool IsWindowWheelEventEnabled(PangyaUiNode node, PangyaUiRenderOptions? options = null) =>
        options?.WheelEvents?.GetValueOrDefault(node, true) ?? true;

    public static PangyaUiRenderOptions SetWindowVisible(PangyaUiNode node, bool visible,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        var hidden = options.HiddenNodes?.ToHashSet() ?? [];
        var shown = options.VisibleNodes?.ToHashSet() ?? [];
        if (visible) { hidden.Remove(node); shown.Add(node); }
        else { shown.Remove(node); hidden.Add(node); }
        options = options with { HiddenNodes = hidden, VisibleNodes = shown };
        if (visible && node.Type.Equals("CONTEXTMENU", StringComparison.OrdinalIgnoreCase) &&
            options.MenuStates?.TryGetValue(node, out PangyaUiMenuState? state) == true)
        {
            var menus = options.MenuStates.ToDictionary(pair => pair.Key, pair => pair.Value);
            menus[node] = state with { DisabledRows = null, DisabledTexts = null };
            options = options with { MenuStates = menus };
        }
        return options;
    }

    public static PangyaUiRenderOptions SetWindowTopmost(PangyaUiNode node, bool topmost,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        var nodes = options.TopmostNodes?.ToHashSet() ?? [];
        var order = options.TopmostOrder?.ToList() ?? nodes.ToList();
        if (topmost)
        {
            if (nodes.Add(node)) order.Add(node);
        }
        else
        {
            nodes.RemoveWhere(candidate =>
            {
                for (PangyaUiNode? current = candidate; current is not null; current = current.Parent)
                    if (ReferenceEquals(current, node)) return true;
                return false;
            });
        }
        order.RemoveAll(candidate => !nodes.Contains(candidate));
        return options with { TopmostNodes = nodes, TopmostOrder = order,
            TopFocusedNode = options.TopFocusedNode is { } focused && nodes.Contains(focused) ? focused : null };
    }

    public static bool IsWindowTopFocused(PangyaUiNode node, PangyaUiRenderOptions options) =>
        ReferenceEquals(options.TopFocusedNode, node) && options.TopmostNodes?.Contains(node) == true;

    public static PangyaUiRenderOptions ResetWindowTopFocus(PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.TopmostNodes is not null)
            foreach (PangyaUiNode node in options.TopmostNodes)
            {
                if (!string.Equals(node.Name, "messenger_chat", StringComparison.Ordinal)) continue;
                options = SetTopFocusCaption(node, false, options);
                options = SetWindowSecondaryAlphaToChildren(node, 0.7f, options);
            }
        return options with { TopFocusedNode = null };
    }

    public static PangyaUiRenderOptions SetWindowTopFocus(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (options.TopmostNodes is not { } registeredNodes || !registeredNodes.Contains(node)) return options;
        options = ResetWindowTopFocus(options);
        options = SetTopFocusCaption(node, true, options);
        options = SetWindowSecondaryAlphaToChildren(node, 1f, options);
        var order = options.TopmostOrder?.ToList() ?? registeredNodes.ToList();
        foreach (PangyaUiNode registered in registeredNodes)
            if (!order.Contains(registered)) order.Add(registered);
        // Keep equivalent entries in registration order. The direct parent rule
        // takes precedence over the focus flag in Fresh's comparator.
        for (int index = 1; index < order.Count; index++)
        {
            PangyaUiNode candidate = order[index];
            int position = index;
            while (position > 0 && ComesBefore(candidate, order[position - 1]))
            {
                order[position] = order[position - 1];
                position--;
            }
            order[position] = candidate;
        }
        return options with { TopFocusedNode = node, TopmostOrder = order };

        bool ComesBefore(PangyaUiNode first, PangyaUiNode second)
        {
            if (ReferenceEquals(first.Parent, second)) return false;
            if (ReferenceEquals(second.Parent, first)) return true;
            return !ReferenceEquals(first, node) && ReferenceEquals(second, node);
        }
    }

    private static PangyaUiRenderOptions SetTopFocusCaption(PangyaUiNode node, bool focused,
        PangyaUiRenderOptions options)
    {
        if (!node.IsForm || node.IsLayout) return options;
        var text = options.FrameText?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        text[node] = (text.GetValueOrDefault(node) ?? new()) with { UseCaptionFocus = focused };
        return options with { FrameText = text };
    }

    private static int GetTopmostRank(PangyaUiNode node, IReadOnlyList<PangyaUiNode>? order)
    {
        if (order is null) return int.MaxValue;
        for (PangyaUiNode? current = node; current is not null; current = current.Parent)
            for (int index = 0; index < order.Count; index++)
                if (ReferenceEquals(order[index], current)) return index;
        return int.MaxValue;
    }

    private static bool IsTopmost(PangyaUiNode node, IReadOnlySet<PangyaUiNode> topmostNodes)
    {
        for (PangyaUiNode? current = node; current is not null; current = current.Parent)
            if (topmostNodes.Contains(current)) return true;
        return false;
    }

    private static bool HasVisibleParents(PangyaUiNode node, PangyaUiNode root,
        PangyaUiRenderOptions? options)
    {
        if (ReferenceEquals(node, root)) return true;
        for (PangyaUiNode? parent = node.Parent; parent is not null; parent = parent.Parent)
        {
            if (!IsRenderVisible(parent, options)) return false;
            if (ReferenceEquals(parent, root)) break;
        }
        return true;
    }

    public bool HasAnimatedControls(PangyaUiNode root, IReadOnlySet<string> symbols)
    {
        var pending = new Stack<PangyaUiNode>();
        var visited = new HashSet<PangyaUiNode>(ReferenceEqualityComparer.Instance);
        pending.Push(root);
        while (pending.TryPop(out PangyaUiNode? node))
        {
            if (!visited.Add(node) || !node.IsVisible(symbols) || !node.IsRenderVisible) continue;
            if (node.LayoutBase is not null && node.Element["anibg"] is XmlElement animation &&
                PangyaUiDimensionHelper.ParsePoint(animation.GetAttribute("vel")) is Point velocity &&
                !velocity.IsEmpty)
                return true;
            if (_resources.Emoticons is not null && IsEdit(node) &&
                node.Parameters.GetValueOrDefault("readonly") == "true")
                return true;
            if ((HasButtonSkin(node) && node.Parameters.GetValueOrDefault("blink") is { Length: > 0 } blink &&
                 GetBitmap(blink, symbols) is not null) ||
                (IsArea(node) && !string.IsNullOrWhiteSpace(node.Parameters.GetValueOrDefault("entrance_effect"))))
                return true;

            foreach (PangyaUiNode child in node.Children) pending.Push(child);
            if (ResolvePreviewReference(node, PangyaUiButtonState.Normal, symbols) is PangyaUiNode definition)
                pending.Push(definition);
        }
        return false;
    }

    public PangyaUiNode? HitTest(PangyaUiDocument document, PangyaUiNode? selectedForm,
        Point point, PangyaUiRenderOptions? options = null)
    {
        options ??= new PangyaUiRenderOptions();
        if (options.ButtonOwnerDrawBounds is not null) options = options with { ButtonOwnerDrawBounds = null };
        return GetRenderOrder(document, selectedForm, options.SelectedNode,
                options.EnabledSymbols, options.TopmostNodes, options)
            .LastOrDefault(node =>
            {
                if (IsRuntimeHidden(node, options)) return false;
                if (node.IsRefresh && string.Equals(node.Element["IsHitTestVisible"]?.GetAttribute("val"),
                        "False", StringComparison.OrdinalIgnoreCase))
                    return false;
                if (GetList(node, options) is not null && !options.ShowDebugBounds)
                    return HitTestListItem(node, point, options) >= 0;
                if (GetTabs(node, options) is not null && !options.ShowDebugBounds)
                    return HitTestTab(node, point, options) >= 0;
                if (IsTextButton(node) && !options.ShowDebugBounds)
                    return HitTestTextButtonSegment(node, point, options) >= 0;
                Rectangle bounds = GetRenderedBounds(node, options);
                return bounds.Width > 0 && bounds.Height > 0 && bounds.Contains(point);
            });
    }

    public Rectangle GetRenderedBounds(PangyaUiNode node,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new PangyaUiRenderOptions();
        if (IsRuntimeHidden(node, options)) return Rectangle.Empty;
        IReadOnlySet<string> symbols = Symbols(options);
        Rectangle absoluteBounds = GetAbsoluteBounds(node, options);
        if (node.IsRefresh)
            return IsRenderVisible(node, options) ? ClipRefreshBounds(node, absoluteBounds, options) : Rectangle.Empty;
        if (IsRenderVisible(node, options) && IsContextMenu(node))
        {
            using var bitmap = new Bitmap(1, 1);
            using Graphics metrics = Graphics.FromImage(bitmap);
            return RenderContextMenu(metrics, node, absoluteBounds.Location, options, draw: false, out _);
        }
        if (IsRenderVisible(node, options) && GetList(node, options) is { } list)
            return DrawList(null, node, absoluteBounds, list, options);
        if (IsRenderVisible(node, options) && GetTabs(node, options) is { } tabs)
            return RenderTabs(null, node, absoluteBounds.Location, tabs, symbols, options);
        if (!IsRenderVisible(node, options) || IsTransparent(node, options.ButtonState))
            return options.ShowDebugBounds ? absoluteBounds : Rectangle.Empty;
        if (IsTextButton(node) && !options.ShowDebugBounds)
        {
            string textButton = GetTextButtonCaption(node, options);
            if (textButton.Length == 0) return Rectangle.Empty;
            using var bitmap = new Bitmap(1, 1);
            using Graphics metrics = Graphics.FromImage(bitmap);
            DrawTextButton(metrics, node, absoluteBounds, textButton,
                options with { Pointer = null }, out Rectangle measuredBounds, draw: false);
            return measuredBounds;
        }
        string caption = GetStaticCaption(node, options);
        if ((options.IncludeCaptionVisualBounds || options.IncludeStaticCaptionBounds) &&
            caption.Length > 0 && !IsPrivacyHidden(node, options) &&
            node.Type.Equals("STATIC", StringComparison.OrdinalIgnoreCase))
            return MeasureStaticCaptionBounds(node, absoluteBounds, caption, options);
        if (!(node is { IsForm: true, IsLayout: false }) && !node.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) &&
            (node.LayoutBase is not null || caption.Length > 0 || GetTextButtonCaption(node, options).Length > 0))
            return absoluteBounds;
        if (IsAreaEntranceFinished(node, options)) return Rectangle.Empty;
        bool hasAreaOwner = IsArea(node) && options.AreaOwnerDraw?.ContainsKey(node) == true;
        if (IsViewer(node))
            return DrawViewer(null, node, absoluteBounds, options);
        if (IsEdit(node)) return MeasureEditBounds(node, absoluteBounds, symbols, options);
        if (IsGauge(node)) return MeasureGaugeBounds(node, absoluteBounds, symbols, options);
        if (IsImageGauge(node))
            return RenderImageGauge(null, node, absoluteBounds.Location, symbols, options);
        if (HasButtonSkin(node, options))
            return MeasureButtonBounds(node, absoluteBounds, symbols, options);

        string resource = GetRenderResource(node, options.ButtonState, options.AreaImages);
        string resourceType = IsArea(node) && options.AreaImages?.ContainsKey(node) == true ? "BITMAP" : GetPreviewReferenceType(node);
        Image? areaBitmap = IsArea(node) ? options.AreaBitmapImages?.GetValueOrDefault(node) : null;
        PangyaUiNode? definition = areaBitmap is null
            ? _resources.TryResolve(resource, resourceType, symbols) : null;
        if (GetBitmapFilename(definition) is string bitmapFilename)
        {
            resource = bitmapFilename;
            definition = null;
        }
        if (IsMacroItem(node))
        {
            if (definition is null) return Rectangle.Empty;
            Point hostOrigin = GetParentOffset(node, options);
            Rectangle expanded = GetReferencedBounds(node, options,
                new HashSet<PangyaUiNode>(ReferenceEqualityComparer.Instance), hostOrigin);
            if (!expanded.IsEmpty) expanded.Offset(hostOrigin);
            return expanded;
        }
        if (definition is not null)
        {
            Rectangle bounds = absoluteBounds;
            if (definition.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase))
                return GetBaseFrameBounds(bounds, definition, symbols, node.Parameters.GetValueOrDefault("bgimg"),
                    node, options);
            if (definition.IsForm && !definition.IsLayout)
                return GetInstantiatedFormBounds(definition, GetInstantiatedFormRectangle(node, definition, bounds, options),
                    ForResourceChildren(node, options),
                    new HashSet<PangyaUiNode>(ReferenceEqualityComparer.Instance));
            if (bounds.Width > 0 && bounds.Height > 0) return bounds;
            Rectangle referenced = GetReferencedBounds(definition, options,
                new HashSet<PangyaUiNode>(ReferenceEqualityComparer.Instance), bounds.Location);
            referenced.Offset(bounds.Location);
            return referenced;
        }
        Image? image = areaBitmap ?? (string.IsNullOrWhiteSpace(resource) ? null : _images.GetImage(resource));
        if (image is null) return hasAreaOwner || options.ShowDebugBounds ? absoluteBounds : Rectangle.Empty;
        Rectangle destination = ImageBounds(node, absoluteBounds, image);
        Rectangle? entranceClip = ApplyAreaEntrance(node, absoluteBounds, image, options, ref destination);
        Rectangle visibleImage = entranceClip ?? ClipImageBounds(node, absoluteBounds, destination, options);
        if (IsArea(node) && GetAreaTint(node, options).A == 0) visibleImage = Rectangle.Empty;
        if (!hasAreaOwner) return visibleImage;
        Rectangle? rendered = entranceClip.HasValue ? destination : absoluteBounds;
        IncludeBounds(ref rendered, visibleImage);
        return rendered ?? Rectangle.Empty;
    }

    private void DrawNode(Graphics graphics, PangyaUiNode node, Point offset,
        PangyaUiRenderOptions options, HashSet<PangyaUiNode> resourceChain,
        List<(PangyaUiNode Node, Rectangle Bounds, PangyaUiComboPreview Combo)> dropdowns,
        List<(PangyaUiNode Node, Point Offset, HashSet<PangyaUiNode> Chain, PangyaUiResourceInstance? Instance)> topmost, bool drawTopmost = false,
        bool queueDropdown = true)
    {
        if (IsRuntimeHidden(node, options)) return;
        if (options.DeferredDisplayContainers?.Contains(node) == true) return;
        if (!drawTopmost && resourceChain.Count > 0 && options.TopmostNodes?.Contains(node) == true)
        {
            topmost.Add((node, offset, new HashSet<PangyaUiNode>(resourceChain, ReferenceEqualityComparer.Instance), options.ResourceInstance));
            return;
        }
        Point windowOffset = GetWindowOffset(node, options);
        offset = new Point(ClampCoordinate((long)offset.X + windowOffset.X),
            ClampCoordinate((long)offset.Y + windowOffset.Y));
        if (IsRenderVisible(node, options) && GetList(node, options) is PangyaUiListPreview list)
        {
            var bounds = new Rectangle(ClampCoordinate((long)offset.X + GetWindowBounds(node, options).X),
                ClampCoordinate((long)offset.Y + GetWindowBounds(node, options).Y), GetWindowBounds(node, options).Width, GetWindowBounds(node, options).Height);
            DrawList(graphics, node, bounds, list, options);
            return;
        }
        if (IsRenderVisible(node, options) && GetTabs(node, options) is { } tabs)
        {
            RenderTabs(graphics, node, new Point(ClampCoordinate((long)offset.X + GetWindowBounds(node, options).X),
                ClampCoordinate((long)offset.Y + GetWindowBounds(node, options).Y)), tabs, Symbols(options), options);
            return;
        }
        if (IsGroupBox(node))
        {
            // The top-level render order already includes inline children.
            if (IsRenderVisible(node, options) && resourceChain.Count > 0)
            {
                Point childOffset = new(ClampCoordinate((long)offset.X + GetWindowBounds(node, options).X),
                    ClampCoordinate((long)offset.Y + GetWindowBounds(node, options).Y));
                foreach (PangyaUiNode child in node.Children.Where(child => child.IsVisible(Symbols(options))))
                    DrawNode(graphics, child, childOffset, options, resourceChain, dropdowns, topmost);
            }
            return;
        }
        if (IsRenderVisible(node, options) && node.IsForm && !node.IsLayout &&
            options.ModalForms?.Contains(node) == true)
        {
            bool modeless = options.FormFlags?.TryGetValue(node, out uint flags) == true
                ? (flags & 0x4) != 0 : node.Parameters.ContainsKey("modal");
            if (!modeless)
            {
                using var backdrop = new SolidBrush(Color.FromArgb(110, Color.Black));
                graphics.FillRectangle(backdrop, new Rectangle(Point.Empty, options.RenderCanvasSize));
            }
        }
        if (!IsRenderVisible(node, options) || IsTransparent(node, options.ButtonState)) return;
        IReadOnlySet<string> symbols = Symbols(options);
        Rectangle nodeBounds = GetWindowBounds(node, options);
        nodeBounds.Offset(offset);
        if (node.IsRefresh)
        {
            DrawRefreshTexture(graphics, node, nodeBounds, symbols, options);
            DrawRefreshText(graphics, node, nodeBounds, options);
            return;
        }
        if (IsContextMenu(node))
        {
            RenderContextMenu(graphics, node, nodeBounds.Location, options, draw: true, out _);
            return;
        }
        if (IsImageGauge(node))
        {
            RenderImageGauge(graphics, node, nodeBounds.Location, symbols, options);
            return;
        }
        if (IsGauge(node))
        {
            PangyaUiGaugeRange? range = options.GaugeRanges?.GetValueOrDefault(node);
            double value = options.GaugePositions?.TryGetValue(node, out float position) == true && float.IsFinite(position)
                ? position : options.GaugeValues?.GetValueOrDefault(node) ?? 0;
            if (IsExtendedGauge(node) && options.ExtendedGaugeSteps?.TryGetValue(node, out int step) == true)
            {
                int lower = range?.Lower ?? 0;
                int upper = range?.Upper ?? 100;
                value = lower + (upper - (double)lower) *
                    Math.Clamp(step / (double)GetExtendedGaugeStepCount(node, options), 0, 1);
            }
            DrawGauge(graphics, node, nodeBounds, value, symbols, options,
                enabled: options.DisabledNodes?.Contains(node) != true,
                expansion: options.GaugeExpansions?.GetValueOrDefault(node) ?? 0,
                range: range, transition: options.GaugeTransitions?.GetValueOrDefault(node));
            return;
        }
        if (IsEdit(node))
        {
            Rectangle editBounds = GetEditBounds(node, nodeBounds, symbols, options);
            PangyaUiComboPreview? combo = IsCombo(node) ? options.Combos?.GetValueOrDefault(node) : null;
            IReadOnlyList<string>? lines = options.EditLines?.GetValueOrDefault(node);
            PangyaUiComboItem? selected = combo is not null && (uint)combo.SelectedIndex < (uint)combo.Items.Count
                ? combo.Items[combo.SelectedIndex] : null;
            bool imageSelection = IsImageCombo(node);
            if (lines is null && selected is not null && !imageSelection)
                lines = [LimitComboText(node, selected.Text)];
            DrawEdit(graphics, node, editBounds, lines,
                enabled: options.DisabledNodes?.Contains(node) != true,
                caret: options.EditCarets?.GetValueOrDefault(node), options: options);
            if (IsCombo(node) && node.Parameters.ContainsKey("normal"))
            {
                Rectangle buttonBounds = GetComboButtonBounds(node, nodeBounds, editBounds, symbols);
                if (!buttonBounds.IsEmpty)
                {
                    PangyaUiRenderOptions buttonOptions = options;
                    if (options.ButtonState == PangyaUiButtonState.Normal)
                    {
                        if (ReferenceEquals(options.PressedButton, node))
                            buttonOptions = options with { ButtonState = PangyaUiButtonState.Selected };
                        else if (ReferenceEquals(options.HoveredNode, node) &&
                            options.Pointer is Point pointer && buttonBounds.Contains(pointer))
                            buttonOptions = options with { ButtonState = PangyaUiButtonState.Hover };
                    }
                    DrawButton(graphics, node, buttonBounds, buttonOptions);
                    if (GetSelectedComboArtwork(node, buttonBounds, combo, symbols) is { } artwork)
                        DrawGraphicsImage(graphics, artwork.Image, artwork.Bounds);
                }
            }
            if (queueDropdown && combo?.IsOpen == true)
                dropdowns.Add((node, editBounds, ResolveReferencedComboSide(combo, editBounds.Y, options)));
            return;
        }
        if (IsTextButton(node))
        {
            string textButton = GetTextButtonCaption(node, options);
            if (textButton.Length == 0) return;
            DrawTextButton(graphics, node, nodeBounds, textButton, options, out _);
            return;
        }
        if (HasButtonSkin(node, options))
        {
            PangyaUiRenderOptions buttonOptions = options;
            if (options.ButtonState == PangyaUiButtonState.Normal)
            {
                if (ReferenceEquals(options.PressedButton, node) ||
                    options.LatchedButtons?.Contains(node) == true ||
                    IsDefaultMakeRoomModeButton(node, options))
                    buttonOptions = options with { ButtonState = PangyaUiButtonState.Selected };
                else if (ReferenceEquals(options.HoveredNode, node))
                    buttonOptions = options with { ButtonState = PangyaUiButtonState.Hover };
            }
            Rectangle buttonBounds = GetButtonBounds(node, nodeBounds, symbols, options);
            if (buttonBounds == Rectangle.Empty) buttonBounds.Location = nodeBounds.Location;
            DrawButton(graphics, node, buttonBounds, buttonOptions);
            return;
        }
        if (IsViewer(node))
        {
            DrawViewer(graphics, node, nodeBounds, options);
            return;
        }
        if (node.LayoutBase is XmlElement layoutBase)
        {
            using var background = new SolidBrush(ParseColor(layoutBase.GetAttribute("color"),
                Color.FromArgb(46, 49, 55), implicitAlpha: false));
            graphics.FillRectangle(background, nodeBounds);
            Image? layoutImage = GetDesktopWallpaper(node, options, out bool showWallpaper);
            if (layoutImage is not null)
                DrawDesktopWallpaper(graphics, layoutImage, nodeBounds,
                    options.DesktopWallpaperTints?.GetValueOrDefault(node, options.DesktopWallpaperTint) ??
                    options.DesktopWallpaperTint);
            if (showWallpaper && node.Element["anibg"] is XmlElement animation &&
                GetBitmap(animation.GetAttribute("img"), symbols) is Image animationImage)
            {
                Rectangle destination = PangyaUiNode.ParseBounds(animation);
                int width = Math.Min(destination.Width, animationImage.Width);
                int height = Math.Min(destination.Height, animationImage.Height);
                if (width > 0 && height > 0)
                {
                    var imageBounds = new Rectangle(destination.X, destination.Y, width, height);
                    Rectangle clip = Rectangle.Intersect(imageBounds, nodeBounds);
                    if (clip.Width > 0 && clip.Height > 0)
                    {
                        Point velocity = PangyaUiDimensionHelper.ParsePoint(animation.GetAttribute("vel")) ?? Point.Empty;
                        double elapsed = Math.Max(0, options.LayoutAnimationTime.TotalSeconds);
                        PointF sourceOffset = options.DesktopAnimationOffsets?.TryGetValue(node, out PointF supplied) == true &&
                            float.IsFinite(supplied.X) && float.IsFinite(supplied.Y)
                            ? supplied
                            : new PointF(AnimatedSourceOffset(velocity.X, elapsed, animationImage.Width - width),
                                AnimatedSourceOffset(velocity.Y, elapsed, animationImage.Height - height));
                        PointF[] corners = [clip.Location, new(clip.Right, clip.Top), new(clip.Left, clip.Bottom)];
                        var source = new RectangleF(sourceOffset.X + clip.X - imageBounds.X,
                            sourceOffset.Y + clip.Y - imageBounds.Y, clip.Width, clip.Height);
                        graphics.DrawImage(animationImage, corners, source, GraphicsUnit.Pixel);
                    }
                }
            }
            return;
        }

        Rectangle areaOwnerBounds = nodeBounds;
        string resource = GetRenderResource(node, options.ButtonState, options.AreaImages);
        string resourceType = IsArea(node) && options.AreaImages?.ContainsKey(node) == true ? "BITMAP" : GetPreviewReferenceType(node);
        Image? areaBitmap = IsArea(node) ? options.AreaBitmapImages?.GetValueOrDefault(node) : null;
        PangyaUiNode? definition = areaBitmap is null
            ? _resources.TryResolve(resource, resourceType, symbols) : null;
        if (GetBitmapFilename(definition) is string bitmapFilename)
        {
            resource = bitmapFilename;
            definition = null;
        }
        if (definition is not null)
        {
            if (!resourceChain.Add(definition)) return;
            if (definition.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase))
                DrawFrame(graphics, nodeBounds, definition, symbols, IsFrameBlinkVisible(node, options),
                    node.Parameters.GetValueOrDefault("bgimg"), node, IsWindowViewFocused(node, options),
                    hideDescription: IsPrivacyHidden(node, options), textOptions: options, frameInstance: node);
            else
            {
                if (definition.IsForm && !definition.IsLayout)
                {
                    nodeBounds = GetInstantiatedFormRectangle(node, definition, nodeBounds, options);
                    string frameResource = definition.GetResource(options.ButtonState);
                    PangyaUiNode? frame = _resources.TryResolve(frameResource, "FRAME", symbols);
                    if (frame?.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) == true)
                        DrawFrame(graphics, nodeBounds, frame, symbols, IsFrameBlinkVisible(node, options),
                            textSource: definition, isViewFocused: IsWindowViewFocused(node, options),
                            hideDescription: IsPrivacyHidden(node, options) || IsPrivacyHidden(definition, options),
                            textOptions: options, frameInstance: node);
                }
                // Fresh creates macro children directly under the instance's parent.
                Point childOffset = IsMacroItem(node) ? offset : nodeBounds.Location;
                var childOptions = ForResourceChildren(node, options);
                foreach (PangyaUiNode child in definition.Children.Where(child =>
                             child.IsVisible(symbols)))
                    DrawNode(graphics, child, childOffset, childOptions, resourceChain, dropdowns, topmost);
            }
            resourceChain.Remove(definition);
            return;
        }

        if (IsMacroItem(node)) return;
        Image? image = areaBitmap ?? (string.IsNullOrWhiteSpace(resource) ? null : _images.GetImage(resource));
        if (image is not null)
        {
            Rectangle destination = ImageBounds(node, nodeBounds, image);
            Rectangle? entranceClip = ApplyAreaEntrance(node, nodeBounds, image, options, ref destination);
            if (entranceClip.HasValue) areaOwnerBounds = destination;
            Rectangle clip = entranceClip ?? ClipImageBounds(node, nodeBounds, destination, options);
            if (clip.Width > 0 && clip.Height > 0)
            {
                GraphicsState state = graphics.Save();
                try
                {
                    graphics.SetClip(clip, CombineMode.Intersect);
                    if (IsArea(node))
                    {
                        Color tint = GetAreaTint(node, options);
                        Rectangle visible = Rectangle.Intersect(clip, destination);
                        if (visible.Width > 0 && visible.Height > 0)
                        {
                            graphics.PixelOffsetMode = PixelOffsetMode.Half;
                            float scaleX = (float)image.Width / destination.Width;
                            float scaleY = (float)image.Height / destination.Height;
                            DrawTintedImage(graphics, image, visible, (visible.Y - destination.Y) * scaleY,
                                visible.Width * scaleX, visible.Height * scaleY, tint,
                                sourceX: (visible.X - destination.X) * scaleX);
                        }
                    }
                    else
                        DrawGraphicsImage(graphics, image, destination);
                }
                finally { graphics.Restore(state); }
            }
        }
        if (IsAreaEntranceFinished(node, options)) return;
        if (IsArea(node) && options.AreaOwnerDraw?.GetValueOrDefault(node) is { } drawArea)
        {
            GraphicsState state = graphics.Save();
            try { drawArea(graphics, areaOwnerBounds); }
            finally { graphics.Restore(state); }
        }
        string caption = GetStaticCaption(node, options);
        if (caption.Length > 0 && !IsPrivacyHidden(node, options))
        {
            node.Parameters.TryGetValue("color", out string? color);
            bool enabled = options.DisabledNodes?.Contains(node) != true;
            Color foreground = ParseColor(color, Color.Gray, implicitAlpha: false);
            (Color Foreground, Color Outline)? textColors =
                options.StaticTextColors?.TryGetValue(node, out var runtimeColors) == true ? runtimeColors : null;
            if (textColors.HasValue)
                foreground = textColors.Value.Foreground;
            double opacity = GetWindowAlphaScale(node, options) *
                (options.CaptionOpacity?.GetValueOrDefault(node, byte.MaxValue) ?? byte.MaxValue);
            using var brush = new SolidBrush(Color.FromArgb(enabled ? (int)(foreground.A * opacity / 255) : foreground.A * 2 / 5, foreground));
            using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
            int alignment = ReadDecimalPrefix(node.Parameters.GetValueOrDefault("align"));
            format.Alignment = alignment switch
            {
                1 => StringAlignment.Center,
                2 => StringAlignment.Far,
                _ => StringAlignment.Near
            };
            int style = ReadDecimalPrefix(node.Parameters.GetValueOrDefault("style"));
            int initialStyle = style;
            if (options.StaticTextStyles?.TryGetValue(node, out int runtimeStyle) == true)
                style = runtimeStyle;
            Color edge = (initialStyle & 6) != 0
                ? ParseColor(node.Parameters.GetValueOrDefault("color2"), Color.Transparent, implicitAlpha: false)
                : Color.White;
            if (textColors.HasValue)
                edge = textColors.Value.Outline;
            using var outline = new SolidBrush(enabled ? Color.FromArgb((int)(edge.A * opacity / 255), edge) : Color.White);
            bool smallFont = options.SmallFontCaptions?.Contains(node) == true;
            using Font? fallbackSmallFont = smallFont ? new Font(SystemFonts.DefaultFont.FontFamily, 11, GraphicsUnit.Pixel) : null;
            Font captionFont = fallbackSmallFont ?? SystemFonts.DefaultFont;
            var measureText = smallFont ? options.MeasureTooltipText : options.MeasureCaptionText;
            var drawText = smallFont ? options.DrawTooltipText : options.DrawCaptionText;
            int? fontHeight = smallFont ? options.TooltipFontHeight : options.CaptionFontHeight;
            Rectangle? clip = options.CaptionClips?.TryGetValue(node, out Rectangle customClip) == true ? customClip : null;
            float scroll = 0;
            if (IsScrollingCaption(node, options))
            {
                float width = MeasureCaptionLine(graphics, caption,
                    SystemFonts.DefaultFont, format, symbols: null, measureText: options.MeasureCaptionText);
                scroll = Math.Clamp(options.CaptionScrollOffsets?.GetValueOrDefault(node) ?? 0,
                    0, width > nodeBounds.Width ? width : 0);
                clip ??= nodeBounds;
            }
            if (smallFont || options.CaptionClippingEnabled?.GetValueOrDefault(node, true) == false)
                clip = null;

            DrawTaggedCaption(graphics, caption, brush, outline,
                new PointF(nodeBounds.X - scroll, nodeBounds.Y), format, style, captionFont,
                tagAlpha: enabled ? (byte)opacity : byte.MaxValue,
                emoticonSymbols: symbols, animationElapsed: options.EmoticonAnimationTime,
                lineHeight: fontHeight, measureText: measureText,
                drawText: drawText, parseTags: false, forceOpaqueText: true, glyphClip: clip);
        }
    }

    private static readonly System.Text.RegularExpressions.Regex CaptionTags = PangyaUiText.Tokens;

    private static readonly System.Text.RegularExpressions.Regex EditCaptionTags = new(
        @"\\c(?<tag>[^\\\r\n]*)\\c",
        System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    private static readonly System.Text.RegularExpressions.Regex EmoticonTokens = new(
        @"\((?<name>[^()\r\n]+)\)", System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    private bool TryGetEmoticon(string name, IReadOnlySet<string>? symbols, out Image? image,
        out Rectangle source, out bool flip, TimeSpan? animationElapsed = null)
    {
        flip = name.Contains('-');
        image = null;
        source = Rectangle.Empty;
        if (symbols is null || _resources.Emoticons is not { } emoticons)
            return false;
        if (!emoticons.TryGetFrame(name, animationElapsed, out string resource, out source))
            return false;
        image = GetBitmap(resource, symbols);
        return image is not null && new Rectangle(Point.Empty, image.Size).Contains(source);
    }

    private static bool HasEditAnimationMarker(ReadOnlySpan<char> text, bool startsWord)
    {
        for (int index = 0; index < text.Length; index++)
        {
            if (text[index] == '\b' && startsWord) return true;
            startsWord = text[index] is ' ' or '\t';
        }
        return false;
    }

    private static float GetEditAlignmentOffset(StringAlignment alignment, float difference) => alignment switch
    {
        StringAlignment.Center => MathF.Truncate(MathF.Truncate(difference) / 2),
        StringAlignment.Far => MathF.Truncate(difference),
        _ => 0
    };

    private static string TrimEditAlignmentSpace(string line) =>
        line.Length > 0 && line[^1] is (' ' or '\t') ? line[..^1] : line;

    private float MeasureCaptionLine(Graphics graphics, string line, Font font, StringFormat format,
        IReadOnlySet<string>? symbols, TimeSpan? animationElapsed = null, Func<string, float>? measureText = null)
    {
        int start = 0;
        float width = 0;
        foreach (System.Text.RegularExpressions.Match token in EmoticonTokens.Matches(line))
        {
            if (!TryGetEmoticon(token.Groups["name"].Value, symbols, out _, out Rectangle source,
                    out _, animationElapsed)) continue;
            width += MeasureRun(line[start..token.Index]) + source.Width;
            start = token.Index + token.Length;
        }
        return width + MeasureRun(line[start..]);

        float MeasureRun(string run) => measureText?.Invoke(run) ??
            graphics.MeasureString(run, font, PointF.Empty, format).Width;
    }

    private (Color Fill, Color Edge, int Style) DrawTaggedCaption(Graphics graphics, string caption, SolidBrush foreground,
        SolidBrush outline, PointF origin, StringFormat format, int style, Font? font = null,
        byte tagAlpha = byte.MaxValue, IReadOnlySet<string>? emoticonSymbols = null,
        TimeSpan? animationElapsed = null, bool useEditAnimationMarker = false, Color? defaultTagEdge = null, string? editAlignmentText = null, float editAvailableWidth = 0, bool justifyEditLine = false, bool splitEditWords = false, float lineSpacing = 0, float? lineHeight = null, Func<string, float>? measureText = null,
        Action<Graphics, string, Brush, PointF>? drawText = null, bool parseTags = true,
        bool forceOpaqueText = false, bool editTags = false, Color? defaultTagFill = null,
        bool measureEmoticons = true, Rectangle? glyphClip = null, bool preserveLiteralTokenBoundaries = false)
    {
        font ??= SystemFonts.DefaultFont;
        var tags = editTags ? EditCaptionTags : CaptionTags;
        if (!forceOpaqueText && !caption.Contains('\n') && drawText is null && lineSpacing == 0 && editAlignmentText is null && !useEditAnimationMarker && (!parseTags || !tags.IsMatch(caption)) && (emoticonSymbols is null || !EmoticonTokens.IsMatch(caption)))
        {
            DrawStyledText(graphics, caption, foreground, outline, origin, format, style, font, glyphClip: glyphClip);
            return (foreground.Color, outline.Color, style);
        }

        using var runFormat = (StringFormat)format.Clone();
        runFormat.Alignment = StringAlignment.Near;
        runFormat.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        using var fill = new SolidBrush(foreground.Color);
        using var edge = new SolidBrush(outline.Color);
        float y = origin.Y;
        foreach (string line in caption.Replace("\r\n", "\n").Split('\n'))
        {
            string visibleLine = editAlignmentText ?? (parseTags
                ? editTags ? tags.Replace(line, "") : PangyaUiText.RemoveTags(line, preserveLiteralTokenBoundaries) : line);
            if (useEditAnimationMarker) visibleLine = visibleLine.Replace("\b", "");
            TimeSpan? runAnimation = useEditAnimationMarker ? null : animationElapsed;
            float width = MeasureCaptionLine(graphics, visibleLine, font, runFormat,
                measureEmoticons ? emoticonSymbols : null, runAnimation, measureText);
            float x = editAlignmentText is not null
                ? origin.X + GetEditAlignmentOffset(format.Alignment, editAvailableWidth - width)
                : origin.X - (format.Alignment == StringAlignment.Center ? width / 2 :
                    format.Alignment == StringAlignment.Far ? width : 0);
            int gaps = justifyEditLine && format.Alignment == StringAlignment.Near
                ? visibleLine.Count(character => character is ' ' or '\t') : 0;
            float difference = MathF.Truncate(editAvailableWidth - width);
            float extraSpace = gaps > 0 ? MathF.Truncate(difference / gaps) : 0;
            float remainingSpace = gaps > 0 ? difference % gaps : 0;
            float spaceWidth = splitEditWords
                ? MathF.Truncate(measureText?.Invoke(" ") ?? graphics.MeasureString(" ", font, PointF.Empty, runFormat).Width) : 0;
            int start = 0;
            foreach (System.Text.RegularExpressions.Match match in parseTags ? tags.Matches(line).Cast<System.Text.RegularExpressions.Match>() : [])
            {
                if (!editTags && !PangyaUiText.IsTag(match.Groups["tag"].Value))
                {
                    if (preserveLiteralTokenBoundaries)
                    {
                        // FrTEXT emits a delimited literal as its own token.
                        DrawToken(line[start..match.Index]);
                        DrawRun(match.Value);
                        start = match.Index + match.Length;
                    }
                    continue;
                }
                DrawToken(line[start..match.Index]);
                string tag = editTags ? match.Groups["tag"].Value :
                    match.Groups["tag"].Value.Trim(' ', '\\', 'c');
                if (editTags)
                    ApplyEditTag(tag);
                else ApplyFreshTag(tag);
                start = match.Index + match.Length;
            }
            string finalRun = line[start..];
            DrawToken(splitEditWords ? TrimEditAlignmentSpace(finalRun) : finalRun);
            y += (lineHeight ?? font.GetHeight(graphics)) + lineSpacing;

            void DrawToken(string token)
            {
                if (parseTags && !editTags && preserveLiteralTokenBoundaries && PangyaUiText.IsPlainTokenTag(token))
                    ApplyFreshTag(token.Trim(' ', '\\', 'c'));
                else DrawRun(token);
            }

            void ApplyFreshTag(string tag)
            {
                if (tag.Length == 1)
                    style = char.ToLowerInvariant(tag[0]) switch { 't' => 1, 'o' => 2, 's' => 4, _ => 0 };
                else
                {
                    int separator = PangyaUiText.GetOutlineColorSeparator(tag);
                    Color fillColor = Color.FromArgb(unchecked((int)PangyaUiText.ParseTagColor(tag)));
                    Color edgeColor = separator >= 0
                        ? Color.FromArgb(unchecked((int)PangyaUiText.ParseTagColor(tag[(separator + 1)..])))
                        : defaultTagEdge ?? Color.White;
                    fill.Color = Color.FromArgb(fillColor.A * tagAlpha / 255, fillColor);
                    edge.Color = Color.FromArgb(edgeColor.A * tagAlpha / 255, edgeColor);
                }
            }

            void DrawRun(string run)
            {
                int start = 0;
                foreach (System.Text.RegularExpressions.Match token in EmoticonTokens.Matches(run))
                {
                    if (splitEditWords && token.ValueSpan.ContainsAny(' ', '\t')) continue;
                    if (useEditAnimationMarker && HasEditAnimationMarker(run.AsSpan(start, token.Index - start), start == 0))
                        runAnimation = animationElapsed;
                    if (!TryGetEmoticon(token.Groups["name"].Value, emoticonSymbols, out Image? image,
                            out Rectangle source, out bool flip, runAnimation) || image is null) continue;
                    DrawPlain(run[start..token.Index]);
                    float top = y + (lineHeight ?? font.GetHeight(graphics)) - source.Height;
                    float right = x + source.Width;
                    PointF[] corners = flip ? [new(right, top), new(x, top), new(right, top + source.Height)]
                        : [new(x, top), new(right, top), new(x, top + source.Height)];
                    DrawGraphicsImage(graphics, image, corners, source);
                    x = right;
                    start = token.Index + token.Length;
                }
                if (useEditAnimationMarker && HasEditAnimationMarker(run.AsSpan(start), start == 0))
                    runAnimation = animationElapsed;
                DrawPlain(run[start..]);
            }

            void DrawPlain(string run)
            {
                if (useEditAnimationMarker) run = run.Replace("\b", "");
                if (splitEditWords)
                {
                    int start = 0;
                    for (int index = 0; index < run.Length; index++)
                    {
                        if (run[index] != ' ') continue;
                        DrawTextRun(run[start..index]);
                        x += spaceWidth;
                        if (gaps > 0)
                        {
                            x += extraSpace;
                            if (--gaps == 0) x += remainingSpace;
                        }
                        start = index + 1;
                    }
                    run = run[start..];
                }
                DrawTextRun(run);
            }

            void DrawTextRun(string run)
            {
                if (run.Length == 0) return;
                if (forceOpaqueText)
                {
                    fill.Color = Color.FromArgb(byte.MaxValue, fill.Color);
                    edge.Color = Color.FromArgb(byte.MaxValue, edge.Color);
                }
                DrawStyledText(graphics, run, fill, edge, new PointF(x, y), runFormat, style, font, drawText, glyphClip);
                float runWidth = !editTags && drawText?.Target is IPangyaUiBitmapTextPasses bitmapText
                    ? bitmapText.GetDrawAdvance(run)
                    : measureText?.Invoke(run) ?? graphics.MeasureString(run, font, PointF.Empty, runFormat).Width;
                x += editAlignmentText is not null ? MathF.Truncate(runWidth) : runWidth;
            }
        }
        return (fill.Color, edge.Color, style);

        void ApplyEditTag(string tag)
        {
            Color fillColor = defaultTagFill ?? foreground.Color;
            Color edgeColor = defaultTagEdge ?? outline.Color;
            int colorCount = 0;
            foreach (string part in tag.Split(','))
            {
                string token = part.Trim();
                if (token.Length == 1)
                {
                    style = char.ToLowerInvariant(token[0]) switch
                    {
                        'n' => 0, 't' => 1, 'o' => 2, 's' => 4, _ => style
                    };
                    continue;
                }
                string hex = token.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? token[2..] : token;
                if (colorCount >= 2 || !uint.TryParse(hex, NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out uint value)) continue;
                Color color = Color.FromArgb(unchecked((int)value));
                if (colorCount == 0) fillColor = color;
                else edgeColor = color;
                colorCount++;
            }
            if (colorCount == 0) return;
            fill.Color = Color.FromArgb(fillColor.A * tagAlpha / byte.MaxValue, fillColor);
            edge.Color = Color.FromArgb(edgeColor.A * tagAlpha / byte.MaxValue, edgeColor);
        }
    }

    private static bool IsPrivacyHidden(PangyaUiNode node, PangyaUiRenderOptions options) =>
        options.HidePrivacy && options.PrivateNodes?.Contains(node) == true;

    private static bool IsRuntimeHidden(PangyaUiNode node, PangyaUiRenderOptions? options)
    {
        for (PangyaUiNode? current = node; current is not null; current = current.Parent)
        {
            // A macro definition supplies children but never becomes a FrWnd.
            if (!ReferenceEquals(current, node) && IsMacroDefinition(current)) continue;
            if (options?.HiddenNodes?.Contains(current) == true ||
                options?.WindowLifecycles?.GetValueOrDefault(current) is
                    PangyaUiWindowLifecycle.PendingInitialization or PangyaUiWindowLifecycle.PendingDestruction ||
                (!ReferenceEquals(current, node) && !IsRenderVisible(current, options)))
                return true;
        }
        return false;
    }

    private static PangyaUiNode? GetDeferredDisplayContainer(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        for (PangyaUiNode? current = node; current is not null; current = current.Parent)
        {
            if (options.DeferredDisplayContainers?.Contains(current) == true) return current;
            if (options.TopmostNodes?.Contains(current) == true) return null;
        }
        return null;
    }

    private static bool IsRenderVisible(PangyaUiNode node, PangyaUiRenderOptions? options) =>
        options?.VisibleNodes?.Contains(node) == true || node.IsRenderVisible;

    private Rectangle MeasureStaticCaptionBounds(PangyaUiNode node, Rectangle bounds, string caption,
        PangyaUiRenderOptions options, Point clipOrigin = default)
    {
        bool small = options.SmallFontCaptions?.Contains(node) == true;
        using var bitmap = new Bitmap(1, 1);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        using Font? smallFallback = small ? new Font(SystemFonts.DefaultFont.FontFamily, 11, GraphicsUnit.Pixel) : null;
        Font font = smallFallback ?? SystemFonts.DefaultFont;
        var measure = small ? options.MeasureTooltipText : options.MeasureCaptionText;
        var bitmapText = (small ? options.DrawTooltipText : options.DrawCaptionText)?.Target as IPangyaUiBitmapTextPasses;
        float height = (small ? options.TooltipFontHeight : options.CaptionFontHeight) ?? font.GetHeight(graphics);
        int alignment = ReadDecimalPrefix(node.Parameters.GetValueOrDefault("align"));
        int style = options.StaticTextStyles?.TryGetValue(node, out int runtimeStyle) == true
            ? runtimeStyle : ReadDecimalPrefix(node.Parameters.GetValueOrDefault("style"));
        Rectangle? measured = bounds;
        float y = bounds.Y;
        foreach (string line in caption.Replace("\r\n", "\n").Split('\n'))
        {
            float width = MeasureCaptionLine(graphics, line, font, format, Symbols(options), measureText: measure);
            float x = bounds.X - (alignment == 1 ? width / 2 : alignment == 2 ? width : 0);
            int edge = style == 2 ? 1 : 0;
            int right = style is 1 or 2 or 4 ? 1 : 0;
            int bottom = style is 2 or 4 ? 1 : 0;
            IncludeBounds(ref measured, MeasureCaptionLineArtwork(bitmapText, line, new PointF(x, y),
                style, height, Symbols(options), options.EmoticonAnimationTime));
            if (width > 0)
            {
                var ink = Rectangle.FromLTRB((int)MathF.Floor(x) - edge, (int)MathF.Floor(y) - edge,
                    (int)MathF.Ceiling(x + width) + right, (int)MathF.Ceiling(y + height) + bottom);
                if (!small && options.CaptionClippingEnabled?.GetValueOrDefault(node, true) != false)
                {
                    if (options.CaptionClips?.TryGetValue(node, out Rectangle clip) == true)
                        ink = IntersectAreaClip(ink, clip, clipOrigin);
                    else if (IsScrollingCaption(node, options))
                        ink = Rectangle.Intersect(ink, bounds);
                }
                IncludeBounds(ref measured, ink);
            }
            y += height;
        }
        return measured ?? bounds;
    }

    private static Rectangle MeasureCaptionArtwork(IPangyaUiBitmapTextPasses? bitmapText,
        string text, PointF origin, int style)
    {
        if (bitmapText is null || !text.Contains(@"\<", StringComparison.Ordinal)) return Rectangle.Empty;
        RectangleF overlay = bitmapText.GetOverlayBounds(text, origin);
        if (overlay.Width <= 0 || overlay.Height <= 0) return Rectangle.Empty;
        int edge = style == 2 ? 1 : 0;
        return Rectangle.FromLTRB(
            ClampCoordinate((long)MathF.Floor(overlay.Left) - edge),
            ClampCoordinate((long)MathF.Floor(overlay.Top) - edge),
            ClampCoordinate((long)MathF.Ceiling(overlay.Right) + (style is 1 or 2 or 4 ? 1 : 0)),
            ClampCoordinate((long)MathF.Ceiling(overlay.Bottom) + (style is 2 or 4 ? 1 : 0)));
    }

    private Rectangle MeasureCaptionLineArtwork(IPangyaUiBitmapTextPasses? bitmapText, string line,
        PointF origin, int style, float lineHeight, IReadOnlySet<string>? symbols, TimeSpan? elapsed,
        bool includeGlyphs = false)
    {
        if (bitmapText is null) return Rectangle.Empty;

        Rectangle? measured = null;
        int start = 0;
        foreach (System.Text.RegularExpressions.Match token in EmoticonTokens.Matches(line))
        {
            if (!TryGetEmoticon(token.Groups["name"].Value, symbols, out _, out Rectangle source, out _, elapsed))
                continue;
            origin.X += MeasureRun(line[start..token.Index]);
            float top = origin.Y + lineHeight - source.Height;
            IncludeBounds(ref measured, Rectangle.FromLTRB(
                ClampCoordinate((long)MathF.Floor(origin.X)), ClampCoordinate((long)MathF.Floor(top)),
                ClampCoordinate((long)MathF.Ceiling(origin.X + source.Width)),
                ClampCoordinate((long)MathF.Ceiling(top + source.Height))));
            origin.X += source.Width;
            start = token.Index + token.Length;
        }
        MeasureRun(line[start..]);
        return measured ?? Rectangle.Empty;

        float MeasureRun(string run)
        {
            IncludeBounds(ref measured, MeasureCaptionArtwork(bitmapText, run, origin, style));
            float width = bitmapText.GetDrawAdvance(run);
            if (includeGlyphs)
                IncludeBounds(ref measured, MeasureGlyphExtent(origin, width, lineHeight, style));
            return width;
        }
    }

    private static readonly System.Text.Encoding StaticCaptionEncoding =
        System.Text.CodePagesEncodingProvider.Instance.GetEncoding(932)
        ?? throw new InvalidOperationException("Shift-JIS encoding is unavailable.");

    private static string GetStaticCaption(PangyaUiNode node, PangyaUiRenderOptions? options)
    {
        if (!node.Type.Equals("STATIC", StringComparison.OrdinalIgnoreCase))
            return node.StaticCaption;
        string caption = options?.StaticCaptions?.GetValueOrDefault(node) ?? TranslateXmlText(node.StaticCaption, options);
        if (options?.SmallFontCaptions?.Contains(node) == true ||
            options?.TruncatedStaticCaptions?.GetValueOrDefault(node) == caption) return caption;
        if (caption.Length <= 50 || StaticCaptionEncoding.GetByteCount(caption) <= 100) return caption;

        return TruncateShiftJis(caption, 100) + "...";
    }

    public static PangyaUiRenderOptions SetStaticCaption(PangyaUiNode node, string caption,
        bool smallFont = false, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(caption);
        if (!node.Type.Equals("STATIC", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Expected a static caption.", nameof(node));
        options ??= new();
        var captions = options.StaticCaptions?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var truncated = options.TruncatedStaticCaptions?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        if (!smallFont && StaticCaptionEncoding.GetByteCount(caption) > 100)
            captions[node] = truncated[node] = TruncateShiftJis(caption, 100) + "...";
        else
        {
            captions[node] = caption;
            truncated.Remove(node);
        }
        options = options with { TruncatedStaticCaptions = truncated };
        var fonts = options.SmallFontCaptions?.ToHashSet() ?? [];
        if (smallFont) fonts.Add(node);
        if (smallFont || !string.Equals(node.Parameters.GetValueOrDefault("scrolling"), "true",
                StringComparison.OrdinalIgnoreCase))
            return options with { StaticCaptions = captions, SmallFontCaptions = fonts };
        var scrolling = options.CaptionScrollOffsets?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        scrolling[node] = 0;
        return SetCaptionClippingArea(node, GetAbsoluteBounds(node, options), options with
        {
            StaticCaptions = captions, SmallFontCaptions = fonts, CaptionScrollOffsets = scrolling
        });
    }

    public static PangyaUiRenderOptions SetTextButtonCaption(PangyaUiNode node, string caption,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(caption);
        if (!IsTextButton(node)) throw new ArgumentException("Expected a text button.", nameof(node));
        caption = ReadNullTerminatedText(caption);
        options ??= new();
        var captions = options.TextButtonCaptions?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var states = options.MenuStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        using var bitmap = new Bitmap(1, 1);
        using Graphics metrics = Graphics.FromImage(bitmap);
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        float Measure(string text) => options.MeasureCaptionText?.Invoke(text) ??
            metrics.MeasureString(text, SystemFonts.DefaultFont, PointF.Empty, format).Width;
        Rectangle bounds = GetWindowBounds(node, options);
        bool initialized = options.TextButtonCaptionGeometryNodes?.Contains(node) == true;
        bool centered = IsTextButtonCentered(node);
        string separator = GetTextButtonSeparator(options);
        string originalCaption = GetTextButtonCaption(node, options);
        float currentWidth = initialized ? options.TextButtonCaptionWidths?.GetValueOrDefault(node, bounds.Width) ?? bounds.Width
            : MeasureTextButtonCaptionWidth(originalCaption, separator, bounds.Width, centered, Measure);
        int initialShift = !initialized && centered ? (int)((bounds.Width - Measure(originalCaption) + 1) / 2) : 0;
        int shift = centered ? (int)((currentWidth - Measure(caption) + 1) / 2) : 0;
        float width = MeasureTextButtonCaptionWidth(caption, separator, currentWidth, centered, Measure);
        var offsets = options.WindowOffsets?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        Point previousOffset = GetWindowOffset(node, options);
        offsets[node] = new Point(ClampCoordinate((long)previousOffset.X + initialShift + shift), previousOffset.Y);
        var sizes = options.WindowSizes?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        sizes[node] = new Size(Math.Max(0, (int)Math.Ceiling(width)), bounds.Height);
        var widths = options.TextButtonCaptionWidths?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        widths[node] = width;
        var geometry = options.TextButtonCaptionGeometryNodes?.ToHashSet() ?? [];
        geometry.Add(node);
        captions[node] = caption;
        states[node] = (states.GetValueOrDefault(node) ?? new()) with { SelectedRow = -1 };
        return options with { TextButtonCaptions = captions, MenuStates = states,
            WindowOffsets = offsets, WindowSizes = sizes, TextButtonCaptionGeometryNodes = geometry, TextButtonCaptionWidths = widths };

    }

    private static bool IsTextButtonCentered(PangyaUiNode node) =>
        !node.Parameters.TryGetValue("center", out string? center) || center.Equals("true", StringComparison.OrdinalIgnoreCase);

    private static float MeasureTextButtonCaptionWidth(string text, string separator, float previousWidth,
        bool centered, Func<string, float> measure)
    {
        float result = 0;
        int count = 0;
        float separatorWidth = measure(separator);
        foreach (string segment in text.Split(separator, StringSplitOptions.None))
        {
            if (segment.Length == 0) break;
            if (count++ > 0) result += separatorWidth;
            result += measure(segment);
        }
        return count > 0 ? result : centered ? measure(text) : previousWidth;
    }

    public static PangyaUiRenderOptions SelectTextButton(PangyaUiNode node, int index,
        bool selected = true, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsTextButton(node)) throw new ArgumentException("Expected a text button.", nameof(node));
        options ??= new();
        int count = GetTextButtonCaption(node, options).Split(GetTextButtonSeparator(options), StringSplitOptions.None)
            .TakeWhile(segment => segment.Length > 0).Count();
        if (count == 0) return options;
        int target = Math.Clamp(index, 0, count - 1);
        var states = options.MenuStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        PangyaUiMenuState state = states.GetValueOrDefault(node) ?? new();
        if (!selected && state.SelectedRow != target) return options;
        states[node] = state with { SelectedRow = selected ? target : -1 };
        return options with { MenuStates = states };
    }

    public static string? GetSelectedTextButtonText(PangyaUiNode node, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsTextButton(node)) throw new ArgumentException("Expected a text button.", nameof(node));
        int selected = options?.MenuStates?.GetValueOrDefault(node)?.SelectedRow ?? -1;
        return selected < 0 ? null : GetTextButtonCaption(node, options)
            .Split(GetTextButtonSeparator(options), StringSplitOptions.None)
            .TakeWhile(segment => segment.Length > 0).ElementAtOrDefault(selected);
    }

    public string? GetTextButtonTextUnderCursor(PangyaUiNode node, Point point, PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (!IsTextButton(node)) throw new ArgumentException("Expected a text button.", nameof(node));
        int index = HitTestTextButtonSegment(node, point, options);
        return index < 0 ? null : GetTextButtonCaption(node, options)
            .Split(GetTextButtonSeparator(options), StringSplitOptions.None)
            .TakeWhile(segment => segment.Length > 0).ElementAtOrDefault(index);
    }

    public static PangyaUiRenderOptions SetCaptionClippingArea(PangyaUiNode node, Rectangle? area,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new();
        var clips = options.CaptionClips?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var enabled = options.CaptionClippingEnabled?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        if (area is Rectangle rectangle) clips[node] = rectangle;
        else clips.Remove(node);
        enabled[node] = area.HasValue;
        return options with { CaptionClips = clips, CaptionClippingEnabled = enabled };
    }

    private static bool IsTextButton(PangyaUiNode node) =>
        node.Type.Equals("TEXTBUTTON", StringComparison.OrdinalIgnoreCase);

    private static string GetTextButtonCaption(PangyaUiNode node, PangyaUiRenderOptions? options) =>
        IsTextButton(node)
            ? ReadNullTerminatedText(options?.TextButtonCaptions?.GetValueOrDefault(node) ?? TranslateXmlText(node.TextButtonCaption, options))
            : node.TextButtonCaption;

    private static string GetTextButtonSeparator(PangyaUiRenderOptions? options) =>
        TranslateXmlText("|", options) is { Length: > 0 } separator ? separator : "|";

    private static string TranslateXmlText(string text, PangyaUiRenderOptions? options) =>
        options?.LanguageTable?.Translate(text) ?? text;

    private static string LimitComboText(PangyaUiNode node, string text)
    {
        int limit = int.TryParse(node.Parameters.GetValueOrDefault("charlimit"), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out int value) ? Math.Max(0, value) : 255;
        return TruncateShiftJis(text, limit);
    }

    private static string TruncateShiftJis(string text, int limit)
    {
        if (limit == 0) return string.Empty;
        if (StaticCaptionEncoding.GetByteCount(text) <= limit) return text;
        int charsUsed = 0;
        foreach (Rune rune in text.EnumerateRunes())
        {
            int length = rune.Utf16SequenceLength;
            int bytes = StaticCaptionEncoding.GetByteCount(text.AsSpan(charsUsed, length));
            if (bytes > limit) break;
            limit -= bytes;
            charsUsed += length;
        }
        return text[..charsUsed];
    }
    public static bool IsScrollingCaption(PangyaUiNode node, PangyaUiRenderOptions? options = null) =>
        !IsRuntimeHidden(node, options) && GetStaticCaption(node, options).Length > 0 &&
        GetWindowBounds(node, options).Width > 0 && GetWindowBounds(node, options).Height > 0 &&
        string.Equals(node.Parameters.GetValueOrDefault("scrolling"), "true", StringComparison.OrdinalIgnoreCase);

    public float GetCaptionScrollLimit(PangyaUiNode node, IReadOnlySet<string> symbols, PangyaUiRenderOptions? options = null)
    {
        if (!IsScrollingCaption(node, options)) return 0;
        using var bitmap = new Bitmap(1, 1);
        using Graphics graphics = Graphics.FromImage(bitmap);
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        return Math.Max(0, MeasureCaptionLine(graphics, GetStaticCaption(node, options),
            SystemFonts.DefaultFont, format, symbols: null,
            measureText: options?.MeasureCaptionText) - GetWindowBounds(node, options).Width);
    }

    private static void DrawStyledText(Graphics graphics, string text, Brush foreground, Brush outline,
        PointF origin, StringFormat format, int style, Font? font = null,
        Action<Graphics, string, Brush, PointF>? drawText = null, Rectangle? glyphClip = null)
    {
        font ??= SystemFonts.DefaultFont;
        if (drawText?.Target is IPangyaUiBitmapTextPasses bitmapText &&
            text.Contains(@"\<", StringComparison.Ordinal))
        {
            DrawPass((target, run, brush, point) => bitmapText.DrawOverlays(target, run, brush, point));
            DrawClipped(bitmapText.DrawGlyphs);
        }
        else DrawClipped(drawText);

        void DrawClipped(Action<Graphics, string, Brush, PointF>? draw)
        {
            if (glyphClip is not Rectangle clip)
            {
                DrawPass(draw);
                return;
            }
            GraphicsState state = graphics.Save();
            try
            {
                graphics.SetClip(clip, CombineMode.Intersect);
                DrawPass(draw);
            }
            finally { graphics.Restore(state); }
        }

        void DrawPass(Action<Graphics, string, Brush, PointF>? draw)
        {
            switch (style)
            {
                case 1:
                    DrawOffset(foreground, 1, 0);
                    break;
                case 2:
                    ReadOnlySpan<Point> offsets =
                    [
                        new(0, -1), new(-1, 0), new(1, 0), new(0, 1),
                        new(-1, -1), new(1, -1), new(-1, 1), new(1, 1)
                    ];
                    foreach (Point offset in offsets) DrawOffset(outline, offset.X, offset.Y);
                    break;
                case 4:
                    DrawOffset(outline, 1, 1);
                    break;
            }
            DrawOffset(foreground, 0, 0);

            void DrawOffset(Brush brush, int dx, int dy)
            {
                var point = new PointF(origin.X + dx, origin.Y + dy);
                using SolidBrush? faded = brush is SolidBrush solid && GetGraphicsAlpha(graphics) != 1
                    ? new SolidBrush(Color.FromArgb((int)(solid.Color.A * GetGraphicsAlpha(graphics)), solid.Color)) : null;
                Brush effective = faded ?? brush;
                if (draw is null) graphics.DrawString(text, font, effective, point, format);
                else draw(graphics, text, effective, point);
            }
        }
    }

    private void DrawRefreshTexture(Graphics graphics, PangyaUiNode node, Rectangle bounds,
        IReadOnlySet<string> symbols, PangyaUiRenderOptions options)
    {
        if (node.Element["Assets"] is not XmlElement assets ||
            node.Element.OwnerDocument?.DocumentElement?["Textures"] is not XmlElement textures)
            return;
        bool check = node.Type.Equals("CHECK", StringComparison.OrdinalIgnoreCase);
        string preferred = options.DisabledNodes?.Contains(node) == true ? "Disable" : options.ButtonState switch
        {
            _ when check && (options.ButtonState == PangyaUiButtonState.Selected ||
                options.LatchedButtons?.Contains(node) == true) => "Check",
            PangyaUiButtonState.Hover => "Hover",
            PangyaUiButtonState.Selected => "Push",
            _ => "Normal"
        };
        XmlElement? texture = FindTexture(preferred) ??
            (preferred == "Normal" ? null : FindTexture("Normal")) ?? FindTexture("Default");
        if (texture is null) return;
        Rectangle clip = ClipRefreshBounds(node, bounds, options);
        if (clip.Width <= 0 || clip.Height <= 0) return;
        Image?[] images = texture.ChildNodes.OfType<XmlElement>().Where(element => element.Name == "Image")
            .Take(9).Select(element => GetRefreshImage(element, symbols)).ToArray();
        GraphicsState state = graphics.Save();
        try
        {
            graphics.SetClip(clip, CombineMode.Intersect);
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            if (texture.GetAttribute("Type") == "Single" && images.FirstOrDefault() is Image image)
                DrawGraphicsImage(graphics, image, bounds);
            else if (texture.GetAttribute("Type") == "Frame" && images.Length == 9)
            {
                // Refresh stores the center first, then the corners, then the edges.
                ReadOnlySpan<int> order = [1, 5, 3, 7, 0, 8, 2, 6, 4];
                int left = Math.Min(bounds.Width, images[1]?.Width ?? images[7]?.Width ?? 0);
                int right = Math.Min(bounds.Width - left, images[3]?.Width ?? images[8]?.Width ?? 0);
                int top = Math.Min(bounds.Height, images[1]?.Height ?? images[5]?.Height ?? 0);
                int bottom = Math.Min(bounds.Height - top, images[2]?.Height ?? images[6]?.Height ?? 0);
                ReadOnlySpan<int> widths = [left, bounds.Width - left - right, right];
                ReadOnlySpan<int> heights = [top, bounds.Height - top - bottom, bottom];
                int y = bounds.Y;
                for (int row = 0; row < 3; row++)
                {
                    int x = bounds.X;
                    for (int column = 0; column < 3; column++)
                    {
                        if (images[order[row * 3 + column]] is Image slice && widths[column] > 0 && heights[row] > 0)
                            DrawGraphicsImage(graphics, slice, new Rectangle(x, y, widths[column], heights[row]));
                        x += widths[column];
                    }
                    y += heights[row];
                }
            }
        }
        finally { graphics.Restore(state); }

        XmlElement? FindTexture(string state)
        {
            string? name = assets.ChildNodes.OfType<XmlElement>()
                .FirstOrDefault(element => element.Name == "Asset" && element.GetAttribute("Name") == state)
                ?.GetAttribute("TextureName");
            return string.IsNullOrEmpty(name) ? null : textures.ChildNodes.OfType<XmlElement>()
                .FirstOrDefault(element => element.Name == "Texture" && element.GetAttribute("Name") == name);
        }
    }

    private Image? GetRefreshImage(XmlElement element, IReadOnlySet<string> symbols)
    {
        string name = element.GetAttribute("Path").Trim();
        if (name.Length == 0 || Path.IsPathRooted(name) ||
            (Path.HasExtension(name) && !PangyaImageLoader.IsSupportedPath(name)))
            return null;
        if (_images is PangyaFileImageProvider files &&
            Uri.TryCreate(element.BaseURI, UriKind.Absolute, out Uri? source) && source.IsFile &&
            Path.GetDirectoryName(source.LocalPath) is { } directory)
        {
            try
            {
                string candidate = Path.GetFullPath(name, directory);
                string root = Path.TrimEndingDirectorySeparator(files.DataRoot) + Path.DirectorySeparatorChar;
                if (candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase) &&
                    File.Exists(candidate) && files.GetImageByPath(candidate) is Image local)
                    return local;
            }
            catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
            {
                return null;
            }
        }
        return GetBitmap(name, symbols);
    }

    private static void DrawRefreshText(Graphics graphics, PangyaUiNode node, Rectangle bounds,
        PangyaUiRenderOptions options)
    {
        bool textArea = node.Type.Equals("TEXTAREA", StringComparison.OrdinalIgnoreCase);
        bool edit = node.Type.Equals("EDIT", StringComparison.OrdinalIgnoreCase);
        if (!textArea && !edit && !node.Type.Equals("LABEL", StringComparison.OrdinalIgnoreCase)) return;
        string text = edit && options.EditLines?.GetValueOrDefault(node) is { } lines
            ? string.Join(" ", lines) : Value(textArea || edit ? "Text" : "label");
        if (text.Length == 0) return;
        if (textArea) text = text.Replace("\\n", "\n", StringComparison.Ordinal);
        if (edit && Value("passwordchar") is { Length: > 0 } password)
        {
            string mask = Rune.GetRuneAt(password, 0).ToString();
            text = string.Concat(text.EnumerateRunes().Select(_ => mask));
        }
        Rectangle clip = ClipRefreshBounds(node, bounds, options);
        if (clip.Width <= 0 || clip.Height <= 0) return;

        string family = Value("FontName");
        float size = float.TryParse(Value("FontSize"), NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) &&
            float.IsFinite(parsed) && parsed > 0 ? Math.Min(parsed, 512) : 12;
        using Font font = CreateRefreshFont(family, size, text);
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        if (edit) format.FormatFlags |= StringFormatFlags.NoWrap;
        format.Alignment = Alignment("halign");
        format.LineAlignment = Alignment("valign");
        format.Trimming = Value("TextTrimming") == "CharacterEllipsis"
            ? StringTrimming.EllipsisCharacter : StringTrimming.None;

        string[] padding = Value("Padding").Split(',');
        RectangleF textBounds = RectangleF.FromLTRB(
            bounds.X + (float)ReadPadding("l", 0), bounds.Y + (float)ReadPadding("t", 1),
            bounds.X + (float)bounds.Width - ReadPadding("r", 2),
            bounds.Y + (float)bounds.Height - ReadPadding("b", 3));
        if (textBounds.Width <= 0 || textBounds.Height <= 0) return;
        string color = Value(edit ? "Color" : "color");
        Color foreground = color.Length == 9 && color[0] == '#' &&
            uint.TryParse(color.AsSpan(1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint rgba)
            ? Color.FromArgb(unchecked((int)((rgba >> 8) | (rgba << 24)))) : Color.White;
        using var brush = new SolidBrush(foreground);
        GraphicsState state = graphics.Save();
        try
        {
            graphics.SetClip(clip, CombineMode.Intersect);
            graphics.DrawString(text, font, brush, textBounds, format);
        }
        finally { graphics.Restore(state); }

        string Value(string name) => node.Element[name]?.GetAttribute("val") ?? string.Empty;
        int ReadPadding(string edge, int index) => textArea || edit
            ? ReadDecimalPrefix(node.Element["padding"]?.GetAttribute(edge))
            : padding.Length == 4 ? ReadDecimalPrefix(padding[index]) : 0;
        StringAlignment Alignment(string name) => Value(name) switch
        {
            "1" => StringAlignment.Center,
            "2" => StringAlignment.Far,
            _ => StringAlignment.Near
        };
    }

    private static Font CreateRefreshFont(string name, float size, string text)
    {
        Font? declared = TryCreate(name, size);
        if (declared is not null) return declared;
        return name switch
        {
            "나눔명조" => TryCreate("NanumMyeongjo", size) ?? TryCreate("Batang", size) ?? TryCreate("Malgun Gothic", size),
            "나눔고딕" => TryCreate("NanumGothic", size) ?? TryCreate("Malgun Gothic", size),
            "NtreevSoft" => TryCreate("Malgun Gothic", size),
            _ when text.Any(character => character is >= '\uac00' and <= '\ud7a3') => TryCreate("Malgun Gothic", size),
            _ => null
        } ?? new Font(SystemFonts.DefaultFont.FontFamily, size, GraphicsUnit.Pixel);

        static Font? TryCreate(string familyName, float pixelSize)
        {
            if (string.IsNullOrWhiteSpace(familyName)) return null;
            try
            {
                using var family = new FontFamily(familyName);
                return new Font(family, pixelSize, GraphicsUnit.Pixel);
            }
            catch (ArgumentException) { return null; }
        }
    }

    private static Rectangle ClipRefreshBounds(PangyaUiNode node, Rectangle bounds, PangyaUiRenderOptions options)
    {
        for (PangyaUiNode? parent = node.Parent; parent is not null; parent = parent.Parent)
            if (string.Equals(parent.Element["ClipChildren"]?.GetAttribute("val"), "True", StringComparison.OrdinalIgnoreCase))
                bounds = Rectangle.Intersect(bounds, GetAbsoluteBounds(parent, options));
        return bounds;
    }

    private static bool IsAreaEntranceFinished(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        if (options.SkipAreaEntranceEffects || !IsArea(node) ||
            node.Parameters.GetValueOrDefault("entrance_effect") is not { Length: > 0 } effect)
            return false;
        var (opening, duration, _) = ReadAreaEntrance(effect);
        return duration >= 0 && options.LayoutAnimationTime.TotalSeconds - opening - duration >= 0.2;
    }

    private static (double Opening, double Duration, string Direction) ReadAreaEntrance(string effect)
    {
        double opening = 1;
        double duration = -1;
        string direction = "top";
        bool startsOffscreen = false;
        foreach (string token in effect.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            int separator = token.IndexOf(':');
            if (separator < 0) continue;
            string key = token[..separator].ToLowerInvariant();
            string value = token[(separator + 1)..];
            if (key == "from")
            {
                startsOffscreen = true;
                string candidate = value.ToLowerInvariant();
                if (candidate is "top" or "bottom" or "left" or "right") direction = candidate;
            }
            else if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture,
                         out double seconds) && double.IsFinite(seconds))
            {
                if (key == "openingtime") opening = Math.Max(0, seconds);
                if (key == "duration") duration = seconds;
            }
        }
        return (startsOffscreen ? opening : 0, duration, direction);
    }

    private static Rectangle? ApplyAreaEntrance(PangyaUiNode node, Rectangle bounds, Image image,
        PangyaUiRenderOptions options, ref Rectangle destination)
    {
        if (options.SkipAreaEntranceEffects || !IsArea(node) ||
            node.Parameters.GetValueOrDefault("entrance_effect") is not { Length: > 0 } effect)
            return null;

        var (opening, duration, direction) = ReadAreaEntrance(effect);
        double elapsed = Math.Max(0, options.LayoutAnimationTime.TotalSeconds);
        double closingElapsed = elapsed - opening - duration;
        if (duration >= 0 && closingElapsed >= 0.2) return Rectangle.Empty;
        double displacement = opening > 0 && elapsed < opening ? 1 - elapsed / opening :
            duration >= 0 && closingElapsed > 0 ? closingElapsed / 0.2 : 0;
        var viewport = new Rectangle(bounds.Location, image.Size);
        long dx = direction is "left" or "right" ? (long)(image.Width * displacement) : 0;
        long dy = direction is "top" or "bottom" ? (long)(image.Height * displacement) : 0;
        destination = new Rectangle(
            ClampCoordinate(bounds.X + (direction == "left" ? -dx : dx)),
            ClampCoordinate(bounds.Y + (direction == "top" ? -dy : dy)),
            image.Width, image.Height);
        return Rectangle.Intersect(viewport, destination);
    }

    private static float AnimatedSourceOffset(int velocity, double elapsed, int travel)
    {
        if (travel <= 0 || velocity == 0 || elapsed <= 0) return 0;
        if (velocity < 0)
        {
            double offset = (velocity * elapsed) % travel;
            return (float)(offset < 0 ? offset + travel : offset);
        }

        // Replay Fresh's positive-edge reset at a nominal 60 Hz.
        const double framesPerSecond = 60;
        double frames = elapsed * framesPerSecond;
        double completed = Math.Floor(frames);
        double step = velocity / framesPerSecond;
        double cycle = Math.Max(1, Math.Ceiling(travel / step) - 1);
        double offsetAtFrame = completed > 0 ? ((completed - 1) % cycle + 1) * step : 0;
        double remainder = (frames - completed) * step;
        return (float)(remainder > 0 && offsetAtFrame + remainder >= travel
            ? remainder : offsetAtFrame + remainder);
    }
    private static bool IsContextMenu(PangyaUiNode node) =>
        node.Type.Equals("CONTEXTMENU", StringComparison.OrdinalIgnoreCase);

    public static PangyaUiRenderOptions SetContextMenuList(PangyaUiNode node, string caption,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(caption);
        if (!IsContextMenu(node)) throw new ArgumentException("Expected a context menu.", nameof(node));
        options ??= new();
        var captions = options.ContextMenuCaptions?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        captions[node] = caption;
        var states = options.MenuStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        states[node] = (states.GetValueOrDefault(node) ?? new()) with
        {
            SelectedRow = -1, DisabledRows = null, DisabledTexts = null, TextReplacements = null
        };
        var texts = options.ContextMenuItemTexts?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        texts.Remove(node);
        return options with
        {
            ContextMenuCaptions = captions, MenuStates = states, ContextMenuItemTexts = texts,
            HoveredNode = ReferenceEquals(options.HoveredNode, node) ? null : options.HoveredNode
        };
    }

    private static string GetContextMenuCaption(PangyaUiNode node, PangyaUiRenderOptions options) =>
        IsContextMenu(node)
            ? options.ContextMenuCaptions?.GetValueOrDefault(node) ?? node.ContextMenuCaption
            : node.ContextMenuCaption;

    public static PangyaUiRenderOptions DisableContextMenuItems(PangyaUiNode node, string text,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(text);
        if (!IsContextMenu(node)) throw new ArgumentException("Expected a context menu.", nameof(node));
        options ??= new();
        var states = options.MenuStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        PangyaUiMenuState state = states.GetValueOrDefault(node) ?? new();
        var disabled = state.DisabledRows?.ToHashSet() ?? [];
        string target = ReadNullTerminatedText(text);
        int index = 0;
        foreach (var (label, _) in GetMenuEntries(node, options))
        {
            if (string.Equals(GetMenuRowText(node, options, state, index, label), target, StringComparison.Ordinal))
                disabled.Add(index);
            index++;
        }
        states[node] = state with { DisabledRows = disabled };
        return options with { MenuStates = states };
    }

    private static string ReadNullTerminatedText(string text) =>
        text.IndexOf('\0') is int end and >= 0 ? text[..end] : text;

    public static PangyaUiRenderOptions ReplaceContextMenuItems(PangyaUiNode node, string source, string destination,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(destination);
        if (!IsContextMenu(node)) throw new ArgumentException("Expected a context menu.", nameof(node));
        options ??= new();
        source = ReadNullTerminatedText(source);
        destination = ReadNullTerminatedText(destination);
        var states = options.MenuStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        PangyaUiMenuState state = states.GetValueOrDefault(node) ?? new();
        var disabled = state.DisabledRows?.ToHashSet() ?? [];
        var rows = new Dictionary<int, string>();
        int index = 0;
        foreach (var (label, _) in GetMenuEntries(node, options))
        {
            string text = GetMenuRowText(node, options, state, index, label);
            if (state.DisabledTexts?.Contains(text, StringComparer.Ordinal) == true)
                disabled.Add(index);
            rows[index++] = string.Equals(text, source, StringComparison.Ordinal) ? destination : text;
        }
        var texts = options.ContextMenuItemTexts?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        texts[node] = rows;
        states[node] = state with { DisabledRows = disabled, DisabledTexts = null, TextReplacements = null };
        return options with { ContextMenuItemTexts = texts, MenuStates = states };
    }

    private static IEnumerable<(string Label, string Icon)> GetMenuEntries(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        foreach (string entry in ReadNullTerminatedText(GetContextMenuCaption(node, options)).Split('|'))
        {
            if (entry.Length == 0) yield break;
            int split = entry.IndexOf('@');
            yield return split < 0 ? (entry, string.Empty) : (entry[..split], entry[split..]);
        }
    }

    private static bool IsMenuSeparator(string label) => label.Contains("separator", StringComparison.Ordinal) ||
        label.Contains("seperator", StringComparison.Ordinal);

    private static string GetMenuRowText(PangyaUiNode node, PangyaUiRenderOptions options,
        PangyaUiMenuState? state, int index, string label)
    {
        string text = options.ContextMenuItemTexts?.GetValueOrDefault(node)?.TryGetValue(index, out string? replacement) == true
                ? ReadNullTerminatedText(replacement) : TruncateShiftJis(label, 31);
        if (state?.TextReplacements is { } replacements)
            foreach (var pair in replacements)
                if (string.Equals(text, pair.Key, StringComparison.Ordinal)) return ReadNullTerminatedText(pair.Value);
        return text;
    }
    public int HitTestMenuRow(PangyaUiNode node, Point point, PangyaUiRenderOptions options)
    {
        if (IsRuntimeHidden(node, options) || !IsRenderVisible(node, options) || GetContextMenuCaption(node, options).Length == 0) return -1;
        using var bitmap = new Bitmap(1, 1);
        using Graphics metrics = Graphics.FromImage(bitmap);
        RenderContextMenu(metrics, node, GetAbsoluteBounds(node, options).Location,
            options with { HoveredNode = node, Pointer = point, IncludeCaptionVisualBounds = false }, draw: false, out int row);
        return row;
    }

    private Rectangle RenderContextMenu(Graphics graphics, PangyaUiNode node, Point origin,
        PangyaUiRenderOptions options, bool draw, out int hoveredRow)
    {
        Font font = SystemFonts.DefaultFont;
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        var rows = new List<(string Text, Image? Icon, bool Separator, float Top, float Height)>();
        bool hasIcons = false;
        float width = 0;
        float y = 8;
        PangyaUiMenuState? menuState = options.MenuStates?.GetValueOrDefault(node);
        foreach (var (label, iconToken) in GetMenuEntries(node, options))
        {
            hasIcons |= iconToken.Length > 0;
            Image? icon = iconToken.Length > 1 ? GetBitmap(iconToken[1..], Symbols(options)) : null;
            bool separator = IsMenuSeparator(label);
            if (!separator) y += 4;
            float height = separator ? 10 : (options.CaptionFontHeight ?? font.GetHeight(graphics)) + 3;
            string displayText = GetMenuRowText(node, options, menuState, rows.Count, label);
            rows.Add((displayText, icon, separator, y, height));
            y += height;
            float labelWidth = options.MeasureCaptionText?.Invoke(label) ??
                graphics.MeasureString(label, font, PointF.Empty, format).Width;
            width = Math.Max(width, labelWidth + (icon?.Width ?? 0));
        }

        var bounds = new Rectangle(origin, new Size((int)Math.Ceiling(width + 36), (int)Math.Ceiling(y + 10)));
        bool IsDisabled(int index) => menuState?.DisabledRows?.Contains(index) == true ||
            index >= 0 && index < rows.Count && menuState?.DisabledTexts is { } disabledTexts &&
            disabledTexts.Any(text => string.Equals(text, rows[index].Text, StringComparison.Ordinal));
        hoveredRow = -1;
        if (ReferenceEquals(options.HoveredNode, node) &&
            options.Pointer is Point pointer && bounds.Contains(pointer))
            hoveredRow = rows.FindIndex(row => !row.Separator &&
                pointer.Y >= origin.Y + row.Top && pointer.Y < origin.Y + row.Top + row.Height);
        if (IsDisabled(hoveredRow)) hoveredRow = -1;
        int activeRow = hoveredRow;
        IReadOnlySet<string> symbols = Symbols(options);
        PangyaUiNode? frame = _resources.TryResolve(GetRenderResource(node, options.ButtonState), "FRAME", symbols);
        Rectangle renderedBounds = bounds;
        if (frame?.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) == true)
        {
            renderedBounds = DrawFrame(draw ? graphics : null, bounds, frame, symbols, showBlink: false,
                node.Parameters.GetValueOrDefault("bgimg"), isViewFocused: IsWindowViewFocused(node, options),
                contextMenu: true, textOptions: options, frameInstance: node);
        }
        Rectangle? measured = renderedBounds;
        var bitmapText = options.DrawCaptionText?.Target as IPangyaUiBitmapTextPasses;
        for (int index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            if (row.Separator) continue;
            if (row.Icon is Image icon)
            {
                PointF iconOrigin = GetIconOrigin(origin.Y + row.Top, row.Height, icon);
                IncludeBounds(ref measured, Rectangle.FromLTRB(
                    ClampCoordinate((long)MathF.Floor(iconOrigin.X)), ClampCoordinate((long)MathF.Floor(iconOrigin.Y)),
                    ClampCoordinate((long)MathF.Ceiling(iconOrigin.X + icon.Width)),
                    ClampCoordinate((long)MathF.Ceiling(iconOrigin.Y + icon.Height))));
            }
            if (!draw && options.IncludeCaptionVisualBounds && row.Text.Length > 0)
            {
                PointF labelOrigin = GetLabelOrigin(row.Top);
                int style = GetRowStyle(index);
                float labelWidth = bitmapText?.GetDrawAdvance(row.Text) ?? options.MeasureCaptionText?.Invoke(row.Text) ??
                    graphics.MeasureString(row.Text, font, PointF.Empty, format).Width;
                IncludeBounds(ref measured, MeasureGlyphExtent(labelOrigin, labelWidth,
                    options.CaptionFontHeight ?? font.GetHeight(graphics), style));
                IncludeBounds(ref measured, MeasureCaptionArtwork(bitmapText, row.Text, labelOrigin, style));
            }
        }
        renderedBounds = measured ?? bounds;
        if (!draw) return renderedBounds;
        byte opacity = GetWindowOpacity(node, options, menuState?.Opacity ?? byte.MaxValue, secondary: false);
        Color normalColor = ParseColor(node.Parameters.GetValueOrDefault("fontcolor"), Color.Black, implicitAlpha: false);
        using var brush = new SolidBrush(Color.FromArgb((int)(normalColor.A *
            GetWindowAlphaScale(node, options, secondary: false) *
            (menuState?.Opacity ?? byte.MaxValue) / byte.MaxValue), normalColor));
        using var highlight = new SolidBrush(Color.FromArgb(opacity, 255, 204, 0));
        using var selection = new SolidBrush(Color.FromArgb(menuState?.SelectionAlpha ?? byte.MaxValue,
            255, 204, 0));
        using var disabled = new SolidBrush(Color.FromArgb(opacity, 204, 204, 204));
        using var pen = new Pen(Color.FromArgb(243, 243, 243));
        for (int index = 0; index < rows.Count; index++)
        {
            var row = rows[index];
            float top = origin.Y + row.Top;
            if (row.Separator)
                graphics.DrawLine(pen, origin.X + 3f, top + row.Height / 2,
                    origin.X + bounds.Width - 3f, top + row.Height / 2);
            else
            {
                bool enabled = !IsDisabled(index);
                if (row.Icon is Image icon)
                {
                    PointF iconOrigin = GetIconOrigin(top, row.Height, icon);
                    GraphicsState iconState = graphics.Save();
                    try
                    {
                        graphics.PixelOffsetMode = PixelOffsetMode.Half;
                        graphics.TranslateTransform(iconOrigin.X, iconOrigin.Y);
                        DrawTintedImage(graphics, icon, new Rectangle(Point.Empty, icon.Size),
                            0, icon.Width, icon.Height, enabled ? Color.White : Color.FromArgb(51, Color.White));
                    }
                    finally { graphics.Restore(iconState); }
                }
                Brush rowBrush = !enabled ? disabled : hoveredRow == index ? highlight :
                    menuState?.SelectedRow == index ? selection : brush;
                PointF labelOrigin = GetLabelOrigin(row.Top);
                int style = GetRowStyle(index);
                DrawStyledText(graphics, row.Text, rowBrush, brush, labelOrigin, format, style,
                    font, options.DrawCaptionText);
            }
        }
        return renderedBounds;

        PointF GetIconOrigin(float rowTop, float rowHeight, Image icon) =>
            new(origin.X + 7f - (20f - icon.Width) / 2f, rowTop - (rowHeight - icon.Height) / 2f - 3f);

        PointF GetLabelOrigin(float rowTop) => new(origin.X + (hasIcons ? 30f : 10f), origin.Y + rowTop);

        int GetRowStyle(int index) => !IsDisabled(index) && (activeRow == index || menuState?.SelectedRow == index) ? 1 : 0;

    }

    private static PangyaUiListPreview? GetList(PangyaUiNode node, PangyaUiRenderOptions options) =>
        node.Type.Equals("LISTBOX", StringComparison.OrdinalIgnoreCase)
            ? options.Lists?.GetValueOrDefault(node) : null;

    private static int GetListColumns(PangyaUiNode node, PangyaUiListPreview list) =>
        Math.Max(1, list.ColumnCapacity ?? (int.TryParse(node.Parameters.GetValueOrDefault("column"),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out int columns) ? columns : 1));

    private static bool IsListScrollbarVisible(PangyaUiNode node, PangyaUiListPreview list) =>
        list.ShowScrollbar ?? (!int.TryParse(node.Parameters.GetValueOrDefault("show_scrollbar"),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out int show) || show != 0);

    private static Size? GetListItemSize(PangyaUiNode node, PangyaUiListPreview list) =>
        list.ItemSize ?? PangyaUiDimensionHelper.ParseSize(node.Parameters.GetValueOrDefault("itemsize"));

    private static float GetListScrollRow(PangyaUiListPreview list, int maximum) =>
        list.ScrollThumbRow is float row && float.IsFinite(row)
            ? Math.Max(0, row)
            : list.FollowBottom ? maximum : Math.Clamp(list.TopRow, 0, maximum);

    private static int GetListTopRow(PangyaUiListPreview list, int maximum) =>
        (int)Math.Clamp((double)(GetListScrollRow(list, maximum) + 0.5f), 0, int.MaxValue);

    private static PangyaUiListPreview ResizeListScroll(PangyaUiNode node, PangyaUiListPreview previous,
        PangyaUiListPreview next, Size nextItemSize, PangyaUiRenderOptions before, PangyaUiRenderOptions after)
    {
        int previousCapacity = GetListItemSize(node, previous) is { Height: > 0 } oldSize
            ? Math.Max(0, GetWindowBounds(node, before).Height / oldSize.Height) : 0;
        int previousRows = previous.ItemCount > 0 ? (previous.ItemCount - 1) / GetListColumns(node, previous) + 1 : 0;
        float row = GetListScrollRow(previous, Math.Max(0, previousRows - previousCapacity));
        int rows = next.ItemCount > 0 ? (next.ItemCount - 1) / GetListColumns(node, next) + 1 : 0;
        int capacity = nextItemSize.Height > 0 ? Math.Max(0, GetWindowBounds(node, after).Height / nextItemSize.Height) : 0;
        var scroll = PangyaUiScrollState.RestoreRows(previousCapacity, rows, row, next.FollowBottom);
        scroll.Resize(capacity);
        return next with { TopRow = scroll.RoundedTopRow, ScrollThumbRow = scroll.TopRow,
            TopItem = null, ScrollbarAtLeft = false };
    }

    private static PangyaUiRenderOptions WithListPreview(PangyaUiNode node, PangyaUiListPreview list,
        PangyaUiRenderOptions options)
    {
        var lists = options.Lists!.ToDictionary(pair => pair.Key, pair => pair.Value);
        lists[node] = list;
        return options with { Lists = lists };
    }

    public PangyaUiRenderOptions ResizeListItems(PangyaUiNode node, Size itemSize, PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (itemSize.Width <= 0 || itemSize.Height <= 0) throw new ArgumentOutOfRangeException(nameof(itemSize));
        if (GetList(node, options) is not { } list)
            throw new ArgumentException("The node must have a list preview.", nameof(node));
        int scrollbarWidth = IsListScrollbarVisible(node, list)
            ? GetBitmap("scroll_n_1", Symbols(options))?.Width ?? 0 : 0;
        int columns = (int)Math.Max(1, ((long)GetWindowBounds(node, options).Width - scrollbarWidth) / itemSize.Width);
        var resized = list with { ItemSize = itemSize, ColumnCapacity = columns };
        return WithListPreview(node, ResizeListScroll(node, list, resized, itemSize, options, options), options);
    }

    public int HitTestListItem(PangyaUiNode node, Point point, PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (IsRuntimeHidden(node, options) || !IsRenderVisible(node, options) ||
            options.DisabledNodes?.Contains(node) == true ||
            GetList(node, options) is not { MouseEventsEnabled: true, ItemCount: > 0 } list)
            return -1;
        Rectangle bounds = GetAbsoluteBounds(node, options);
        if (!bounds.Contains(point) ||
            (list.ItemSize ?? PangyaUiDimensionHelper.ParseSize(node.Parameters.GetValueOrDefault("itemsize")))
                is not Size item || item.Width <= 0 || item.Height <= 0)
            return -1;
        int columns = GetListColumns(node, list);
        int scrollbarWidth = IsListScrollbarVisible(node, list)
            ? GetBitmap("scroll_n_1", Symbols(options))?.Width ?? 0 : 0;
        int capacity = bounds.Height / item.Height;
        int totalRows = (list.ItemCount - 1) / columns + 1;
        if (capacity > 0 && totalRows > capacity && (list.ScrollbarAtLeft
                ? point.X < (long)bounds.X + scrollbarWidth
                : point.X >= (long)bounds.X + bounds.Width - scrollbarWidth)) return -1;
        int maximumTopRow = Math.Max(0, totalRows - capacity);
        int topRow = GetListTopRow(list, maximumTopRow);
        long first = list.TopItem is int topItem ? Math.Clamp(topItem, 0, list.ItemCount - 1) : (long)topRow * columns;
        int column = (point.X - bounds.X) / item.Width;
        int row = (point.Y - bounds.Y) / item.Height;
        if (column >= columns || row >= Math.Max(1, capacity)) return -1;
        long index = first + (long)row * columns + column;
        return index >= list.ItemCount || list.ExcludeDummyItemsFromHitTesting && list.DummyItems?.Contains((int)index) == true
            ? -1 : (int)index;
    }

    private Rectangle DrawList(Graphics? graphics, PangyaUiNode node, Rectangle bounds,
        PangyaUiListPreview list, PangyaUiRenderOptions options)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return bounds;
        if (graphics is not null && list.DrawBackground is { } drawBackground)
        {
            GraphicsState backgroundState = graphics.Save();
            try { drawBackground(graphics, bounds); }
            finally { graphics.Restore(backgroundState); }
        }

        if (GetListItemSize(node, list) is not Size item ||
            item.Width <= 0 || item.Height <= 0)
            return bounds;

        int columns = GetListColumns(node, list);
        IReadOnlySet<string> symbols = Symbols(options);
        bool showScrollbar = IsListScrollbarVisible(node, list);
        int scrollbarWidth = showScrollbar
            ? GetBitmap("scroll_n_1", symbols)?.Width ?? 0 : 0;
        int capacity = bounds.Height / item.Height;
        bool showGuide = options.ScrollbarGuides?.GetValueOrDefault(node) == true;
        int totalRows = list.ItemCount > 0 ? (list.ItemCount - 1) / columns + 1 : 0;
        int maximumTopRow = Math.Max(0, totalRows - capacity);
        if (graphics is null || list.ItemCount <= 0)
        {
            Rectangle? measured = bounds;
            IncludeBounds(ref measured, DrawListScrollbar(measure: graphics is null));
            return measured ?? bounds;
        }
        int topRow = GetListTopRow(list, maximumTopRow);
        long firstItem = list.TopItem is int topItem
            ? Math.Clamp(topItem, 0, list.ItemCount - 1) : (long)topRow * columns;
        int rows = Math.Max(1, capacity);
        for (int row = 0; row < rows; row++)
        {
            long first = firstItem + (long)row * columns;
            if (first >= list.ItemCount) break;
            for (int column = 0; column < columns && first + column < list.ItemCount; column++)
            {
                var cell = new Rectangle(ClampCoordinate((long)bounds.X + (long)column * item.Width),
                    ClampCoordinate((long)bounds.Y + (long)row * item.Height), item.Width, item.Height);
                GraphicsState itemState = graphics.Save();
                try
                {
                    int index = (int)(first + column);
                    if (list.DrawItemState is { } drawItemState)
                    {
                        bool hovered = list.UnderCursorItems is { } cursorItems ? cursorItems.Contains(index) :
                            list.ItemUnderCursor is int cursorIndex ? cursorIndex == index :
                            list.MouseEventsEnabled && ReferenceEquals(options.HoveredNode, node) &&
                            options.Pointer is Point pointer && bounds.Contains(pointer) && cell.Contains(pointer) &&
                            (capacity <= 0 || totalRows <= capacity ||
                             (list.ScrollbarAtLeft
                                 ? pointer.X >= (long)bounds.X + scrollbarWidth
                                 : pointer.X < (long)bounds.X + bounds.Width - scrollbarWidth));
                        int visibleIndex = (int)(index - firstItem);
                        drawItemState(graphics, new PangyaUiListItemState(index, visibleIndex, cell,
                            hovered, list.SelectedItems?.Contains(index) == true)
                        {
                            IsDummy = list.DummyItems?.Contains(index) == true
                        });
                    }
                    else
                        list.DrawItem(graphics, index, cell);
                }
                finally { graphics.Restore(itemState); }
            }
        }

        DrawListScrollbar(measure: false);
        return bounds;

        Rectangle DrawListScrollbar(bool measure) => showScrollbar
            ? DrawScrollBar(graphics, bounds, capacity, totalRows,
                totalRows > 0 ? GetListScrollRow(list, maximumTopRow) : 0, symbols, showGuide,
                focused: capacity > 0 && totalRows > capacity &&
                    IsWindowViewFocused(node, options) && ReferenceEquals(options.HoveredNode, node),
                atLeft: list.ScrollbarAtLeft, opacity: GetScrollbarOpacity(node, options), retainTopRow: true, measure: measure)
            : Rectangle.Empty;
    }

    private static IReadOnlyList<PangyaUiTab>? GetTabs(PangyaUiNode node, PangyaUiRenderOptions options) =>
        node.Type.Equals("TABBUTTON", StringComparison.OrdinalIgnoreCase)
            ? options.Tabs?.GetValueOrDefault(node) ?? GetMakeRoomTabs(node, options) ??
                (options.TabOwnerDraw?.ContainsKey(node) == true ? Array.Empty<PangyaUiTab>() : null)
            : null;

    private static readonly PangyaUiTab[] MakeRoomUserTabs = CreateMakeRoomTabs([2, 3, 4], 50);
    private static readonly PangyaUiTab[] MakeRoomHoleTabs = CreateMakeRoomTabs([3, 6, 9, 18], 37.5);
    private static readonly PangyaUiTab[] MakeRoomTimeTabs = CreateMakeRoomTabs([40, 60, 120, 300], 37.5);

    private static PangyaUiTab[] CreateMakeRoomTabs(int[] values, double width, int selectedIndex = 0) =>
        values.Select((value, index) => new PangyaUiTab("select_button_n", "select_button_o", "select_button_d",
            index == selectedIndex ? PangyaUiButtonState.Selected : PangyaUiButtonState.Normal,
            new Size((int)Math.Ceiling(width), 20))
            { Caption = value.ToString(CultureInfo.InvariantCulture), LayoutWidth = (float)width }).ToArray();

    private static readonly PangyaUiTab[] MakeRoomGameTabs =
    [
        new("select_button_n", "select_button_o", "select_button_d", PangyaUiButtonState.Selected,
            new Size(75, 20)) { Caption = "스트로크" },
        new("select_button_n", "select_button_o", "select_button_d", Size: new Size(75, 20)) { Caption = "매치" }
    ];

    private static readonly PangyaUiTab[] MatchUserTabs = CreateMakeRoomTabs([2, 4], 75, selectedIndex: 1);
    private static readonly PangyaUiTab[] MatchHoleTabs = CreateMakeRoomTabs([6, 9, 18], 50);

    private static bool IsDefaultMakeRoomModeButton(PangyaUiNode node, PangyaUiRenderOptions options) =>
        node.Name == "make_vs" && node.Parent is { } room &&
        string.Equals(room.Name, "makeroom", StringComparison.OrdinalIgnoreCase) &&
        !room.Children.Any(child => child.Name.StartsWith("make_", StringComparison.Ordinal) &&
            options.LatchedButtons?.Contains(child) == true);

    private static IReadOnlyList<PangyaUiTab>? GetMakeRoomTabs(PangyaUiNode node, PangyaUiRenderOptions options) =>
        node.Parent is { } room && string.Equals(room.Name, "makeroom", StringComparison.OrdinalIgnoreCase) ? node.Name switch
        {
            "gametype" => MakeRoomGameTabs,
            "userlimit" => IsMakeRoomMatch(room, options) ? MatchUserTabs : MakeRoomUserTabs,
            "hole" => IsMakeRoomMatch(room, options) ? MatchHoleTabs : MakeRoomHoleTabs,
            "timelimit" => MakeRoomTimeTabs,
            _ => null
        } : null;

    private static bool IsMakeRoomMatch(PangyaUiNode room, PangyaUiRenderOptions? options)
    {
        if (options?.SelectedTabsByInstance is { Count: > 0 } selections &&
            room.Children.FirstOrDefault(child => child.Name == "gametype") is { } mode &&
            selections.TryGetValue(new PangyaUiTabInstance(mode, options.ResourceInstance), out int selected))
            return selected == 1;
        return options?.MakeRoomMatchForms?.Contains(room) == true;
    }

    public int HitTestTab(PangyaUiNode node, Point point, PangyaUiRenderOptions options) =>
        HitTestTab(node, point, GetAbsoluteBounds(node, options).Location, options);

    public int HitTestTab(PangyaUiTabInstance instance, Point point, Point renderedOrigin, PangyaUiRenderOptions options) =>
        HitTestTab(instance.Node, point, renderedOrigin, options with { ResourceInstance = instance.Resource });

    public int HitTestTab(PangyaUiNode node, Point point, Point renderedOrigin, PangyaUiRenderOptions options)
    {
        if (IsRuntimeHidden(node, options) || !IsRenderVisible(node, options) || !node.IsVisible(Symbols(options)) ||
            options.DisabledNodes?.Contains(node) == true ||
            options.TabMouseEventsEnabled?.GetValueOrDefault(node, true) == false || GetTabs(node, options) is not { } tabs)
            return -1;
        int hit = -1;
        RenderTabs(null, node, renderedOrigin, tabs, Symbols(options), options,
            (index, bounds) => { if (bounds.Contains(point)) hit = index; });
        return hit;
    }

    private Rectangle RenderTabs(Graphics? graphics, PangyaUiNode node, Point origin,
        IReadOnlyList<PangyaUiTab> tabs, IReadOnlySet<string> symbols, PangyaUiRenderOptions options,
        Action<int, Rectangle>? visitTab = null)
    {
        Image? GetImage(string? filename) =>
            string.IsNullOrWhiteSpace(filename) ? null : GetBitmap(filename, symbols);
        Image? separator = GetImage(node.Parameters.GetValueOrDefault("sepImg"));
        float separatorWidth = ReferenceEquals(tabs, GetMakeRoomTabs(node, options)) ? 1 : separator?.Width ?? 0;
        if (options.TabSeparatorWidths?.TryGetValue(node, out float suppliedWidth) == true &&
            float.IsFinite(suppliedWidth))
            separatorWidth = Math.Clamp(suppliedWidth, 0, int.MaxValue);
        Rectangle? result = null;
        var drawOwner = options.TabOwnerDraw?.GetValueOrDefault(node);
        if (drawOwner is not null)
        {
            double width = 0;
            int height = 0;
            for (int index = 0; index < tabs.Count; index++)
            {
                PangyaUiTab tab = tabs[index];
                Size size = tab.Size ?? GetImage(tab.Normal)?.Size ?? Size.Empty;
                if (index == 0) height = Math.Max(0, size.Height);
                if (index > 0) width += separatorWidth;
                width += tab.LayoutWidth is > 0 and <= int.MaxValue ? tab.LayoutWidth.Value : Math.Max(0, size.Width);
            }
            Size controlSize = options.WindowSizes?.TryGetValue(node, out Size suppliedSize) == true
                ? new Size(Math.Max(0, suppliedSize.Width), Math.Max(0, suppliedSize.Height))
                : new Size((int)Math.Clamp(Math.Floor(width + 0.5), 0, int.MaxValue), height);
            var barBounds = new Rectangle(origin, controlSize);
            IncludeBounds(ref result, barBounds);
            DrawOwner(null, barBounds);
        }
        byte alpha = GetWindowOpacity(node, options,
            options.TabOpacity?.GetValueOrDefault(node, byte.MaxValue) ?? byte.MaxValue);
        var instance = new PangyaUiTabInstance(node, options.ResourceInstance);
        int? selectedIndex = options.SelectedTabsByInstance?.TryGetValue(instance, out int stableSelection) == true
            ? stableSelection : options.SelectedTabInstances?.TryGetValue((node, origin), out int instanceSelection) == true
            ? instanceSelection : options.SelectedTabIndices?.TryGetValue(node, out int selection) == true ? selection : null;
        if (selectedIndex is null &&
            options.SelectedTabIdentifiers?.TryGetValue(node, out uint identifier) == true)
        {
            for (int index = 0; index < tabs.Count; index++)
                if ((tabs[index].Identifier ?? (uint)index) == identifier)
                {
                    selectedIndex = index;
                    break;
                }
        }
        int? unselectedIndex = null;
        if (options.UnselectedTabIdentifiers?.TryGetValue(node, out uint unselectedIdentifier) == true)
        {
            for (int index = 0; index < tabs.Count; index++)
                if ((tabs[index].Identifier ?? (uint)index) == unselectedIdentifier)
                {
                    unselectedIndex = index;
                    break;
                }
        }
        double x = origin.X;
        Size? inputSize = options.WindowSizes?.TryGetValue(node, out Size suppliedInputSize) == true ? suppliedInputSize : null;
        int inputHeight = inputSize?.Height ?? 0;
        for (int index = 0; index < tabs.Count; index++)
        {
            PangyaUiTab tab = tabs[index];
            Image? normal = GetImage(tab.Normal);
            Size size = tab.Size ?? normal?.Size ?? Size.Empty;
            if (index == 0 && inputSize is null) inputHeight = size.Height;
            if (index > 0 && separator is not null)
            {
                long separatorLeft = (long)Math.Floor(x + 0.5);
                int separatorPixels = ClampCoordinate((long)Math.Floor(x + separatorWidth + 0.5) - separatorLeft);
                var separatorBounds = new Rectangle(ClampCoordinate(separatorLeft), origin.Y,
                    separatorPixels, separator.Height);
                DrawSlice(separator, separatorBounds);
                if (alpha > 0) IncludeBounds(ref result, separatorBounds);

            }
            if (index > 0) x += separatorWidth;
            float layoutWidth = tab.LayoutWidth is > 0 and <= int.MaxValue ? tab.LayoutWidth.Value : Math.Max(0, size.Width);
            long left = (long)Math.Floor(x + 0.5);
            int pixelWidth = ClampCoordinate((long)Math.Floor(x + layoutWidth + 0.5) - left);
            var bounds = new Rectangle(ClampCoordinate(left), origin.Y, pixelWidth, Math.Max(0, size.Height));
            // The tab can draw beyond the bar, but mouse dispatch uses the bar's client area.
            Rectangle hitBounds = bounds with { Height = Math.Clamp(inputHeight, 0, bounds.Height) };
            if (inputSize is Size explicitSize)
                hitBounds = Rectangle.Intersect(hitBounds, new Rectangle(origin, explicitSize));
            PangyaUiButtonState state = selectedIndex is int selected
                ? selected == index ? PangyaUiButtonState.Selected : PangyaUiButtonState.Normal
                : tab.State;
            if (unselectedIndex == index && state == PangyaUiButtonState.Selected)
                state = PangyaUiButtonState.Normal;
            bool hovered = options.HoveredTabIndices?.TryGetValue(node, out int? retainedHover) == true
                ? retainedHover == index : options.HoveredTabInstance is { } instanceHit
                ? instanceHit == (instance, index) : options.HoveredTab is { } tabHit
                ? tabHit == (node, index, bounds)
                : options.TabMouseEventsEnabled?.GetValueOrDefault(node, true) != false &&
                  options.DisabledNodes?.Contains(node) != true &&
                  ReferenceEquals(options.HoveredNode, node) &&
                  options.Pointer is Point pointer && hitBounds.Contains(pointer);
            if (state == PangyaUiButtonState.Normal && hovered)
                state = PangyaUiButtonState.Hover;
            Image? variant = GetImage(state switch
            {
                PangyaUiButtonState.Hover => tab.Hover,
                PangyaUiButtonState.Selected => tab.Selected,
                _ => null
            });
            string below = variant is null ? "below_normal" :
                state == PangyaUiButtonState.Selected ? "below_selected" : "below_over";
            if (GetImage(node.Parameters.GetValueOrDefault(below)) is Image background)
                DrawSlice(background, bounds);
            if ((variant ?? normal) is Image foreground)
                DrawSlice(foreground, bounds);
            IncludeBounds(ref result, bounds);
            if (bounds.Width > 0 && bounds.Height > 0 && alpha > 0 && tab.Caption is { Length: > 0 } caption &&
                (graphics is not null || options.IncludeCaptionVisualBounds))
            {
                using var font = new Font(SystemFonts.DefaultFont.FontFamily, 11, GraphicsUnit.Pixel);
                var captionOrigin = new PointF(bounds.X + bounds.Width / 2f, bounds.Y + 5);
                if (graphics is not null)
                {
                    using var brush = new SolidBrush(Color.FromArgb(alpha, Color.White));
                    using var format = new StringFormat { Alignment = StringAlignment.Center };
                    DrawTaggedCaption(graphics, caption, brush, brush, captionOrigin, format, 0, font,
                        lineHeight: options.TooltipFontHeight, measureText: options.MeasureTooltipText,
                        drawText: options.DrawTooltipText, parseTags: false);
                }
                else
                {
                    using var bitmap = new Bitmap(1, 1);
                    using Graphics metrics = Graphics.FromImage(bitmap);
                    using var format = new StringFormat { FormatFlags = StringFormatFlags.MeasureTrailingSpaces };
                    var bitmapText = options.DrawTooltipText?.Target as IPangyaUiBitmapTextPasses;
                    float height = options.TooltipFontHeight ?? font.GetHeight(metrics);
                    foreach (string line in caption.Replace("\r\n", "\n").Split('\n'))
                    {
                        float width = MeasureCaptionLine(metrics, line, font, format, null, null, options.MeasureTooltipText);
                        var start = new PointF(captionOrigin.X - width / 2, captionOrigin.Y);
                        IncludeBounds(ref result, MeasureGlyphExtent(start, bitmapText?.GetDrawAdvance(line) ?? width, height, 0));
                        IncludeBounds(ref result, MeasureCaptionArtwork(bitmapText, line, start, style: 0));
                        captionOrigin.Y += height;
                    }
                }
            }
            if (hitBounds.Width > 0 && hitBounds.Height > 0) visitTab?.Invoke(index, hitBounds);
            if (graphics is not null)
            {
                options.OnTabRendered?.Invoke(node, index, bounds);
                options.OnTabInstanceRendered?.Invoke(node, index, bounds, origin);
                options.OnTabRenderedWithInstance?.Invoke(instance, index, bounds, origin);
            }
            DrawOwner(index, bounds);
            x += layoutWidth;
        }
        return result ?? Rectangle.Empty;

        void DrawSlice(Image image, Rectangle bounds)
        {
            if (graphics is null || alpha == 0 || bounds.Width <= 0 || bounds.Height <= 0) return;
            if (alpha == byte.MaxValue)
                DrawGraphicsImage(graphics, image, bounds);
            else
                DrawTintedImage(graphics, image, bounds, 0, image.Width, image.Height, Color.FromArgb(alpha, Color.White));
        }

        void DrawOwner(int? index, Rectangle bounds)
        {
            if (graphics is null || drawOwner is null) return;
            GraphicsState state = graphics.Save();
            try { drawOwner(graphics, index, bounds); }
            finally { graphics.Restore(state); }
        }
    }

    private static bool IsGauge(PangyaUiNode node) =>
        node.Type.Equals("GAUGEBAR", StringComparison.OrdinalIgnoreCase) || IsExtendedGauge(node);

    private static bool IsExtendedGauge(PangyaUiNode node) =>
        node.Type.Equals("GAUGEBAREX", StringComparison.OrdinalIgnoreCase);

    public static int GetExtendedGaugeStepCount(PangyaUiNode node) =>
        GetExtendedGaugeStepCount(node, null);

    public static int GetExtendedGaugeStepCount(PangyaUiNode node, PangyaUiRenderOptions? options)
    {
        if (options?.GaugeStepCounts?.TryGetValue(node, out int supplied) == true)
            return Math.Max(1, supplied);
        return int.TryParse(node.Parameters.GetValueOrDefault("steps"), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out int steps) ? Math.Max(1, steps) : 1;
    }

    public int HitTestExtendedGaugeButton(PangyaUiNode node, Point point, IReadOnlySet<string> symbols,
        PangyaUiRenderOptions? options = null)
    {
        if (IsRuntimeHidden(node, options) || !IsExtendedGauge(node) || !IsRenderVisible(node, options) ||
            options?.DisabledNodes?.Contains(node) == true) return 0;
        Rectangle bounds = GetAbsoluteBounds(node, options);
        foreach (bool left in new[] { true, false })
            if (node.Parameters.GetValueOrDefault(left ? "left" : "right") == "true" &&
                GetGaugeImage(node, left ? "normal" : "rnormal", symbols) is Image button &&
                GetGaugeButtonBounds(node, bounds, button.Size, left, options).Contains(point))
                return left ? -1 : 1;
        return 0;
    }

    private static Rectangle GetGaugeButtonBounds(Rectangle bounds, Size size, bool left)
    {
        int x = left ? ClampCoordinate((long)bounds.X + 2) :
            ClampCoordinate((long)bounds.X + bounds.Width - size.Width - 2);
        int y = ClampCoordinate((long)bounds.Y + ((long)bounds.Height - size.Height + 1) / 2);
        return new Rectangle(new Point(x, y), size);
    }

    private static Rectangle GetGaugeButtonBounds(PangyaUiNode node, Rectangle bounds, Size size, bool left,
        PangyaUiRenderOptions? options) =>
        options?.GaugeButtons?.TryGetValue((node, left), out Rectangle cached) == true
            ? new Rectangle(AddOrigins(bounds.Location, cached.Location), cached.Size)
            : GetGaugeButtonBounds(bounds, size, left);

    private Rectangle MeasureGaugeBounds(PangyaUiNode node, Rectangle bounds, IReadOnlySet<string> symbols,
        PangyaUiRenderOptions options)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return bounds;
        Rectangle? measured = bounds;
        bool extended = IsExtendedGauge(node);
        if (extended)
            foreach (bool left in new[] { true, false })
                if (node.Parameters.GetValueOrDefault(left ? "left" : "right") == "true" &&
                    GetGaugeImage(node, left ? "normal" : "rnormal", symbols) is Image button)
                    IncludeBounds(ref measured, GetGaugeButtonBounds(node, bounds, button.Size, left, options));
        Rectangle interior = GetGaugeInterior(node, bounds, symbols, options);
        if (interior.Width <= 0 || interior.Height <= 0) return measured ?? bounds;
        if (options.GaugeBars?.TryGetValue(node, out RectangleF bar) == true && bar.Width > 0 && bar.Height > 0 &&
            float.IsFinite(bar.Left) && float.IsFinite(bar.Top) && float.IsFinite(bar.Right) && float.IsFinite(bar.Bottom))
        {
            bar.Offset(bounds.X, bounds.Y);
            IncludeBounds(ref measured, EncloseBounds(bar));
        }
        if (extended && GetGaugeDividers(node, interior, options) is { } dividers)
        {
            IncludeBounds(ref measured, EncloseBounds(GetGaugeDivider(interior, 1, dividers.Pitch)));
            IncludeBounds(ref measured, EncloseBounds(GetGaugeDivider(interior, dividers.Count, dividers.Pitch)));
        }
        return measured ?? bounds;
    }

    private static (int Count, float Pitch)? GetGaugeDividers(PangyaUiNode node, Rectangle interior,
        PangyaUiRenderOptions options)
    {
        int steps = GetExtendedGaugeStepCount(node, options);
        return steps > 1 && steps - 1 <= interior.Width / 2
            ? (steps - 1, (interior.Width - (steps - 1) * 2f) / steps) : null;
    }

    private static RectangleF GetGaugeDivider(Rectangle interior, int step, float pitch) =>
        new(interior.X + step * pitch + (step - 1) * 2f, interior.Y, 2f, interior.Height);

    private Rectangle GetGaugeInterior(PangyaUiNode node, Rectangle bounds, IReadOnlySet<string> symbols,
        PangyaUiRenderOptions options)
    {
        Rectangle track = bounds;
        if (options.GaugeTracks?.TryGetValue(node, out Rectangle cachedTrack) == true)
            track = new Rectangle(AddOrigins(bounds.Location, cachedTrack.Location), cachedTrack.Size);
        else if (IsExtendedGauge(node))
            foreach (bool left in new[] { true, false })
            {
                if (node.Parameters.GetValueOrDefault(left ? "left" : "right") != "true") continue;
                int width = GetGaugeImage(node, left ? "normal" : "rnormal", symbols)?.Width ?? 0;
                int reserved = Math.Min(track.Width, width + 2);
                if (left) track.X = ClampCoordinate((long)track.X + reserved);
                track.Width -= reserved;
            }
        int border = GetGaugeBorder(node, track, options);
        return Rectangle.Inflate(track, -border, -border);
    }

    private static bool IsImageGauge(PangyaUiNode node) =>
        node.Type.Equals("GAUGEBARIMAGE", StringComparison.OrdinalIgnoreCase);

    /// <summary>Returns a borrowed provider-owned image; only index 1 selects the mask.</summary>
    public Image? GetImageGaugeSkin(PangyaUiNode node, int index, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsImageGauge(node)) throw new ArgumentException("The node must be an image gauge.", nameof(node));
        return GetGaugeImage(node, index == 1 ? "mask" : "normal", Symbols(options ?? new()));
    }

    private Image? GetGaugeImage(PangyaUiNode node, string key, IReadOnlySet<string> symbols) =>
        node.Parameters.TryGetValue(key, out string? name) ? GetBitmap(name, symbols) : null;

    private static int GetGaugeBorder(PangyaUiNode node, Rectangle track, PangyaUiRenderOptions options)
    {
        int border = int.TryParse(node.Parameters.GetValueOrDefault("border_thickness"),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out int thickness) ? thickness : IsExtendedGauge(node) ? 3 : 2;
        border = options.GaugeAppearances?.GetValueOrDefault(node)?.BorderThickness ?? border;
        return Math.Clamp(border, 0, Math.Max(0, Math.Min(track.Width, track.Height) / 2));
    }

    private PangyaUiRenderOptions CacheGaugeGeometry(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        Rectangle bounds = new(Point.Empty, GetWindowBounds(node, options).Size);
        Rectangle track = bounds;
        var buttons = options.GaugeButtons?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        if (IsExtendedGauge(node))
        {
            foreach (bool left in new[] { true, false })
            {
                if (node.Parameters.GetValueOrDefault(left ? "left" : "right") != "true") continue;
                Image? image = GetGaugeImage(node, left ? "normal" : "rnormal", Symbols(options));
                if (image is not null && !buttons.ContainsKey((node, left)))
                    buttons[(node, left)] = GetGaugeButtonBounds(bounds, image.Size, left);
                int reserved = Math.Min(Math.Max(0, track.Width), (image?.Width ?? 0) + 2);
                if (left) track.X += reserved;
                track.Width -= reserved;
            }
        }
        var tracks = options.GaugeTracks?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        if (tracks.TryGetValue(node, out Rectangle retained)) track = retained;
        else tracks[node] = track;
        var bars = options.GaugeBars?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        if (!bars.ContainsKey(node))
        {
            Rectangle interior = Rectangle.Inflate(track, -GetGaugeBorder(node, track, options), -GetGaugeBorder(node, track, options));
            PangyaUiGaugeRange? range = options.GaugeRanges?.GetValueOrDefault(node);
            double value = options.GaugePositions?.GetValueOrDefault(node) ?? options.GaugeValues?.GetValueOrDefault(node) ?? 0;
            if (IsExtendedGauge(node) && options.ExtendedGaugeSteps?.TryGetValue(node, out int step) == true)
                value = (range?.Lower ?? 0) + ((range?.Upper ?? 100) - (double)(range?.Lower ?? 0)) *
                    Math.Clamp(step / (double)GetExtendedGaugeStepCount(node, options), 0, 1);
            if (options.GaugeTransitions?.GetValueOrDefault(node) is { } transition)
                value = GetGaugeTransitionValue(transition, range, 100);
            float ratio = (float)GaugeRatio(value, range, 100);
            float origin = (float)GaugeRatio(range?.Origin ?? 0, range, 100);
            bars[node] = new(interior.X + interior.Width * Math.Min(origin, ratio), interior.Y,
                interior.Width * Math.Abs(ratio - origin), interior.Height);
        }
        return options with { GaugeTracks = tracks, GaugeBars = bars, GaugeButtons = buttons };
    }

    private Rectangle RenderImageGauge(Graphics? graphics, PangyaUiNode node, Point origin,
        IReadOnlySet<string> symbols, PangyaUiRenderOptions options)
    {
        if (GetGaugeImage(node, "normal", symbols) is not Image skin ||
            GetGaugeImage(node, "mask", symbols) is not Image mask)
            return Rectangle.Empty;

        var skinBounds = new Rectangle(origin, skin.Size);
        if (int.TryParse(node.Parameters.GetValueOrDefault("drawstyle"), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out int drawStyle) && drawStyle == 1)
        {
            if (options.ImageGaugeOwnerDraw?.GetValueOrDefault(node) is not { } drawOwner)
                return Rectangle.Empty;
            if (graphics is not null)
            {
                GraphicsState state = graphics.Save();
                try { drawOwner(graphics, skinBounds); }
                finally { graphics.Restore(state); }
            }
            return skinBounds;
        }

        PangyaUiGaugeRange? range = options.GaugeRanges?.GetValueOrDefault(node);
        double value = options.GaugeTransitions?.GetValueOrDefault(node) is { } transition
            ? GetGaugeTransitionValue(transition, range, skin.Width)
            : options.GaugePositions?.TryGetValue(node, out float position) == true && float.IsFinite(position)
                ? position : options.GaugeValues?.GetValueOrDefault(node) ?? 0;
        float start = (float)(GaugeRatio(value, range, skin.Width) * mask.Width);
        float width = mask.Width - start;
        if (graphics is not null) DrawGraphicsImage(graphics, skin, skinBounds);
        if (width <= 0) return skinBounds;

        if (graphics is not null)
        {
            PointF[] corners =
            [
                new(origin.X + start, origin.Y),
                new(origin.X + (float)mask.Width, origin.Y),
                new(origin.X + start, origin.Y + (float)mask.Height)
            ];
            DrawGraphicsImage(graphics, mask, corners, new RectangleF(start, 0, width, mask.Height));
        }
        int measuredStart = (int)start;
        var maskBounds = new Rectangle(ClampCoordinate((long)origin.X + measuredStart),
            origin.Y, mask.Width - measuredStart, mask.Height);
        Rectangle? measured = skinBounds;
        IncludeBounds(ref measured, maskBounds);
        return measured ?? skinBounds;
    }
    private void DrawGauge(Graphics graphics, PangyaUiNode node, Rectangle bounds, double value,
        IReadOnlySet<string> symbols, PangyaUiRenderOptions options, bool enabled, int expansion, PangyaUiGaugeRange? range,
        PangyaUiGaugeTransition? transition)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return;
        PangyaUiGaugeAppearance? appearance = options.GaugeAppearances?.GetValueOrDefault(node);
        if (transition is not null)
            value = GetGaugeTransitionValue(transition, range, 100);
        using var background = new SolidBrush(ParseColor(node.Parameters.GetValueOrDefault("bgcolor"),
            Color.FromArgb(64, 64, 64), implicitAlpha: false));
        graphics.FillRectangle(background, bounds);
        bool extended = IsExtendedGauge(node);
        var buttons = new List<(Rectangle Bounds, PangyaUiRenderOptions Options, bool Right)>(2);
        if (extended)
        {
            Color outline = ParseColor(node.Parameters.GetValueOrDefault("bordercolor"), Color.Gray, implicitAlpha: false);
            using var pen = new Pen(outline);
            graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
            foreach (bool left in new[] { true, false })
            {
                if (node.Parameters.GetValueOrDefault(left ? "left" : "right") != "true") continue;
                Image? button = GetGaugeImage(node, left ? "normal" : "rnormal", symbols);
                if (button is not null)
                {
                    Rectangle buttonBounds = GetGaugeButtonBounds(node, bounds, button.Size, left, options);
                    PangyaUiRenderOptions buttonOptions = options;
                    if (options.ButtonState == PangyaUiButtonState.Normal &&
                        ReferenceEquals(options.HoveredNode, node) &&
                        options.Pointer is Point pointer && buttonBounds.Contains(pointer))
                        buttonOptions = options with { ButtonState = PangyaUiButtonState.Hover };
                    buttons.Add((buttonBounds, buttonOptions, !left));
                }
            }
        }
        Rectangle interior = GetGaugeInterior(node, bounds, symbols, options);
        float ratio = (float)GaugeRatio(value, range, 100);
        float origin = (float)GaugeRatio(range?.Origin ?? 0, range, 100);
        float startOffset = interior.Width * Math.Min(origin, ratio);
        float widthFilled = interior.Width * Math.Abs(ratio - origin);
        if (interior.Width <= 0 || interior.Height <= 0)
        {
            DrawArrowButtons();
            return;
        }
        Color initialColor = ParseColor(node.Parameters.GetValueOrDefault("color"), Color.White, implicitAlpha: false);
        Color startColor = appearance?.FillColor ?? initialColor;
        var fill = new RectangleF(interior.X + startOffset, interior.Y, widthFilled, interior.Height);
        if (options.GaugeBars?.TryGetValue(node, out RectangleF cachedBar) == true)
        {
            fill = new RectangleF(bounds.X + cachedBar.X, bounds.Y + cachedBar.Y, cachedBar.Width, cachedBar.Height);
            startOffset = fill.X - interior.X;
            widthFilled = fill.Width;
        }
        if (!extended)
        {
            using var foreground = new SolidBrush(startColor);
            double span = (double)(range?.Upper ?? 100) - (range?.Lower ?? 0);
            float change = span > 0 ? (float)(Math.Abs((double)expansion) * bounds.Width / span) : 0;
            float extra = Math.Min(change, expansion < 0 ? widthFilled :
                Math.Max(0, interior.Width - startOffset - widthFilled));
            float mainWidth = expansion < 0 ? widthFilled - extra : widthFilled;
            if (mainWidth > 0)
                graphics.FillRectangle(foreground, fill.X, fill.Y, mainWidth, fill.Height);
            if (extra > 0)
            {
                string colorKey = expansion < 0 ? "excolor2" : "excolor1";
                Color changeColor = (expansion < 0 ? appearance?.ExpansionColor2 : appearance?.ExpansionColor1)
                    ?? ParseColor(node.Parameters.GetValueOrDefault(colorKey),
                        Color.FromArgb(112, 112, 112), implicitAlpha: false);
                using var changeBrush = new SolidBrush(changeColor);
                graphics.FillRectangle(changeBrush, fill.X + mainWidth, fill.Y, extra, fill.Height);
            }
            return;
        }

        Color target = ParseColor(node.Parameters.GetValueOrDefault("color2"), initialColor, implicitAlpha: false);
        double colorValue = transition is null ? value :
            ClampGaugeValue(transition.From, range?.Lower ?? 0, range?.Upper ?? 100);
        if (options.GaugeColorPositions?.TryGetValue(node, out float colorPosition) == true && float.IsFinite(colorPosition))
            colorValue = colorPosition;
        double colorRatio = double.IsNaN(colorValue) ? 0 : Math.Clamp(colorValue / 100, 0, 1);
        int Blend(int start, int end) => (int)(start * (1 - colorRatio) + end * colorRatio);
        Color endColor = Color.FromArgb(Blend(startColor.A, target.A), Blend(startColor.R, target.R),
            Blend(startColor.G, target.G), Blend(startColor.B, target.B));
        if (!enabled)
        {
            startColor = DisabledGaugeColor(startColor);
            endColor = DisabledGaugeColor(endColor);
        }
        if (widthFilled > 0)
        {
            using var gradient = new LinearGradientBrush(fill, startColor, endColor, LinearGradientMode.Horizontal);
            graphics.FillRectangle(gradient, fill);
        }

        if (GetGaugeDividers(node, interior, options) is { } dividers)
            for (int step = 1; step <= dividers.Count; step++)
                graphics.FillRectangle(background, GetGaugeDivider(interior, step, dividers.Pitch));

        DrawArrowButtons();

        void DrawArrowButtons()
        {
            foreach (var button in buttons)
                DrawButton(graphics, node, button.Bounds, button.Options, rightGaugeButton: button.Right);
        }
    }

    private static double GetGaugeTransitionValue(PangyaUiGaugeTransition transition, PangyaUiGaugeRange? range,
        int defaultUpper)
    {
        int lower = range?.Lower ?? 0;
        int upper = range?.Upper ?? defaultUpper;
        if (upper <= lower) return lower;
        double from = ClampGaugeValue(transition.From, lower, upper);
        double to = ClampGaugeValue(transition.To, lower, upper);
        double speed = double.IsFinite(transition.Speed) ? Math.Max(0, transition.Speed) : 65;
        double elapsed = Math.Max(0, transition.Elapsed.TotalSeconds);
        double span = Math.Abs(to - from);
        if (transition.Achilles && span > 0 && elapsed > 0)
        {
            // Replay the per-frame decay at 60 Hz, plus the final partial frame.
            double frames = elapsed * 60;
            double wholeFrames = Math.Floor(frames);
            double fraction = frames - wholeFrames;
            double step = speed / (60 * span);
            double remaining = Math.Pow(Math.Max(0, 1 - step), wholeFrames) *
                Math.Max(0, 1 - step * fraction);
            return to + (from - to) * remaining;
        }
        double distance = Math.Min(span, speed * elapsed);
        return from + Math.Sign(to - from) * distance;
    }

    private static double GaugeRatio(double value, PangyaUiGaugeRange? range, int defaultUpper)
    {
        int lower = range?.Lower ?? 0;
        int upper = range?.Upper ?? defaultUpper;
        return upper > lower ? (ClampGaugeValue(value, lower, upper) - (double)lower) / (upper - (double)lower) : 0;
    }

    private static double ClampGaugeValue(double value, int lower, int upper) =>
        double.IsNaN(value) ? lower : Math.Clamp(value, lower, upper);

    private static Color DisabledGaugeColor(Color color) =>
        Color.FromArgb(80, color.R * 176 >> 8, color.G * 176 >> 8, color.B * 176 >> 8);

    private static bool IsEdit(PangyaUiNode node) =>
        node.Type.Equals("EDIT", StringComparison.OrdinalIgnoreCase) || IsCombo(node);

    private static (Color Foreground, Color Outline) GetEditTextColors(PangyaUiNode node,
        PangyaUiRenderOptions options)
    {
        PangyaUiEditTextColors? colors = options.EditTextColors?.GetValueOrDefault(node);
        return (
            colors?.Foreground ?? ParseColor(node.Parameters.GetValueOrDefault("fontcolor"),
                Color.Gray, implicitAlpha: false),
            colors?.Outline ?? ParseColor(node.Parameters.GetValueOrDefault("fontcolor2"),
                Color.Silver, implicitAlpha: false));
    }

    private static bool IsExtendedCombo(PangyaUiNode node) =>
        node.Type.Equals("COMBOCTLEX", StringComparison.OrdinalIgnoreCase);

    public static bool IsImageCombo(PangyaUiNode node) =>
        IsExtendedCombo(node) && int.TryParse(node.Parameters.GetValueOrDefault("liststyle"),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out int style) && style == 1;

    private Rectangle DrawComboList(Graphics? graphics, PangyaUiNode node, Rectangle bounds,
        PangyaUiComboPreview combo, IReadOnlySet<string> symbols, PangyaUiRenderOptions options)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return Rectangle.Empty;
        IReadOnlyDictionary<string, string> parameters = node.Parameters;
        int leftMargin = ReadComboInteger(node, "leftmargin", 0);
        bool extended = IsExtendedCombo(node);
        var (listBounds, lineHeight, capacity, topRow, margin) =
            GetComboListGeometry(node, bounds, combo, options.ComboViewportHeight, symbols);
        if (listBounds.Height == 0) return Rectangle.Empty;
        Image? normalSkin = extended ? GetGaugeImage(node, "normal", symbols) : null;
        Image? hoverSkin = extended ? GetGaugeImage(node, "over", symbols) : null;
        bool imageRows = extended && IsImageCombo(node);
        int fontStyle = ReadHexTextStyle(parameters.GetValueOrDefault("font"));
        Color background = ParseColor(parameters.GetValueOrDefault("bgcolor"), Color.Transparent, implicitAlpha: false);
        Color highlightColor = ParseColor(parameters.GetValueOrDefault("selectcolor"),
            Color.FromArgb(0), implicitAlpha: false);
        if (highlightColor.ToArgb() == 0)
            highlightColor = Color.FromArgb(unchecked((int)0xff000000) | (~background.ToArgb() & 0xffffff));
        Rectangle highlightBounds = Rectangle.Empty;
        int hoveredRow = combo.HoveredIndex - topRow;
        if (!extended && highlightColor.A > 0 && (uint)hoveredRow < (uint)capacity)
        {
            int scrollbarWidth = GetBitmap("scroll_n_1", symbols)?.Width ?? 0;
            highlightBounds = new Rectangle(listBounds.X,
                ClampCoordinate((long)listBounds.Y + hoveredRow * (long)lineHeight + margin / 2),
                Math.Max(0, listBounds.Width - scrollbarWidth), lineHeight);
        }
        if (graphics is null)
        {
            Rectangle? measured = listBounds;
            IncludeBounds(ref measured, highlightBounds);
            if (extended || options.IncludeCaptionVisualBounds)
            {
                var bitmapText = options.DrawCaptionText?.Target as IPangyaUiBitmapTextPasses;
                for (int row = 0; row < capacity; row++)
                {
                    foreach (var artwork in GetRowArtwork(row))
                        IncludeBounds(ref measured, EncloseBounds(artwork.Bounds));
                    if (imageRows || !options.IncludeCaptionVisualBounds) continue;
                    string caption = LimitComboText(node, combo.Items[topRow + row].Text);
                    var (origin, style) = GetRowTextLayout(row);
                    IncludeBounds(ref measured, MeasureFrameText(caption, origin, options, bitmapText, style));
                    IncludeBounds(ref measured, MeasureCaptionArtwork(bitmapText, caption, origin, style));
                }
            }
            IncludeBounds(ref measured, DrawListScrollbar(measure: true));
            return measured ?? listBounds;
        }
        var textColors = GetEditTextColors(node, options);
        Color foreground = textColors.Foreground;
        using var back = new SolidBrush(background);
        using var text = new SolidBrush(foreground);
        using var selectedText = new SolidBrush(background);
        using var outline = new SolidBrush(textColors.Outline);
        using var highlight = new SolidBrush(highlightColor);
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        GraphicsState state = graphics.Save();
        try
        {
            if (!extended)
            {
                graphics.FillRectangle(back, listBounds);
                using var border = new Pen(ParseColor(parameters.GetValueOrDefault("bordercolor"),
                    Color.Transparent, implicitAlpha: false));
                graphics.DrawRectangle(border, listBounds.X, listBounds.Y, listBounds.Width - 1, listBounds.Height - 1);
            }
            for (int row = 0; row < capacity; row++)
            {
                int index = topRow + row;
                PangyaUiComboItem item = combo.Items[index];
                if (extended)
                {
                    foreach (var artwork in GetRowArtwork(row))
                        DrawGraphicsImage(graphics, artwork.Image, artwork.Bounds);
                    if (imageRows) continue;
                }
                bool highlightText = row == hoveredRow && !extended;
                if (highlightText && highlightBounds.Width > 0)
                    graphics.FillRectangle(highlight, highlightBounds);
                var (origin, style) = GetRowTextLayout(row);
                DrawStyledText(graphics, LimitComboText(node, item.Text), highlightText ? selectedText : text, outline,
                    origin, format, style, drawText: options.DrawCaptionText);
            }
        }
        finally { graphics.Restore(state); }
        // The scrollbar is a child window and draws after the rows.
        DrawListScrollbar(measure: false);
        return listBounds;

        (PointF Origin, int Style) GetRowTextLayout(int row)
        {
            int rowY = ClampCoordinate((long)listBounds.Y + row * (long)lineHeight);
            if (extended) return (new PointF(bounds.X + 0.5f, rowY + 0.5f), fontStyle);
            bool hovered = combo.HoveredIndex == topRow + row;
            int offset = hovered ? -2 : 0;
            return (new PointF(bounds.X + (float)leftMargin + offset,
                rowY + (float)margin + offset), hovered ? 2 : fontStyle);
        }

        IEnumerable<(Image Image, RectangleF Bounds)> GetRowArtwork(int row)
        {
            int index = topRow + row;
            var origin = new Point(bounds.X, ClampCoordinate((long)listBounds.Y + row * (long)lineHeight + margin));
            if (normalSkin is not null) yield return (normalSkin, new Rectangle(origin, normalSkin.Size));
            if (combo.HoveredIndex == index && hoverSkin is not null)
                yield return (hoverSkin, new Rectangle(origin, hoverSkin.Size));
            if (!imageRows) yield break;
            string? filename = combo.TextureImages is { } textures
                ? index < textures.Count ? textures[index] : null : combo.Items[index].Image;
            if (filename is { Length: > 0 } && GetBitmap(filename, symbols) is Image icon)
                yield return (icon, GetComboIconBounds(node, origin, normalSkin?.Width ?? bounds.Width, icon.Size));
        }

        Rectangle DrawListScrollbar(bool measure) =>
            DrawScrollBar(graphics, listBounds, Math.Max(1, capacity), combo.Items.Count,
                combo.ScrollThumbRow is float thumbRow && float.IsFinite(thumbRow) ? thumbRow : topRow,
                symbols, showGuide: options.ScrollbarGuides?.GetValueOrDefault(node) == true,
                focused: capacity > 0 && combo.Items.Count > capacity &&
                    ReferenceEquals(options.WheelFocusedNode, node) && IsWindowWheelEventEnabled(node, options),
                opacity: GetScrollbarOpacity(node, options), thumbOffset: combo.ScrollThumbOffset, measure: measure);
    }


    private static RectangleF GetComboIconBounds(PangyaUiNode node, Point origin, int itemWidth, Size icon)
    {
        int margin = ReadComboInteger(node, "leftmargin", 0);
        int.TryParse(node.Parameters.GetValueOrDefault("align"), NumberStyles.Integer,
            CultureInfo.InvariantCulture, out int alignment);
        double offset = alignment switch
        {
            1 => itemWidth * 0.5 + margin,
            2 => (long)itemWidth - margin,
            _ => margin
        };
        return new RectangleF((float)Math.Clamp(origin.X + offset - icon.Width / 2, int.MinValue, int.MaxValue),
            ClampCoordinate((long)origin.Y + 2), icon.Width, icon.Height);
    }

    private (Image Image, RectangleF Bounds)? GetSelectedComboArtwork(PangyaUiNode node, Rectangle button,
        PangyaUiComboPreview? combo, IReadOnlySet<string> symbols)
    {
        if (button.IsEmpty || combo is null || !IsImageCombo(node)) return null;
        string? filename = combo.ImageSelection is { } selection ? selection.Image :
            (uint)combo.SelectedIndex < (uint)combo.Items.Count ? combo.Items[combo.SelectedIndex].Image : null;
        if (filename is not { Length: > 0 } || GetBitmap(filename, symbols) is not Image image) return null;
        return (image, GetComboIconBounds(node, button.Location, button.Width, image.Size));
    }

    public int HitTestComboRow(PangyaUiNode node, Point point, PangyaUiRenderOptions options,
        bool includeScrollbar = false)
    {
        if (IsRuntimeHidden(node, options) || !IsCombo(node) || !IsRenderVisible(node, options) ||
            options.Combos?.GetValueOrDefault(node) is not { IsOpen: true } combo ||
            combo.Items.Count == 0)
            return -1;
        IReadOnlySet<string> symbols = Symbols(options);
        Rectangle edit = GetEditBounds(node, GetAbsoluteBounds(node, options), symbols, options);
        if (edit.Width <= 0 || edit.Height <= 0) return -1;
        var (bounds, lineHeight, capacity, topRow, _) =
            GetComboListGeometry(node, edit, combo, options.ComboViewportHeight, symbols);
        if (!bounds.Contains(point)) return -1;
        if (!includeScrollbar && TryGetComboScrollbarPage(node, point, options, out _, out _, out _))
            return -1;
        long row = ((long)point.Y - bounds.Y) / lineHeight;
        return row < capacity ? topRow + (int)row : -1;
    }

    public PangyaUiRenderOptions InitializeComboOpeningSide(PangyaUiNode node, int viewportHeight,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentOutOfRangeException.ThrowIfNegative(viewportHeight);
        if (!IsCombo(node)) throw new ArgumentException("Expected a combo control.", nameof(node));
        options ??= new();
        var combos = options.Combos?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        PangyaUiComboPreview combo = combos.GetValueOrDefault(node) ?? new([]);
        combos[node] = combo with
        {
            AutomaticOpensAbove = viewportHeight > 0 && GetAbsoluteBounds(node, options).Y > viewportHeight * 0.5
        };
        return options with { Combos = combos };
    }

    public Rectangle GetComboDropdownBounds(PangyaUiNode node, PangyaUiRenderOptions options, bool includeScrollbar = false)
    {
        if (IsRuntimeHidden(node, options) || !IsCombo(node) || !IsRenderVisible(node, options) ||
            options.Combos?.GetValueOrDefault(node) is not { IsOpen: true } combo)
            return Rectangle.Empty;
        IReadOnlySet<string> symbols = Symbols(options);
        Rectangle edit = GetEditBounds(node, GetAbsoluteBounds(node, options), symbols, options);
        if (includeScrollbar) return DrawComboList(null, node, edit, combo, symbols, options);
        return edit.Width > 0 && edit.Height > 0
            ? GetComboListGeometry(node, edit, combo, options.ComboViewportHeight, symbols).Bounds
            : Rectangle.Empty;
    }
    public bool TryGetComboScrollbarPage(PangyaUiNode node, Point point, PangyaUiRenderOptions options,
        out int rowDelta, out Rectangle thumbBounds, out Rectangle trackBounds)
    {
        bool hit = TryGetComboScrollbarGeometry(node, point, options, out rowDelta, out RectangleF thumb, out trackBounds);
        thumbBounds = EncloseBounds(thumb);
        return hit;
    }

    public bool TryGetComboScrollbarGeometry(PangyaUiNode node, Point point, PangyaUiRenderOptions options,
        out int rowDelta, out RectangleF thumbBounds, out Rectangle trackBounds)
    {
        rowDelta = 0;
        thumbBounds = RectangleF.Empty;
        trackBounds = Rectangle.Empty;
        if (IsRuntimeHidden(node, options) || !IsCombo(node) || !IsRenderVisible(node, options) ||
            options.Combos?.GetValueOrDefault(node) is not { IsOpen: true } combo) return false;
        IReadOnlySet<string> symbols = Symbols(options);
        Rectangle edit = GetEditBounds(node, GetAbsoluteBounds(node, options), symbols, options);
        var (bounds, _, capacity, topRow, _) =
            GetComboListGeometry(node, edit, combo, options.ComboViewportHeight, symbols);
        DrawScrollBarCore(null, bounds, capacity, combo.Items.Count, combo.ScrollThumbRow ?? topRow,
            symbols, false, out RectangleF thumb, thumbOffset: combo.ScrollThumbOffset);
        if (thumb.IsEmpty) return false;
        trackBounds = EncloseBounds(new RectangleF(thumb.X, bounds.Y, thumb.Width, bounds.Height));
        if (!trackBounds.Contains(point))
        {
            trackBounds = Rectangle.Empty;
            return false;
        }
        thumbBounds = thumb;
        rowDelta = point.Y < thumb.Top ? -capacity / 2 : point.Y > thumb.Bottom ? capacity / 2 : 0;
        return true;
    }
    public bool HitTestComboUnfold(PangyaUiNode node, Point point, IReadOnlySet<string> symbols) =>
        HitTestComboUnfold(node, point, symbols, null);

    public bool HitTestComboUnfold(PangyaUiNode node, Point point, IReadOnlySet<string> symbols,
        PangyaUiRenderOptions? options)
    {
        if (IsRuntimeHidden(node, options) || !IsCombo(node) || !IsRenderVisible(node, options) ||
            options?.DisabledNodes?.Contains(node) == true ||
            options?.ImageButtonStates?.GetValueOrDefault(node)?.Enabled == false ||
            !node.Parameters.ContainsKey("normal"))
            return false;
        Rectangle bounds = GetAbsoluteBounds(node, options);
        return GetComboButtonBounds(node, bounds, GetEditBounds(node, bounds, symbols, options), symbols)
            .Contains(point);
    }

    private Rectangle GetComboButtonBounds(PangyaUiNode node, Rectangle bounds, Rectangle editBounds,
        IReadOnlySet<string> symbols)
    {
        Rectangle button = GetButtonBounds(node, editBounds, symbols);
        if (button.IsEmpty) return button;
        int width = IsExtendedCombo(node) ? button.Width : node.Bounds.Width;
        // Fresh places the child button during Init. Parent SetRect moves it
        // with the parent but does not recenter it within the new size.
        int height = node.Bounds.Height;
        button.X = ClampCoordinate((long)editBounds.X + width - button.Width);
        button.Y = ClampCoordinate((long)bounds.Y + ((long)height - button.Height + 1) / 2);
        return button;
    }

    private (Rectangle Bounds, int LineHeight, int Capacity, int TopRow, int Margin)
        GetComboListGeometry(PangyaUiNode node, Rectangle bounds, PangyaUiComboPreview combo, int viewportHeight,
            IReadOnlySet<string> symbols)
    {
        bool extended = IsExtendedCombo(node);
        int margin = Math.Clamp(ReadComboInteger(node, "topmargin", 0), -4096, 4096);
        Image? rowSkin = extended ? GetGaugeImage(node, "normal", symbols) : null;
        int lineHeight = (int)Math.Clamp(rowSkin is not null
            ? rowSkin.Height + (long)margin : ReadComboInteger(node, "lineheight", 16),
            1, 8192);
        int capacity = GetComboRowCapacity(node, combo);
        int topRow = Math.Clamp(combo.TopRow, 0, combo.Items.Count - capacity);
        int viewportRows = rowSkin is not null ? GetComboRowLimit(node, combo) :
            Math.Max(1, Math.Min(combo.Items.Count, GetComboRowLimit(node, combo)));
        int height = Math.Max(0, viewportRows * lineHeight + (extended ? 0 : margin * 2));
        bool lowerHalf = combo.AutomaticOpensAbove ??
            (viewportHeight > 0 && GetAbsoluteBounds(node, null).Y > viewportHeight * 0.5);
        bool above = combo.Direction == PangyaUiComboDirection.Above ||
            (combo.Direction == PangyaUiComboDirection.Auto && lowerHalf);
        long y = extended
            ? (long)bounds.Y + (rowSkin is not null ? rowSkin.Height + margin : lowerHalf ? -5 : 5)
            : above ? (long)bounds.Y - height - 2 : (long)bounds.Y + bounds.Height + 2;
        // ResizeListRect starts from the child list's rectangle and changes only height and Y.
        int width = rowSkin?.Width ?? (node.Bounds.Width > 0 ? node.Bounds.Width : bounds.Width);
        return (new Rectangle(bounds.X, ClampCoordinate(y), width, height), lineHeight, capacity, topRow, margin);
    }

    private static PangyaUiComboPreview ResolveReferencedComboSide(PangyaUiComboPreview combo, int y,
        PangyaUiRenderOptions options) => options.ResourceInstance is null || combo.AutomaticOpensAbove.HasValue
            ? combo
            : combo with { AutomaticOpensAbove = options.ComboViewportHeight > 0 && y > options.ComboViewportHeight * 0.5 };

    public PangyaUiRenderOptions AddComboTexture(PangyaUiNode node, string filename,
        PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(filename);
        ArgumentNullException.ThrowIfNull(options);
        if (!IsImageCombo(node)) return options;
        PangyaUiComboPreview combo = options.Combos?.GetValueOrDefault(node) ?? new([]);
        int count = checked((combo.StoredTextRowCount ?? combo.Items.Count) + 1);
        string? image = GetBitmap(filename, Symbols(options)) is not null ? filename : null;
        var textures = combo.TextureImages?.ToList() ?? combo.Items.Select(item => item.Image).OfType<string>()
            .Where(existing => existing.Length > 0 && GetBitmap(existing, Symbols(options)) is not null).ToList();
        if (image is not null) textures.Add(image);
        var items = combo.Items.ToList();
        items.Add(new(count.ToString(CultureInfo.InvariantCulture), image));
        var combos = options.Combos?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        combos[node] = combo with { Items = items.ToArray(), StoredTextRowCount = count,
            TextureImages = textures.ToArray() };
        return options with { Combos = combos };
    }

    public PangyaUiRenderOptions SelectComboImage(PangyaUiNode node, int index,
        PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (!IsImageCombo(node)) throw new ArgumentException("The node must be an image combo.", nameof(node));
        PangyaUiComboPreview combo = options.Combos?.GetValueOrDefault(node) ?? new([]);
        string? selected = null;
        int textureIndex = 0;
        foreach (string filename in combo.TextureImages ?? combo.Items.Select(item => item.Image).OfType<string>().ToArray())
        {
            if (filename is not { Length: > 0 } || GetBitmap(filename, Symbols(options)) is null)
                continue;
            if (textureIndex++ == index) { selected = filename; break; }
        }
        var combos = options.Combos?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        combos[node] = combo with { ImageSelection = new(selected,
            selected is not null ? unchecked((byte)index) : (byte)0) };
        return options with { Combos = combos };
    }

    public static PangyaUiRenderOptions ClearComboImageList(PangyaUiNode node,
        PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (!IsImageCombo(node)) throw new ArgumentException("The node must be an image combo.", nameof(node));
        PangyaUiComboPreview? combo = options.Combos?.GetValueOrDefault(node);
        if (combo is null) return options;
        var selected = combo.ImageSelection ?? new PangyaUiComboImageSelection(
            (uint)combo.SelectedIndex < (uint)combo.Items.Count ? combo.Items[combo.SelectedIndex].Image : null,
            combo.SelectedIndex >= 0 ? unchecked((byte)combo.SelectedIndex) : (byte)0);
        var combos = options.Combos!.ToDictionary(pair => pair.Key, pair => pair.Value);
        combos[node] = combo with { Items = Array.Empty<PangyaUiComboItem>(), ImageSelection = selected,
            StoredTextRowCount = combo.StoredTextRowCount ?? combo.Items.Count,
            TextureImages = Array.Empty<string>(),
            TopRow = 0, ScrollThumbRow = null, ScrollThumbOffset = null };
        return options with { Combos = combos };
    }

    public static int GetComboMaximumTopRow(PangyaUiNode node, PangyaUiComboPreview combo) =>
        combo.Items.Count - GetComboRowCapacity(node, combo);

    private static int GetComboRowCapacity(PangyaUiNode node, PangyaUiComboPreview combo)
    {
        int rows = Math.Min(combo.Items.Count, GetComboRowLimit(node, combo));
        if (IsExtendedCombo(node)) return rows;
        int margin = Math.Clamp(ReadComboInteger(node, "topmargin", 0), -4096, 4096);
        int lineHeight = Math.Clamp(ReadComboInteger(node, "lineheight", 16), 1, 8192);
        return Math.Min(combo.Items.Count, Math.Max(0, (Math.Max(1, rows) * lineHeight + margin * 2) / lineHeight));
    }

    private static int GetComboRowLimit(PangyaUiNode node, PangyaUiComboPreview combo)
    {
        int configured = Math.Clamp(ReadComboInteger(node, "maxlistnum", 10), 1, 256);
        return !IsExtendedCombo(node) && combo.IncreasedMaximumRows is int requested
            ? Math.Clamp(requested, configured, 256) : configured;
    }

    private static int ReadComboInteger(PangyaUiNode node, string name, int fallback)
    {
        string? value = node.Parameters.GetValueOrDefault(name);
        ReadOnlySpan<char> prefix = value.AsSpan().TrimStart();
        if (!prefix.IsEmpty && prefix[0] is '+' or '-') prefix = prefix[1..];
        return !prefix.IsEmpty && char.IsAsciiDigit(prefix[0]) ? ReadDecimalPrefix(value) : fallback;
    }
    public static bool IsMultilineEdit(PangyaUiNode node) =>
        node.Type.Equals("EDIT", StringComparison.OrdinalIgnoreCase) &&
        (node.Parameters.GetValueOrDefault("multiline") is not string value ||
         (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int flag) && flag != 0));
    private static bool IsCombo(PangyaUiNode node) =>
        node.Type.Equals("COMBOBOX", StringComparison.OrdinalIgnoreCase) || IsExtendedCombo(node);

    private Rectangle GetEditBounds(PangyaUiNode node, Rectangle bounds, IReadOnlySet<string> symbols,
        PangyaUiRenderOptions? options) =>
        options?.WindowSizes?.ContainsKey(node) != true &&
        IsExtendedCombo(node) && GetGaugeImage(node, "normal", symbols) is Image skin
            ? new Rectangle(bounds.Location, skin.Size) : bounds;

    private static string MaskEditPassword(string text)
    {
        var masked = new StringBuilder(text.Length);
        int start = 0;
        foreach (System.Text.RegularExpressions.Match tag in EditCaptionTags.Matches(text))
        {
            masked.Append('*', text[start..tag.Index].EnumerateRunes().Count());
            masked.Append(tag.Value);
            start = tag.Index + tag.Length;
        }
        masked.Append('*', text[start..].EnumerateRunes().Count());
        return masked.ToString();
    }

    private Rectangle MeasureEditBounds(PangyaUiNode node, Rectangle bounds, IReadOnlySet<string> symbols,
        PangyaUiRenderOptions options)
    {
        Rectangle editBounds = GetEditBounds(node, bounds, symbols, options);
        Rectangle? measured = DrawEdit(null, node, editBounds, options.EditLines?.GetValueOrDefault(node),
            enabled: options.DisabledNodes?.Contains(node) != true, caret: options.EditCarets?.GetValueOrDefault(node), options: options);
        if (IsCombo(node) && node.Parameters.ContainsKey("normal"))
        {
            Rectangle button = GetComboButtonBounds(node, bounds, editBounds, symbols);
            IncludeBounds(ref measured, button);
            if (GetSelectedComboArtwork(node, button, options.Combos?.GetValueOrDefault(node), symbols) is { } artwork)
                IncludeBounds(ref measured, Rectangle.FromLTRB(
                    ClampCoordinate((long)MathF.Floor(artwork.Bounds.Left)),
                    ClampCoordinate((long)MathF.Floor(artwork.Bounds.Top)),
                    ClampCoordinate((long)MathF.Ceiling(artwork.Bounds.Right)),
                    ClampCoordinate((long)MathF.Ceiling(artwork.Bounds.Bottom))));
        }
        return measured ?? Rectangle.Empty;
    }

    private Rectangle DrawEdit(Graphics? graphics, PangyaUiNode node, Rectangle bounds,
        IReadOnlyList<string>? lines, bool enabled, PangyaUiEditCaret? caret, PangyaUiRenderOptions options)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return bounds;
        IReadOnlyDictionary<string, string> parameters = node.Parameters;
        Color background = ParseColor(parameters.GetValueOrDefault("bgcolor"), Color.Transparent, implicitAlpha: false);
        Color border = ParseColor(parameters.GetValueOrDefault("bordercolor"), Color.Transparent, implicitAlpha: false);
        byte opacity = GetScrollbarOpacity(node, options,
            options.EditOpacity?.GetValueOrDefault(node, byte.MaxValue) ?? byte.MaxValue);
        double backgroundAlpha = GetWindowAlphaScale(node, options) *
            (options.EditOpacity?.GetValueOrDefault(node, byte.MaxValue) ?? byte.MaxValue) / byte.MaxValue;
        background = Color.FromArgb((int)(background.A * backgroundAlpha), background);
        Color borderTint = Color.FromArgb((int)(border.A * backgroundAlpha), border);
        if (graphics is not null && background.A > 0)
        {
            using var brush = new SolidBrush(background);
            graphics.FillRectangle(brush, bounds);
        }
        if (graphics is not null && border.A > 0)
        {
            using var pen = new Pen(borderTint);
            graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);
        }
        bool initializedReadOnlyStyle = parameters.GetValueOrDefault("readonly") == "true";
        bool readOnlyStyle = options.EditReadOnlyStyles?.GetValueOrDefault(node,
            initializedReadOnlyStyle) ?? initializedReadOnlyStyle;
        bool keyEvents = options.EditKeyEvents?.TryGetValue(node, out bool suppliedKeyEvents) == true
            ? suppliedKeyEvents
            : !initializedReadOnlyStyle &&
                parameters.GetValueOrDefault("focusoff") != "true";
        if (caret is not null && (!enabled || !keyEvents))
            caret = null;
        bool showGuide = options.ScrollbarGuides?.GetValueOrDefault(node) == true;
        if (lines is null || lines.Count == 0)
        {
            if (caret is null && !showGuide) return bounds;
            lines = [string.Empty];
        }

        int ReadInt(string name, int fallback) =>
            int.TryParse(parameters.GetValueOrDefault(name), NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int value) ? value : fallback;

        int lineHeight = ReadInt("lineheight", 16);
        if (lineHeight <= 0) return bounds;
        int leftMargin = ReadInt("leftmargin", 0);
        int topMargin = ReadInt("topmargin", 0);
        bool multiline = IsMultilineEdit(node);
        int count = multiline ? lines.Count : 1;
        int vertical = ReadInt("valign", string.Equals(parameters.GetValueOrDefault("vcenteralign"),
            "true", StringComparison.OrdinalIgnoreCase) ? 1 : 0);
        int capacity = GetEditRowCapacity(bounds.Height, topMargin, lineHeight);
        int maximum = Math.Max(0, count - Math.Max(1, capacity));
        float scrollRow = GetEditScrollRow(node, options, caret, capacity, maximum);
        if (graphics is null)
        {
            Rectangle? measured = bounds;
            IncludeBounds(ref measured, DrawScrollbar(measure: true));
            return measured ?? bounds;
        }
        int firstLine = (int)Math.Clamp((double)(scrollRow + 0.5f), 0, count);
        long remaining = firstLine == 0
            ? Math.Max(0L, (long)bounds.Height - (long)count * lineHeight - topMargin) : 0;
        long y = (long)bounds.Y + topMargin + (vertical == 1 ? remaining / 2 : vertical == 2 ? remaining : 0);
        using var format = new StringFormat(StringFormatFlags.NoWrap)
        {
            Alignment = ReadInt("align", 0) switch
            {
                1 => StringAlignment.Center,
                2 => StringAlignment.Far,
                _ => StringAlignment.Near
            }
        };
        var textColors = GetEditTextColors(node, options);
        Color foreground = textColors.Foreground;
        using var text = new SolidBrush(enabled ? foreground : Color.FromArgb(foreground.A * 2 / 5, foreground));
        using var outline = new SolidBrush(enabled
            ? textColors.Outline : Color.White);
        int fontStyle = ReadHexTextStyle(parameters.GetValueOrDefault("font"));
        float left = ClampCoordinate((long)bounds.X + leftMargin);
        bool symmetricMargins = string.Equals(parameters.GetValueOrDefault("symmetry", "true"), "true",
            StringComparison.OrdinalIgnoreCase);
        int scrollbarWidth = multiline ? GetBitmap("scroll_n_1", Symbols(options))?.Width ?? 0 : 0;
        float available = (long)bounds.Width - leftMargin * (symmetricMargins ? 2L : 1L) - scrollbarWidth;
        float anchor = left + GetEditAlignmentOffset(format.Alignment, available);
        bool password = string.Equals(parameters.GetValueOrDefault("password"), "true",
            StringComparison.OrdinalIgnoreCase);
        IReadOnlySet<string>? emoticonSymbols = !password && readOnlyStyle
            ? Symbols(options)
            : null;
        bool measureEmoticons = options.EditEmoticonStyles?.GetValueOrDefault(node, true) ?? true;
        Rectangle content = border.A > 0
            ? new Rectangle(ClampCoordinate((long)bounds.X + 1), ClampCoordinate((long)bounds.Y + 1),
                bounds.Width - 1, bounds.Height - 1)
            : bounds;
        if (IsPrivacyHidden(node, options) || content.Width <= 0 || content.Height <= 0)
        {
            DrawScrollbar(measure: false);
            return bounds;
        }
        GraphicsState state = graphics.Save();
        try
        {
            graphics.SetClip(content, CombineMode.Intersect);
            long bottom = (long)bounds.Y + bounds.Height;
            Font font = SystemFonts.DefaultFont;
            float scroll = 0;
            float caretX = anchor;
            if (caret is not null && (uint)caret.Line < (uint)count)
            {
                string originalLine = lines[caret.Line];
                string line = password ? MaskEditPassword(originalLine) : originalLine;
                using var measure = (StringFormat)StringFormat.GenericTypographic.Clone();
                measure.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
                string visibleLine = EditCaptionTags.Replace(line, "");
                if (readOnlyStyle)
                {
                    float lineWidth = MeasureCaptionLine(graphics,
                        TrimEditAlignmentSpace(EditCaptionTags.Replace(originalLine, "")),
                        font, measure, measureEmoticons ? emoticonSymbols : null,
                        measureText: options.MeasureCaptionText);
                    caretX = left + GetEditAlignmentOffset(format.Alignment, available - lineWidth);
                }
                int visibleColumn = Math.Clamp(caret.Column, 0, originalLine.Length);
                string untaggedLine = EditCaptionTags.Replace(originalLine, "");
                foreach (System.Text.RegularExpressions.Match tag in EditCaptionTags.Matches(originalLine))
                    visibleColumn -= Math.Clamp(caret.Column - tag.Index, 0, tag.Length);
                if (password)
                    visibleColumn = untaggedLine[..Math.Clamp(visibleColumn, 0, untaggedLine.Length)]
                        .EnumerateRunes().Count();
                visibleColumn = Math.Clamp(visibleColumn, 0, visibleLine.Length);
                float prefixWidth = MeasureCaptionLine(graphics, visibleLine[..visibleColumn],
                    font, measure, measureEmoticons ? emoticonSymbols : null,
                    measureText: options.MeasureCaptionText);
                caretX += prefixWidth;
                float viewportWidth = (long)bounds.Width - leftMargin * 2L;
                scroll = readOnlyStyle ? 0 : MathF.Truncate(Math.Max(0, prefixWidth - viewportWidth));
            }
            for (int index = firstLine; index < count && y + lineHeight <= bottom; index++, y += lineHeight)
            {
                if (y + lineHeight <= bounds.Y) continue;
                string originalLine = lines[index];
                string line = password ? MaskEditPassword(originalLine) : originalLine.Replace('\t', ' ');
                int lineStyle = options.EditLineStyles?.GetValueOrDefault(node) is { } styles &&
                    index < styles.Count ? styles[index] : fontStyle;
                TimeSpan? animationElapsed = options.EditEmoticonAnimationTime ?? options.EmoticonAnimationTime;
                if (emoticonSymbols is not null &&
                    options.EditLineAnimationStartTicks?.GetValueOrDefault(node) is { } starts && index < starts.Count)
                    animationElapsed = TimeSpan.FromMilliseconds(unchecked((uint)(options.CurrentTick - starts[index])));
                var formatting = DrawTaggedCaption(graphics, line, text, outline,
                    new PointF(left - (password ? 0 : scroll), ClampCoordinate(y)), format, lineStyle,
                    emoticonSymbols: emoticonSymbols,
                    animationElapsed: animationElapsed,
                    useEditAnimationMarker: emoticonSymbols is not null,
                    forceOpaqueText: emoticonSymbols is not null,
                    editTags: true, defaultTagFill: foreground,
                    lineHeight: options.CaptionFontHeight,
                    measureText: options.MeasureCaptionText,
                    drawText: options.DrawCaptionText,
                    editAvailableWidth: available,
                    splitEditWords: !password,
                    justifyEditLine: !password && options.JustifiedEditLines?.GetValueOrDefault(node)?.Contains(index) == true,
                    editAlignmentText: readOnlyStyle
                        ? TrimEditAlignmentSpace(EditCaptionTags.Replace(originalLine, "")) : string.Empty,
                    defaultTagEdge: textColors.Outline,
                    measureEmoticons: measureEmoticons);
                text.Color = formatting.Fill;
                outline.Color = formatting.Edge;
                if (caret?.Line == index)
                {
                    double phase = Math.Max(0, caret.Elapsed.TotalSeconds) % 1;
                    if (caret.Steady || phase > 0 && phase < 0.8)
                    {
                        float x = caretX - scroll;
                        float caretHeight = options.CaptionFontHeight ?? font.GetHeight(graphics);
                        float top = ClampCoordinate(y);
                        using var gradient = new LinearGradientBrush(new RectangleF(x, top, 1, caretHeight),
                            ParseColor(parameters.GetValueOrDefault("caretcolor"), Color.Gray, implicitAlpha: false),
                            ParseColor(parameters.GetValueOrDefault("caretcolor2"), Color.Silver, implicitAlpha: false),
                            LinearGradientMode.Vertical);
                        graphics.FillRectangle(gradient, x, top, 1, caretHeight);
                    }
                }
            }
        }
        finally { graphics.Restore(state); }
        DrawScrollbar(measure: false);
        return bounds;

        Rectangle DrawScrollbar(bool measure) => IsCombo(node) ? Rectangle.Empty :
                DrawScrollBar(graphics, bounds, capacity, count, scrollRow, Symbols(options), showGuide,
                    focused: capacity > 0 && count > capacity &&
                        ReferenceEquals(options.WheelFocusedNode, node) && IsWindowWheelEventEnabled(node, options),
                    opacity: opacity, retainTopRow: true, measure: measure);
    }

    private static int GetEditRowCapacity(int height, int margin, int lineHeight) =>
        lineHeight <= 0 ? 0 : (int)Math.Clamp(((long)height - margin) / lineHeight, 0, int.MaxValue);

    private static float GetEditScrollRow(PangyaUiNode node, PangyaUiRenderOptions options,
        PangyaUiEditCaret? caret, int capacity, int maximum)
    {
        if (options.EditScrollRows?.TryGetValue(node, out float row) == true && float.IsFinite(row))
            return Math.Max(row, 0);
        if (options.EditTopRows?.TryGetValue(node, out int requested) == true)
            return Math.Clamp(requested, 0, maximum);
        return caret is null ? maximum : Math.Clamp(caret.Line - Math.Max(1, capacity) + 1, 0, maximum);
    }

    public static bool TryScrollEditWheel(PangyaUiNode node, int lineCount, int delta, PangyaUiRenderOptions options,
        out PangyaUiRenderOptions updated)
    {
        updated = options;
        if (!node.Type.Equals("EDIT", StringComparison.OrdinalIgnoreCase) || !IsMultilineEdit(node) ||
            IsRuntimeHidden(node, options) || !IsWindowWheelEventEnabled(node, options)) return false;
        int capacity = GetEditRowCapacity(node, options);
        if (capacity == 0 || lineCount <= capacity) return false;
        float row = GetEditScrollRow(node, options, options.EditCarets?.GetValueOrDefault(node),
            capacity, lineCount - capacity);
        var scroll = PangyaUiScrollState.RestoreRows(capacity, lineCount, row,
            options.EditFollowBottom?.GetValueOrDefault(node, true) ?? true);
        scroll.ShowGuide = options.ScrollbarGuides?.GetValueOrDefault(node) == true;
        if (!scroll.ApplyWheel(delta)) return false;
        updated = scroll.ApplyToEdit(node, options);
        return true;
    }

    public static int GetEditMaximumTopRow(PangyaUiNode node, int lineCount) =>
        GetEditMaximumTopRow(node, lineCount, null);

    public static int GetEditMaximumTopRow(PangyaUiNode node, int lineCount,
        PangyaUiRenderOptions? options)
    {
        if (!node.Type.Equals("EDIT", StringComparison.OrdinalIgnoreCase) ||
            !IsMultilineEdit(node)) return 0;
        return Math.Max(0, lineCount - Math.Max(1, GetEditRowCapacity(node, options)));
    }

    private static int GetEditRowCapacity(PangyaUiNode node, PangyaUiRenderOptions? options)
    {
        int Read(string name, int fallback) => int.TryParse(node.Parameters.GetValueOrDefault(name),
            NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : fallback;
        return GetEditRowCapacity(GetWindowBounds(node, options).Height, Read("topmargin", 0), Read("lineheight", 16));
    }

    private static int ReadHexTextStyle(string? value, int fallback = 0) =>
        int.TryParse(ReadHexPrefix(value), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int style)
            ? style : fallback;

    private static int ReadDecimalPrefix(string? value) => PangyaUiDimensionHelper.ReadDecimalPrefix(value);

    private static ReadOnlySpan<char> ReadHexPrefix(string? value)
    {
        ReadOnlySpan<char> hex = value.AsSpan().TrimStart();
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) hex = hex[2..];
        int length = 0;
        while (length < hex.Length && Uri.IsHexDigit(hex[length])) length++;
        return hex[..length];
    }

    public int HitTestTextButtonSegment(PangyaUiNode node, Point point, PangyaUiRenderOptions options)
    {
        Rectangle bounds = GetAbsoluteBounds(node, options);
        if (IsRuntimeHidden(node, options) || !IsRenderVisible(node, options) ||
            options.DisabledNodes?.Contains(node) == true ||
            point.Y < bounds.Top || (long)point.Y >= (long)bounds.Y + bounds.Height ||
            GetTextButtonCaption(node, options).Length == 0)
            return -1;
        using var bitmap = new Bitmap(1, 1);
        using Graphics metrics = Graphics.FromImage(bitmap);
        return DrawTextButton(metrics, node, bounds, GetTextButtonCaption(node, options),
            options with { HoveredNode = node, Pointer = point }, out _, draw: false);
    }

    private static int DrawTextButton(Graphics graphics, PangyaUiNode node, Rectangle bounds,
        string caption, PangyaUiRenderOptions options, out Rectangle measuredBounds, bool draw = true,
        Point clipOrigin = default)
    {
        Font font = SystemFonts.DefaultFont;
        using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
        format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;
        string separator = GetTextButtonSeparator(options);
        string[] segments = caption.Split(separator, StringSplitOptions.None);
        float x = bounds.X;
        if (options.TextButtonCaptionGeometryNodes?.Contains(node) != true && IsTextButtonCentered(node))
            x += (int)((bounds.Width - Measure(caption) + 1) / 2);

        float startX = x;
        measuredBounds = Rectangle.Empty;
        using var normal = new SolidBrush(ParseColor(node.Parameters.GetValueOrDefault("color"), Color.White, implicitAlpha: false));
        using var highlight = new SolidBrush(ParseColor(
            node.Parameters.GetValueOrDefault("select_color"), Color.FromArgb(255, 204, 0), implicitAlpha: false));
        using var outline = new SolidBrush(ParseColor(node.Parameters.GetValueOrDefault("color2"), Color.Black, implicitAlpha: false));
        using var selectedOutline = new SolidBrush(ParseColor(
            node.Parameters.GetValueOrDefault("select_color2"), Color.Black, implicitAlpha: false));
        PangyaUiMenuState? selection = options.MenuStates?.GetValueOrDefault(node);
        byte alpha = selection?.SelectionAlpha ?? byte.MaxValue;
        using var pulsedHighlight = new SolidBrush(Color.FromArgb(highlight.Color.A * alpha / 255, highlight.Color));
        using var pulsedOutline = new SolidBrush(Color.FromArgb(selectedOutline.Color.A * alpha / 255,
            selectedOutline.Color));
        double opacity = GetWindowAlphaScale(node, options, secondary: false) *
            (selection?.Opacity ?? byte.MaxValue);
        normal.Color = ApplyOpacity(normal.Color);
        outline.Color = ApplyOpacity(outline.Color);
        highlight.Color = ApplyOpacity(highlight.Color);
        selectedOutline.Color = ApplyOpacity(selectedOutline.Color);
        int style = ReadHexTextStyle(node.Parameters.GetValueOrDefault("style"), 1);
        int selectedStyle = ReadHexTextStyle(node.Parameters.GetValueOrDefault("sel_style"), 1);
        Rectangle? glyphClip = options.CaptionClippingEnabled?.GetValueOrDefault(node, true) != false &&
            options.CaptionClips?.TryGetValue(node, out Rectangle clip) == true ? clip : null;
        float separatorWidth = Measure(separator);
        bool measureVisuals = !draw && options.IncludeCaptionVisualBounds;
        var bitmapText = measureVisuals
            ? options.DrawCaptionText?.Target as IPangyaUiBitmapTextPasses : null;
        Rectangle? visuals = null;
        for (int index = 0; index < segments.Length; index++)
        {
            string segment = segments[index];
            if (segment.Length == 0) break;
            float segmentWidth = Measure(segment);
            bool hasSeparator = index + 1 < segments.Length && segments[index + 1].Length > 0;
            bool hovered = options.DisabledNodes?.Contains(node) != true &&
                ReferenceEquals(options.HoveredNode, node) && options.Pointer is Point pointer &&
                pointer.Y >= bounds.Top && (long)pointer.Y < (long)bounds.Top + bounds.Height &&
                pointer.X >= x && pointer.X < x + segmentWidth + separatorWidth;
            if (!draw && hovered) return index;
            bool forced = index == 0 && selection?.SelectedRow is not >= 0 && options.ButtonState != PangyaUiButtonState.Normal &&
                !ReferenceEquals(options.HoveredNode, node);
            bool selected = selection?.SelectedRow == index;
            int segmentStyle = hovered || forced || selected ? selectedStyle : style;
            var origin = new PointF(x, bounds.Y);
            SolidBrush fill = hovered || forced ? highlight : selected ? pulsedHighlight : normal;
            SolidBrush edge = hovered || forced ? selectedOutline : selected ? pulsedOutline : outline;
            if (measureVisuals && HasVisibleInk(fill, edge, segmentStyle))
                IncludeVisuals(segment, origin, segmentWidth, segmentStyle);
            if (draw)
                DrawStyledText(graphics, segment, fill, edge, origin, format, segmentStyle,
                    font, options.DrawCaptionText, glyphClip);
            x += segmentWidth;
            if (hasSeparator)
            {
                if (measureVisuals && HasVisibleInk(normal, outline, style))
                    IncludeVisuals(separator, new PointF(x, bounds.Y), separatorWidth, style);
                if (draw) DrawStyledText(graphics, separator, normal, outline, new PointF(x, bounds.Y),
                    format, style, font, options.DrawCaptionText, glyphClip);
                x += separatorWidth;
            }
        }
        measuredBounds = Rectangle.Ceiling(new RectangleF(startX, bounds.Y, x - startX, bounds.Height));
        if (visuals is Rectangle visualBounds) measuredBounds = Rectangle.Union(measuredBounds, visualBounds);
        return -1;

        static bool HasVisibleInk(SolidBrush fill, SolidBrush edge, int textStyle) =>
            fill.Color.A > 0 || (textStyle is 2 or 4 && edge.Color.A > 0);

        void IncludeVisuals(string run, PointF origin, float width, int textStyle)
        {
            IncludeBounds(ref visuals, MeasureCaptionArtwork(bitmapText, run, origin, textStyle));
            Rectangle glyphs = MeasureGlyphExtent(origin, bitmapText?.GetDrawAdvance(run) ?? width,
                options.CaptionFontHeight ?? font.GetHeight(graphics), textStyle);
            if (glyphClip is Rectangle area) glyphs = IntersectAreaClip(glyphs, area, clipOrigin);
            IncludeBounds(ref visuals, glyphs);
        }

        Color ApplyOpacity(Color color) => Color.FromArgb((int)(color.A * opacity / byte.MaxValue), color);

        float Measure(string text) => options.MeasureCaptionText?.Invoke(text) ??
            graphics.MeasureString(text, font, PointF.Empty, format).Width;

    }

    public static bool HasButtonSkin(PangyaUiNode node) =>
        node.Type.Equals("BUTTON", StringComparison.OrdinalIgnoreCase) &&
        node.Parameters.ContainsKey("normal");

    public static bool HasButtonSkin(PangyaUiNode node, PangyaUiRenderOptions options) =>
        HasButtonSkin(node) || (node.Type.Equals("BUTTON", StringComparison.OrdinalIgnoreCase) &&
            (options.ImageButtonStates?.ContainsKey(node) == true ||
             options.WindowSizes?.ContainsKey(node) == true ||
             (options.ButtonOwnerDraw?.ContainsKey(node) == true &&
              ReadDecimalPrefix(node.Parameters.GetValueOrDefault("drawstyle")) != 0)));

    private Rectangle GetButtonBounds(PangyaUiNode node, Rectangle bounds, IReadOnlySet<string> symbols,
        PangyaUiRenderOptions? options = null)
    {
        if (options?.WindowSizes?.TryGetValue(node, out Size suppliedSize) == true)
            return new Rectangle(bounds.Location,
                new Size(Math.Max(0, suppliedSize.Width), Math.Max(0, suppliedSize.Height)));
        PangyaUiImageButtonState? state = options?.ImageButtonStates?.GetValueOrDefault(node);
        Image? image = GetReplacementButtonImage(state, symbols);
        if (image is null)
        {
            if (state?.BitmapImages?.TryGetValue("normal", out Image? supplied) == true)
                image = supplied;
            else
                image = GetRuntimeButtonImage(state, "normal", symbols) ??
                    (node.Parameters.TryGetValue("normal", out string? normal) ? GetBitmap(normal, symbols) : null);
        }
        return image is null ? Rectangle.Empty : new Rectangle(bounds.Location, image.Size);
    }

    private Rectangle MeasureButtonBounds(PangyaUiNode node, Rectangle bounds, IReadOnlySet<string> symbols,
        PangyaUiRenderOptions options)
    {
        Rectangle button = GetButtonBounds(node, bounds, symbols, options);
        if (options.ButtonOwnerDrawBounds?.TryGetValue(node, out Rectangle artwork) != true ||
            options.ButtonOwnerDraw?.ContainsKey(node) != true ||
            ReadDecimalPrefix(node.Parameters.GetValueOrDefault("drawstyle")) == 0) return button;
        artwork.Location = AddOrigins(button == Rectangle.Empty ? bounds.Location : button.Location, artwork.Location);
        Rectangle? measured = null;
        IncludeBounds(ref measured, button);
        IncludeBounds(ref measured, artwork);
        return measured ?? button;
    }

    public static int GetButtonPushStyle(PangyaUiNode node, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        return options?.ImageButtonStates?.GetValueOrDefault(node)?.PushStyle ??
            (node.Parameters.TryGetValue("pushstyle", out string? value) ? ReadDecimalPrefix(value) : 1);
    }

    public static PangyaUiRenderOptions InitializeExternalButton(PangyaUiNode node, Image normal,
        Image? hover, Image? selected, int style, int pushStyle, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(normal);
        options ??= new();
        ValidateButtonImageArguments(node, "normal", options);
        var states = options.ImageButtonStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var state = states.GetValueOrDefault(node) ?? new(Enabled: options.DisabledNodes?.Contains(node) != true);
        var bitmaps = state.BitmapImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        bitmaps["normal"] = normal;
        bitmaps["over"] = hover;
        bitmaps["selected"] = selected;
        states[node] = state with { BitmapImages = bitmaps, Style = style, PushStyle = pushStyle,
            ReplacementImage = null, ReplacementMode = null };
        var sizes = options.WindowSizes?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        sizes[node] = normal.Size;
        return options with { ImageButtonStates = states, WindowSizes = sizes };
    }

    public bool TrySetButtonImage(PangyaUiNode node, string? filename, string mode,
        PangyaUiRenderOptions options, out PangyaUiRenderOptions updatedOptions)
    {
        ValidateButtonImageArguments(node, mode, options);
        updatedOptions = options;
        if (filename is null || GetBitmap(filename, Symbols(options)) is not Image image) return false;
        var states = options.ImageButtonStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        PangyaUiImageButtonState state = states.GetValueOrDefault(node) ??
            new(Enabled: options.DisabledNodes?.Contains(node) != true);
        var slots = state.Images?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var bitmaps = state.BitmapImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        bitmaps[mode] = image;
        slots[mode] = filename;
        states[node] = state with { Images = slots, BitmapImages = bitmaps,
            ReplacementMode = mode, ReplacementImage = filename };
        var sizes = options.WindowSizes?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        sizes[node] = image.Size;
        updatedOptions = options with { ImageButtonStates = states, WindowSizes = sizes };
        return true;
    }

    public bool TrySetButtonBelowImage(PangyaUiNode node, string? filename, string mode,
        PangyaUiRenderOptions options, out PangyaUiRenderOptions updatedOptions)
    {
        ValidateButtonImageArguments(node, mode, options);
        updatedOptions = options;
        Image? image = filename is null ? null : GetBitmap(filename, Symbols(options));
        if (filename is not null && image is null) return false;
        var states = options.ImageButtonStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        PangyaUiImageButtonState state = states.GetValueOrDefault(node) ??
            new(Enabled: options.DisabledNodes?.Contains(node) != true);
        var slots = state.BelowImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        slots[mode] = filename;
        var bitmaps = state.BitmapImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        bitmaps["below_" + mode] = image;
        states[node] = state with { BelowImages = slots, BitmapImages = bitmaps };
        updatedOptions = options with { ImageButtonStates = states };
        return true;
    }

    private static void ValidateButtonImageArguments(PangyaUiNode node, string mode, PangyaUiRenderOptions options)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (!node.Type.Equals("BUTTON", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The node must be a button.", nameof(node));
        if (mode is not ("normal" or "over" or "selected" or "blink"))
            throw new ArgumentOutOfRangeException(nameof(mode));
    }

    private Image? GetReplacementButtonImage(PangyaUiImageButtonState? state, IReadOnlySet<string> symbols)
    {
        if (state is not { ReplacementMode: "normal" or "over" or "selected" or "blink" })
            return null;
        if (state.BitmapImages?.TryGetValue(state.ReplacementMode, out Image? bitmap) == true) return bitmap;
        Image? replacement = state.ReplacementImage is { Length: > 0 } filename
            ? GetBitmap(filename, symbols) : null;
        return replacement ?? GetRuntimeButtonImage(state, state.ReplacementMode, symbols);
    }

    private Image? GetRuntimeButtonImage(PangyaUiImageButtonState? state, string mode,
        IReadOnlySet<string> symbols)
    {
        if (state?.BitmapImages?.TryGetValue(mode, out Image? bitmap) == true) return bitmap;
        return state?.Images?.TryGetValue(mode, out string? filename) == true &&
            !string.IsNullOrWhiteSpace(filename) ? GetBitmap(filename, symbols) : null;
    }
    private void DrawButton(Graphics graphics, PangyaUiNode node, Rectangle bounds,
        PangyaUiRenderOptions options, bool rightGaugeButton = false)
    {
        PangyaUiImageButtonState? runtime = options.ImageButtonStates?.GetValueOrDefault(node);
        bool gaugeArrow = IsExtendedGauge(node);
        bool enabled = (runtime?.Enabled ?? options.DisabledNodes?.Contains(node) != true) &&
            (!gaugeArrow || options.DisabledNodes?.Contains(node) != true);
        PangyaUiButtonState state = runtime?.Status ?? options.ButtonState;
        if (!enabled && runtime?.Status is null && state == PangyaUiButtonState.Hover)
            state = PangyaUiButtonState.Normal;
        if (bounds.Width <= 0 || bounds.Height <= 0)
        {
            DrawOwner();
            return;
        }
        int style = runtime?.Style ?? (node.Parameters.TryGetValue("style", out string? value) ? ReadDecimalPrefix(value) : 1);
        string mode = style == 2 ? state switch
        {
            PangyaUiButtonState.Hover => "over",
            PangyaUiButtonState.Selected => "selected",
            _ => "normal"
        } : "normal";
        Color tint = style == 1 ? state switch
        {
            PangyaUiButtonState.Hover => Color.FromArgb(224, 224, 224),
            PangyaUiButtonState.Selected => Color.FromArgb(176, 176, 176),
            _ => Color.White
        } : Color.White;
        double elapsed = runtime?.SecondsSincePress ?? double.PositiveInfinity;
        if (options.ButtonPressTicks?.TryGetValue(node, out long pressedAt) == true)
            elapsed = (options.CurrentTick - pressedAt) / 1000.0;
        double pushDelay = gaugeArrow ? 0 : runtime?.PushDelay ?? GetButtonPushDelay(node);
        if (style is >= 1 and <= 3 && ((!enabled && style is 1 or 2) || elapsed <= pushDelay))
            tint = Color.FromArgb(80, 176, 176, 176);
        Image? replacement = !gaugeArrow && HasButtonSkin(node, options) ? GetReplacementButtonImage(runtime, Symbols(options)) : null;
        bool blinkPhase = runtime?.Blink ?? options.LayoutAnimationTime.TotalMilliseconds % 1400 >= 700;
        if (style is < 1 or > 3) tint = Color.Transparent;
        if (!IsWindowViewFocused(node, options)) tint = Color.FromArgb(tint.A / 2, tint);
        double alphaModifier = GetWindowAlphaScale(node, options);
        if (gaugeArrow)
        {
            float primary = options.GaugeButtonAlpha?.GetValueOrDefault((node, rightGaugeButton), 1) ?? 1;
            float secondary = options.GaugeButtonAlpha2?.GetValueOrDefault((node, rightGaugeButton), 1) ?? 1;
            alphaModifier = (float.IsFinite(primary) ? Math.Clamp(primary, 0, 1) : 1) *
                (double)(float.IsFinite(secondary) ? Math.Clamp(secondary, 0, 1) : 1);
        }
        if (IsCombo(node))
        {
            float primary = options.ComboButtonAlpha?.GetValueOrDefault(node, 1) ?? 1;
            float secondary = options.ComboButtonAlpha2?.GetValueOrDefault(node, 1) ?? 1;
            alphaModifier = (float.IsFinite(primary) ? Math.Clamp(primary, 0, 1) : 1) *
                (double)(float.IsFinite(secondary) ? Math.Clamp(secondary, 0, 1) : 1);
        }
        if (!rightGaugeButton && blinkPhase)
        {
            bool hasBlinkImage = runtime?.Blink == true && runtime.ReplacementMode == "blink" &&
                replacement is not null;
            hasBlinkImage |= runtime?.Blink == true &&
                GetRuntimeButtonImage(runtime, "blink", Symbols(options)) is not null;
            if (hasBlinkImage ||
                (node.Parameters.GetValueOrDefault("blink") is { Length: > 0 } blink &&
                 GetBitmap(blink, Symbols(options)) is not null))
                mode = "blink";
            else if (runtime?.Blink == true)
                alphaModifier *= 0.5;
        }
        if (options.ButtonOpacity?.TryGetValue(node, out byte opacity) == true)
            alphaModifier *= opacity / 255.0;
        tint = Color.FromArgb((int)(tint.A * alphaModifier), tint);
        foreach (string key in rightGaugeButton ? new[] { "r" + mode } : new[] { "below_" + mode, mode })
        {
            if (!gaugeArrow && runtime?.BitmapImages?.TryGetValue(key, out Image? explicitBitmap) == true)
            {
                if (explicitBitmap is not null)
                    DrawTintedImage(graphics, explicitBitmap, bounds, 0, explicitBitmap.Width, explicitBitmap.Height, tint);
                continue;
            }
            Image? image = key == runtime?.ReplacementMode ? replacement : null;
            if (!gaugeArrow && image is null && !key.StartsWith("below_", StringComparison.Ordinal))
                image = GetRuntimeButtonImage(runtime, key, Symbols(options));
            if (key.StartsWith("below_", StringComparison.Ordinal) &&
                runtime?.BelowImages?.TryGetValue(mode, out string? below) == true)
            {
                if (below is null) continue;
                image = GetBitmap(below, Symbols(options));
            }
            if (image is null && node.Parameters.TryGetValue(key, out string? filename))
                image = GetBitmap(filename, Symbols(options));
            if (image is not null)
                DrawTintedImage(graphics, image, bounds, 0, image.Width, image.Height, tint);
        }

        DrawOwner();

        void DrawOwner()
        {
            if (!HasButtonSkin(node, options) || ReadDecimalPrefix(node.Parameters.GetValueOrDefault("drawstyle")) == 0 ||
                options.ButtonOwnerDraw?.GetValueOrDefault(node) is not { } drawOwner) return;
            GraphicsState ownerState = graphics.Save();
            try { drawOwner(graphics, bounds, state); }
            finally { graphics.Restore(ownerState); }
        }
    }

    public static PangyaUiRenderOptions EnableButton(PangyaUiNode node, bool enabled,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!node.Type.Equals("BUTTON", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The node must be a button.", nameof(node));
        options ??= new();
        var states = options.ImageButtonStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        PangyaUiImageButtonState state = states.GetValueOrDefault(node) ?? new();
        PangyaUiButtonState status = state.Status ?? options.ButtonState;
        states[node] = state with
        {
            Enabled = enabled,
            Status = status == PangyaUiButtonState.Hover ? PangyaUiButtonState.Normal : status
        };
        var disabled = options.DisabledNodes?.ToHashSet() ?? [];
        if (enabled) disabled.Remove(node);
        else disabled.Add(node);
        return options with { ImageButtonStates = states, DisabledNodes = disabled };
    }

    public static PangyaUiRenderOptions SetButtonPushDelay(PangyaUiNode node, double delay,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!node.Type.Equals("BUTTON", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The node must be a button.", nameof(node));
        if (!double.IsFinite(delay)) throw new ArgumentOutOfRangeException(nameof(delay));
        options ??= new();
        var states = options.ImageButtonStates?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var state = states.GetValueOrDefault(node) ?? new(Enabled: options.DisabledNodes?.Contains(node) != true);
        states[node] = state with { PushDelay = delay, SecondsSincePress = delay + 0.1 };
        var ticks = options.ButtonPressTicks?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        ticks.Remove(node);
        return options with { ImageButtonStates = states, ButtonPressTicks = ticks };
    }

    public static double GetButtonPushDelay(PangyaUiNode node, PangyaUiRenderOptions options) =>
        options.ImageButtonStates?.GetValueOrDefault(node)?.PushDelay ?? GetButtonPushDelay(node);

    private static readonly System.Text.RegularExpressions.Regex PushDelayPrefix = new(
        @"\A[ \t\r\n\f\v]*[+-]?(?:[0-9]+(?:\.[0-9]*)?|\.[0-9]+)(?:[eE][+-]?[0-9]+)?",
        System.Text.RegularExpressions.RegexOptions.CultureInvariant);

    public static double GetButtonPushDelay(PangyaUiNode node)
    {
        if (!node.Type.Equals("BUTTON", StringComparison.OrdinalIgnoreCase)) return 0;
        var prefix = PushDelayPrefix.Match(node.Parameters.GetValueOrDefault("pushdelay") ?? string.Empty);
        return double.TryParse(prefix.ValueSpan, NumberStyles.Float,
            CultureInfo.InvariantCulture, out double delay) && double.IsFinite(delay) && delay != 0
            ? delay : 1.2;
    }

    private static bool IsViewer(PangyaUiNode node) =>
        node.Type.Equals("VIEWER", StringComparison.OrdinalIgnoreCase);

    private Image? GetViewerImage(PangyaUiNode node, IReadOnlySet<string> symbols, PangyaUiRenderOptions? options = null)
        => GetViewerImage(node, symbols, options, out _);

    private Image? GetViewerImage(PangyaUiNode node, IReadOnlySet<string> symbols, PangyaUiRenderOptions? options,
        out string resolvedFilename)
    {
        string? filename = options?.ViewerImages?.GetValueOrDefault(node);
        if (!string.IsNullOrWhiteSpace(filename) && GetBitmap(filename, symbols) is Image image)
        {
            resolvedFilename = filename;
            return image;
        }
        if (node.Name == "picture" && node.Parent is { } room &&
            string.Equals(room.Name, "makeroom", StringComparison.OrdinalIgnoreCase))
        {
            string modeImage = IsMakeRoomMatch(room, options)
                ? "picture_match.tga" : "picture_stroke.tga";
            if (GetBitmap(modeImage, symbols) is Image picture)
            {
                resolvedFilename = modeImage;
                return picture;
            }
        }
        filename = node.Parameters.GetValueOrDefault("image");
        Image? authored = string.IsNullOrWhiteSpace(filename) ? null : GetBitmap(filename, symbols);
        resolvedFilename = authored is null ? string.Empty : filename!;
        return authored;
    }

    public bool TrySetAreaImage(PangyaUiNode node, string? filename, PangyaUiRenderOptions options,
        out PangyaUiRenderOptions updatedOptions)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (!IsArea(node)) throw new ArgumentException("The node must be an area.", nameof(node));
        updatedOptions = options;
        string previous = options.AreaImageNames?.GetValueOrDefault(node) ??
            options.AreaImages?.GetValueOrDefault(node) ?? node.Parameters.GetValueOrDefault("bgimg") ?? string.Empty;
        if (filename is not null && string.Equals(previous, filename, StringComparison.OrdinalIgnoreCase))
            return true;
        var names = options.AreaImageNames?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var images = options.AreaImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var bitmaps = options.AreaBitmapImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        names[node] = filename ?? previous;
        images[node] = filename;
        bitmaps.Remove(node);
        updatedOptions = options with { AreaImageNames = names, AreaImages = images, AreaBitmapImages = bitmaps };
        return filename is null || GetBitmap(filename, Symbols(options)) is not null;
    }

    public static bool TrySetAreaBitmap(PangyaUiNode node, Image? image, PangyaUiRenderOptions options,
        out PangyaUiRenderOptions updatedOptions)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(options);
        if (!IsArea(node)) throw new ArgumentException("The node must be an area.", nameof(node));
        updatedOptions = options;
        if (image is null) return false;
        var bitmaps = options.AreaBitmapImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        bitmaps[node] = image;
        updatedOptions = options with { AreaBitmapImages = bitmaps };
        return true;
    }

    public static PangyaUiRenderOptions SetViewerMouseEvent(PangyaUiNode node, bool active,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsViewer(node)) throw new ArgumentException("The node must be a viewer.", nameof(node));
        options ??= new();
        var events = options.ViewerMouseEvents?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        events[node] = active;
        return options with { ViewerMouseEvents = events };
    }

    public static bool ViewerAcceptsMouse(PangyaUiNode node, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsViewer(node) || IsRuntimeHidden(node, options) || !IsRenderVisible(node, options) ||
            options?.DisabledNodes?.Contains(node) == true || !IsWindowViewFocused(node, options ?? new())) return false;
        if (options?.ViewerMouseEvents?.TryGetValue(node, out bool active) == true) return active;
        if (!node.Parameters.TryGetValue("mouse_event", out string? setting)) return true;
        // atoi accepts leading whitespace/sign and stops at the first non-digit.
        ReadOnlySpan<char> value = setting.AsSpan().TrimStart();
        if (!value.IsEmpty && value[0] is ('+' or '-')) value = value[1..];
        foreach (char digit in value)
        {
            if (!char.IsAsciiDigit(digit)) break;
            if (digit != '0') return true;
        }
        return false;
    }

    public bool TryOpenViewer(PangyaUiNode node, string filename, PangyaUiRenderOptions options,
        out PangyaUiRenderOptions updatedOptions)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentNullException.ThrowIfNull(filename);
        ArgumentNullException.ThrowIfNull(options);
        if (!IsViewer(node)) throw new ArgumentException("The node must be a viewer.", nameof(node));
        updatedOptions = options;
        GetViewerImage(node, Symbols(options), options, out string current);
        if (string.Equals(current, filename, StringComparison.Ordinal)) return true;
        if (string.IsNullOrEmpty(filename) || GetBitmap(filename, Symbols(options)) is not Image image) return false;
        var images = options.ViewerImages?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var rows = options.ViewerTopRows?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        var fractionalRows = options.ViewerScrollRows?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        images[node] = filename;
        int top = options.ViewerFollowBottom?.GetValueOrDefault(node) == true
            ? GetViewerScrollRange(node, image, options) : 0;
        rows[node] = top;
        fractionalRows[node] = top;
        updatedOptions = options with
        {
            ViewerImages = images, ViewerTopRows = rows, ViewerScrollRows = fractionalRows,
            DraggedViewerThumb = options.DraggedViewerThumb is { } drag && ReferenceEquals(drag.Node, node)
                ? null : options.DraggedViewerThumb
        };
        return true;
    }

    public int GetViewerMaximumTopRow(PangyaUiNode node, IReadOnlySet<string>? symbols = null,
        PangyaUiRenderOptions? options = null) =>
        IsViewer(node) && !IsRuntimeHidden(node, options)
            ? GetViewerScrollRange(node, GetViewerImage(node, symbols ?? Symbols(options ?? new()), options), options)
            : 0;

    private static int GetViewerScrollRange(PangyaUiNode node, Image? image,
        PangyaUiRenderOptions? options) =>
        node.Parameters.GetValueOrDefault("use_wheel") == "true" &&
        image is not null
            ? Math.Max(0, image.Height / ViewerRowHeight - GetViewerScrollbarCapacity(node, options))
            : 0;

    public static int GetViewerScrollbarCapacity(PangyaUiNode node, PangyaUiRenderOptions? options) =>
        options?.RetainedViewerScrollbarCapacities?.TryGetValue(node, out int retained) == true
            ? retained : Math.Max(0, GetWindowBounds(node, options).Height) / ViewerRowHeight;
    private static float GetViewerScrollRow(PangyaUiNode node, PangyaUiRenderOptions options) =>
        options.ViewerScrollRows?.TryGetValue(node, out float row) == true && float.IsFinite(row)
            ? row : options.ViewerTopRows?.GetValueOrDefault(node) ?? 0;

    private Rectangle DrawViewer(Graphics? graphics, PangyaUiNode node, Rectangle bounds,
        PangyaUiRenderOptions options)
    {
        Action<Graphics, Rectangle>? drawOwner = options.ViewerOwnerDraw?.GetValueOrDefault(node);
        Rectangle? measured = null;
        Image? image = GetViewerImage(node, Symbols(options), options);
        float row = GetViewerScrollRow(node, options);
        byte opacity = GetScrollbarOpacity(node, options,
            options.ViewerOpacity?.GetValueOrDefault(node, byte.MaxValue) ?? byte.MaxValue);
        if (image is not null && bounds.Width > 0 && bounds.Height > 0)
        {
            float sourceY = Math.Clamp(row, 0, GetViewerScrollRange(node, image, options)) * ViewerRowHeight;
            int width = Math.Min(bounds.Width, image.Width);
            float height = Math.Min(bounds.Height, image.Height - sourceY);
            node.Parameters.TryGetValue("bgcolor", out string? color);
            Color tint = options.DisabledNodes?.Contains(node) == true
                ? Color.FromArgb(80, 176, 176, 176) : ParseColor(color, Color.White, implicitAlpha: false);
            double imageAlpha = GetWindowAlphaScale(node, options, secondary: false) *
                (options.ViewerOpacity?.GetValueOrDefault(node, byte.MaxValue) ?? byte.MaxValue) / byte.MaxValue;
            tint = Color.FromArgb((int)(tint.A * imageAlpha), tint);
            if (tint.A > 0)
                IncludeBounds(ref measured, new Rectangle(bounds.X, bounds.Y, width, (int)MathF.Ceiling(height)));
            if (graphics is not null && height > 0 && tint.A > 0)
            {
                using ImageAttributes attributes = CreateTintAttributes(tint);
                PointF[] corners = [bounds.Location, new(bounds.X + (float)width, bounds.Y),
                    new(bounds.X, bounds.Y + height)];
                GraphicsState state = graphics.Save();
                try
                {
                    graphics.PixelOffsetMode = PixelOffsetMode.Half;
                    graphics.DrawImage(image, corners, new RectangleF(0, sourceY, width, height),
                        GraphicsUnit.Pixel, attributes);
                }
                finally { graphics.Restore(state); }
            }
        }

        if (drawOwner is not null)
        {
            IncludeBounds(ref measured, bounds);
            DrawOwner();
        }
        if (node.Parameters.GetValueOrDefault("use_wheel") == "true" &&
            options.HiddenViewerScrollbars?.Contains(node) != true)
            IncludeBounds(ref measured, DrawScrollBar(graphics, bounds, GetViewerScrollbarCapacity(node, options),
                (image?.Height ?? 0) / ViewerRowHeight, row, Symbols(options),
                showGuide: options.ScrollbarGuides?.GetValueOrDefault(node, true) ?? true,
                focused: ReferenceEquals(options.WheelFocusedNode ?? options.WheelFocusedViewer, node) &&
                    IsWindowWheelEventEnabled(node, options), opacity: opacity, measure: true,
                thumbOffset: GetViewerThumbOffset(node, options)));
        return measured ?? Rectangle.Empty;

        void DrawOwner()
        {
            if (graphics is null || drawOwner is null) return;
            GraphicsState state = graphics.Save();
            try { drawOwner(graphics, bounds); }
            finally { graphics.Restore(state); }
        }
    }

    public Rectangle GetViewerViewport(PangyaUiNode node) =>
        GetViewerViewport(node, null);

    public Rectangle GetViewerViewport(PangyaUiNode node, PangyaUiRenderOptions? options) =>
        IsViewer(node) && IsRenderVisible(node, options) && !IsRuntimeHidden(node, options)
            ? GetAbsoluteBounds(node, options) : Rectangle.Empty;

    public PangyaUiNode? HitTestViewerWheel(PangyaUiDocument document, PangyaUiNode? selectedForm,
        Point point, PangyaUiRenderOptions? options = null)
    {
        options ??= new();
        return GetRenderOrder(document, selectedForm, options.SelectedNode, options.EnabledSymbols,
                options.TopmostNodes, options).LastOrDefault(node =>
            IsRenderVisible(node, options) && IsWindowWheelEventEnabled(node, options) &&
            GetViewerScrollbarCapacity(node, options) > 0 &&
            GetViewerViewport(node, options).Contains(point) &&
            GetViewerMaximumTopRow(node, options: options) > 0);
    }

    public PangyaUiNode? HitTestWheelTarget(PangyaUiDocument document, PangyaUiNode? selectedForm,
        Point point, PangyaUiRenderOptions options)
    {
        IReadOnlyList<PangyaUiNode> order = GetRenderOrder(document, selectedForm, null,
            options.EnabledSymbols, options.TopmostNodes, options);
        var visible = order.ToHashSet();
        IReadOnlySet<PangyaUiNode>? topmost = options.TopmostNodes;
        if (TryResolve(TopmostWindows(), out PangyaUiNode? target)) return target;
        IReadOnlyList<PangyaUiNode> initialized = topmost is { Count: > 0 }
            ? GetRenderOrder(document, selectedForm, null, options.EnabledSymbols, null, options) : order;
        for (int index = initialized.Count - 1; index >= 0; index--)
        {
            PangyaUiNode node = initialized[index];
            if (options.Combos?.GetValueOrDefault(node) is { IsOpen: true } combo &&
                IsWindowWheelEventEnabled(node, options) && GetComboMaximumTopRow(node, combo) > 0 &&
                (GetComboDropdownBounds(node, options).Contains(point) ||
                 TryGetComboScrollbarGeometry(node, point, options, out _, out _, out _))) return node;
        }
        return selectedForm is not null && topmost?.Contains(selectedForm) != true &&
            TryResolve(Visit(selectedForm), out target) ? target : null;

        bool TryResolve(IEnumerable<PangyaUiNode> windows, out PangyaUiNode? resolved)
        {
            resolved = null;
            foreach (PangyaUiNode node in windows)
            {
                if (IsRuntimeHidden(node, options) || !IsRenderVisible(node, options)) continue;
                // A form without a scrollable child claims wheel focus through the desktop scrollbar.
                if (node.IsForm && !node.IsLayout && GetAbsoluteBounds(node, options).Contains(point)) return true;
                if (!IsWindowWheelEventEnabled(node, options)) continue;
                if (IsViewer(node) && GetViewerScrollbarCapacity(node, options) > 0 &&
                    GetViewerViewport(node, options).Contains(point) &&
                    GetViewerMaximumTopRow(node, options: options) > 0)
                {
                    resolved = node;
                    return true;
                }
                if (!IsMultilineEdit(node) || options.EditLines?.GetValueOrDefault(node) is not { } lines ||
                    !GetAbsoluteBounds(node, options).Contains(point)) continue;
                int capacity = GetEditRowCapacity(node, options);
                if (capacity > 0 && lines.Count > capacity)
                {
                    resolved = node;
                    return true;
                }
            }
            return false;
        }

        IEnumerable<PangyaUiNode> TopmostWindows()
        {
            if (topmost is not null)
                for (int index = order.Count - 1; index >= 0; index--)
                    if (topmost.Contains(order[index]))
                        foreach (PangyaUiNode node in Visit(order[index])) yield return node;
        }

        IEnumerable<PangyaUiNode> Visit(PangyaUiNode window)
        {
            if (!visible.Contains(window)) yield break;
            for (int index = window.Children.Count - 1; index >= 0; index--)
            {
                PangyaUiNode child = window.Children[index];
                if (topmost?.Contains(child) == true) continue;
                foreach (PangyaUiNode node in Visit(child)) yield return node;
            }
            yield return window;
        }
    }

    private static float? GetViewerThumbOffset(PangyaUiNode node, PangyaUiRenderOptions options) =>
        options.DraggedViewerThumb is { } drag && ReferenceEquals(drag.Node, node) ? drag.Offset : null;

    public Rectangle GetViewerScrollThumb(PangyaUiNode node, PangyaUiRenderOptions options) =>
        EncloseBounds(GetViewerScrollThumbGeometry(node, options));

    public RectangleF GetViewerScrollThumbGeometry(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        IReadOnlySet<string> symbols = Symbols(options);
        if (IsRuntimeHidden(node, options) || options.HiddenViewerScrollbars?.Contains(node) == true ||
            !IsViewer(node) || GetViewerImage(node, symbols, options) is not Image image ||
            GetViewerScrollRange(node, image, options) <= 0)
            return RectangleF.Empty;
        Rectangle bounds = GetViewerViewport(node, options);
        if (bounds.Width <= 0 || bounds.Height <= 0) return RectangleF.Empty;
        DrawScrollBarCore(null, bounds, GetViewerScrollbarCapacity(node, options),
            image.Height / ViewerRowHeight, GetViewerScrollRow(node, options),
            symbols, false, out RectangleF thumb, thumbOffset: GetViewerThumbOffset(node, options));
        return thumb;
    }

    public bool TryScrollViewerWheel(PangyaUiNode node, int delta, PangyaUiRenderOptions options,
        out PangyaUiRenderOptions updated)
    {
        updated = options;
        if (!IsWindowWheelEventEnabled(node, options) || RestoreViewerScroll(node, options) is not { } scroll ||
            !scroll.ApplyWheel(delta)) return false;
        updated = scroll.ApplyToViewer(node, options);
        return true;
    }

    public PangyaUiRenderOptions PageViewerScrollbar(PangyaUiNode node, bool down, PangyaUiRenderOptions options)
    {
        if (RestoreViewerScroll(node, options) is not { RowCapacity: > 0 } scroll) return options;
        if (down) scroll.ScrollDown();
        else scroll.ScrollUp();
        return scroll.ApplyToViewer(node, options);
    }

    private PangyaUiScrollState? RestoreViewerScroll(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        if (!IsViewer(node) || !IsRenderVisible(node, options) || IsRuntimeHidden(node, options) ||
            GetViewerImage(node, Symbols(options), options) is not Image image) return null;
        int maximum = GetViewerScrollRange(node, image, options);
        if (maximum <= 0) return null;
        var scroll = PangyaUiScrollState.RestoreRows(
            GetViewerScrollbarCapacity(node, options), image.Height / ViewerRowHeight,
            Math.Clamp(GetViewerScrollRow(node, options), 0, maximum),
            options.ViewerFollowBottom?.GetValueOrDefault(node) == true);
        scroll.ShowGuide = options.ScrollbarGuides?.GetValueOrDefault(node, true) ?? true;
        return scroll;
    }

    private Rectangle DrawScrollBar(Graphics? graphics, Rectangle bounds, int capacity, int rows,
        float topRow, IReadOnlySet<string> symbols, bool showGuide, bool focused = false, byte opacity = byte.MaxValue,
        bool measure = false, bool atLeft = false, float? thumbOffset = null, bool retainTopRow = false) =>
        DrawScrollBarCore(graphics, bounds, capacity, rows, topRow, symbols, showGuide,
            out _, focused, opacity, measure, atLeft, thumbOffset, retainTopRow);

    private Rectangle DrawScrollBarCore(Graphics? graphics, Rectangle bounds, int capacity, int rows,
        float topRow, IReadOnlySet<string> symbols, bool showGuide, out RectangleF thumb, bool focused = false,
        byte opacity = byte.MaxValue, bool measure = false, bool atLeft = false, float? thumbOffset = null,
        bool retainTopRow = false)
    {
        thumb = RectangleF.Empty;
        if (capacity <= 0 || (rows <= capacity && !showGuide)) return Rectangle.Empty;
        Image? top = GetBitmap("scroll_n_1", symbols);
        Image? middle = GetBitmap("scroll_n_2", symbols);
        Image? bottom = GetBitmap("scroll_n_3", symbols);
        if (top is null || middle is null || bottom is null) return Rectangle.Empty;

        Rectangle? measured = null;
        int x = ClampCoordinate((long)bounds.X + (atLeft ? 0 : bounds.Width - top.Width) + 2);
        float thumbHeight = bounds.Height;
        float thumbY = bounds.Y;
        if (rows > capacity)
        {
            Image? trackTop = GetBitmap("scrollbg1", symbols);
            Image? trackMiddle = GetBitmap("scrollbg2", symbols);
            Image? trackBottom = GetBitmap("scrollbg3", symbols);
            if (trackTop is not null && trackMiddle is not null && trackBottom is not null &&
                (graphics is not null || measure))
            {
                int width = trackTop.Width;
                float middleHeight = bounds.Height - (float)trackTop.Height - trackMiddle.Height;
                DrawFragment(trackTop, new RectangleF(x, bounds.Y, width, trackTop.Height));
                if (middleHeight > 0)
                    DrawFragment(trackMiddle, new RectangleF(x, bounds.Y + (float)trackTop.Height, width, middleHeight));
                DrawFragment(trackBottom, new RectangleF(x, bounds.Y + (float)bounds.Height - trackMiddle.Height,
                    width, trackBottom.Height));
            }

            long caps = (long)top.Height + bottom.Height;
            double exactThumbHeight = caps + (double)(bounds.Height - caps) * capacity / rows;
            thumbHeight = (float)exactThumbHeight;
            int maxRow = rows - capacity;
            double row = !float.IsFinite(topRow) ? 0 : Math.Clamp((double)topRow, 0,
                retainTopRow ? int.MaxValue : maxRow);
            double travel = (bounds.Height - exactThumbHeight) * row / maxRow;
            thumbY = (float)Math.Clamp(bounds.Y + Math.Truncate(travel + 0.5), int.MinValue, int.MaxValue);
            if (thumbOffset is float offset && float.IsFinite(offset) && thumbHeight <= bounds.Height)
                thumbY = bounds.Y + Math.Clamp(offset, 0, bounds.Height - thumbHeight);
            x = ClampCoordinate((long)x + 1);
        }
        if (focused)
        {
            Image focusedTop = GetBitmap("scroll_o_1", symbols) ?? top;
            Image focusedMiddle = GetBitmap("scroll_o_2", symbols) ?? middle;
            Image focusedBottom = GetBitmap("scroll_o_3", symbols) ?? bottom;
            DrawStrip(focusedTop, focusedMiddle, focusedBottom, x, thumbY, top.Width,
                thumbHeight, top.Height, bottom.Height);
        }
        else DrawStrip(top, middle, bottom, x, thumbY, top.Width, thumbHeight);
        thumb = new RectangleF(x, thumbY, top.Width, thumbHeight);
        return measure ? measured ?? Rectangle.Empty : EncloseBounds(thumb);

        void DrawStrip(Image first, Image center, Image last, int left, float y, int width, float height,
            int? firstCap = null, int? lastCap = null)
        {
            if (graphics is null && !measure) return;
            float firstHeight = firstCap ?? first.Height;
            float lastHeight = lastCap ?? last.Height;
            // Keep the cap dimensions and position the bottom cap using the signed center span.
            float centerHeight = height - firstHeight - lastHeight;
            if (firstHeight > 0) DrawFragment(first, new RectangleF(left, y, width, firstHeight));
            if (centerHeight > 0) DrawFragment(center,
                new RectangleF(left, (float)Math.Clamp((double)y + firstHeight, int.MinValue, int.MaxValue), width, centerHeight));
            if (lastHeight > 0) DrawFragment(last,
                new RectangleF(left, (float)Math.Clamp((double)y + firstHeight + centerHeight, int.MinValue, int.MaxValue), width, lastHeight));
        }

        void DrawFragment(Image image, RectangleF destination)
        {
            if (opacity == 0) return;
            if (measure)
                IncludeBounds(ref measured, EncloseBounds(destination));
            if (graphics is null) return;
            GraphicsState state = graphics.Save();
            try
            {
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                if (opacity == byte.MaxValue)
                {
                    DrawGraphicsImage(graphics, image, destination);
                    return;
                }
                using var attributes = new ImageAttributes();
                attributes.SetColorMatrix(new ColorMatrix
                {
                    Matrix33 = (int)(opacity * GetGraphicsAlpha(graphics)) / 255f
                });
                PointF[] corners = [destination.Location, new(destination.Right, destination.Top),
                    new(destination.Left, destination.Bottom)];
                graphics.DrawImage(image, corners, new RectangleF(0, 0, image.Width, image.Height),
                    GraphicsUnit.Pixel, attributes);
            }
            finally { graphics.Restore(state); }
        }
    }

    private static Rectangle EncloseBounds(RectangleF bounds) => Rectangle.FromLTRB(
        ClampCoordinate((long)MathF.Floor(bounds.Left)),
        ClampCoordinate((long)MathF.Floor(bounds.Top)),
        ClampCoordinate((long)MathF.Ceiling(bounds.Right)),
        ClampCoordinate((long)MathF.Ceiling(bounds.Bottom)));

    private static void DrawTintedImage(Graphics graphics, Image image, Rectangle destination,
        float sourceY, float width, float height, Color tint, float sourceX = 0, bool useGraphicsAlpha = true)
    {
        if (useGraphicsAlpha) tint = Color.FromArgb((int)(tint.A * GetGraphicsAlpha(graphics)), tint);
        using ImageAttributes attributes = CreateTintAttributes(tint);
        graphics.DrawImage(image, destination,
            sourceX, sourceY, width, height, GraphicsUnit.Pixel, attributes);
    }

    private static ImageAttributes CreateTintAttributes(Color tint)
    {
        var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix
        {
            Matrix00 = tint.R / 255f,
            Matrix11 = tint.G / 255f,
            Matrix22 = tint.B / 255f,
            Matrix33 = tint.A / 255f
        });
        return attributes;
    }

    private static void DrawGraphicsImage(Graphics graphics, Image image, Rectangle destination)
    {
        if (GetGraphicsAlpha(graphics) == 1) graphics.DrawImage(image, destination);
        else DrawTintedImage(graphics, image, destination, 0, image.Width, image.Height, Color.White);
    }

    private static void DrawGraphicsImage(Graphics graphics, Image image, PointF[] destination, RectangleF source)
    {
        if (GetGraphicsAlpha(graphics) == 1)
        {
            graphics.DrawImage(image, destination, source, GraphicsUnit.Pixel);
            return;
        }
        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix { Matrix33 = (int)(255 * GetGraphicsAlpha(graphics)) / 255f });
        graphics.DrawImage(image, destination, source, GraphicsUnit.Pixel, attributes);
    }

    private static void DrawGraphicsImage(Graphics graphics, Image image, RectangleF destination)
    {
        if (GetGraphicsAlpha(graphics) == 1) graphics.DrawImage(image, destination);
        else DrawGraphicsImage(graphics, image,
            [destination.Location, new PointF(destination.Right, destination.Top),
                new PointF(destination.Left, destination.Bottom)],
            new RectangleF(0, 0, image.Width, image.Height));
    }

    private static Color ParseColor(string? value, Color fallback, bool implicitAlpha = true)
    {
        ReadOnlySpan<char> hex = ReadHexPrefix(value);
        if ((implicitAlpha && hex.Length != 6 && hex.Length != 8) ||
            !uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint color))
            return fallback;
        return Color.FromArgb(unchecked((int)(implicitAlpha && hex.Length == 6 ? color | 0xff000000 : color)));
    }

    public bool CanPreviewFrameBlink(PangyaUiNode node, IReadOnlySet<string> symbols)
    {
        PangyaUiNode? frame = ResolvePreviewReference(node, PangyaUiButtonState.Normal, symbols);
        if (frame?.IsForm == true && !frame.IsLayout)
            frame = _resources.TryResolve(frame.GetResource(PangyaUiButtonState.Normal), "FRAME", symbols);
        XmlElement? layer = frame?.Element.ChildNodes.OfType<XmlElement>()
            .FirstOrDefault(child => child.LocalName.Equals("layer", StringComparison.OrdinalIgnoreCase));
        return frame?.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) == true &&
            ReadLayerValue(layer, "type", 0) == 3 &&
            frame.Element["cfrm"]?.GetAttribute("filename") is { Length: > 0 } prefix &&
            GetBitmap(prefix + "00", symbols) is not null;
    }

    private static void DrawDesktopWallpaper(Graphics graphics, Image image, Rectangle bounds,
        Color tint)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0 || tint.A == 0) return;
        GraphicsState state = graphics.Save();
        try
        {
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            if (tint.ToArgb() == Color.White.ToArgb())
                graphics.DrawImage(image, bounds, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
            else
                DrawTintedImage(graphics, image, bounds, 0, image.Width, image.Height, tint, useGraphicsAlpha: false);
        }
        finally
        {
            graphics.Restore(state);
        }
    }

    private Image? GetStartupWallpaper(PangyaUiRenderOptions options, IReadOnlySet<string> symbols) =>
        options.StartupWallpaperImage ?? (options.StartupWallpaper is { Length: > 0 } name
            ? Path.IsPathFullyQualified(name) ? _images.GetImageByPath(name) : GetBitmap(name, symbols)
            : null);

    private Image? GetBitmap(string name, IReadOnlySet<string> symbols)
    {
        if (string.IsNullOrWhiteSpace(name)) return null;
        string? alias = GetBitmapFilename(_resources.TryResolve(name, "BITMAP", symbols));
        return _images.GetImage(string.IsNullOrWhiteSpace(alias) ? name : alias);
    }

    private static string? GetBitmapFilename(PangyaUiNode? definition) =>
        definition?.Type.Equals("BITMAP", StringComparison.OrdinalIgnoreCase) == true
            ? definition.Element.GetAttribute("filename") : null;

    private static bool IsFrameBlinkVisible(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        if (options.FrameBlinkElapsed?.TryGetValue(node, out TimeSpan elapsed) == true)
            return Math.Max(0, elapsed.Ticks) % (1600 * TimeSpan.TicksPerMillisecond) <
                800 * TimeSpan.TicksPerMillisecond;
        return options.ShowFrameBlink;
    }

    public PangyaUiNode? ResolvePreviewReference(PangyaUiNode node, PangyaUiButtonState state,
        IReadOnlySet<string> symbols) => _resources.TryResolve(GetRenderResource(node, state),
            GetPreviewReferenceType(node), symbols);

    public PangyaUiNode? ResolvePreviewReference(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        IReadOnlySet<string> symbols = Symbols(options);
        if (IsArea(node))
        {
            if (options.AreaBitmapImages?.GetValueOrDefault(node) is not null) return null;
            string resource = GetRenderResource(node, options.ButtonState, options.AreaImages);
            return _resources.TryResolve(resource,
                options.AreaImages?.ContainsKey(node) == true ? "BITMAP" : node.Type, symbols);
        }
        if (!IsViewer(node)) return ResolvePreviewReference(node, options.ButtonState, symbols);
        GetViewerImage(node, symbols, options, out string filename);
        return _resources.TryResolve(filename.Length > 0 ? filename : node.GetResource(options.ButtonState), "BITMAP", symbols);
    }

    private static string GetPreviewReferenceType(PangyaUiNode node) =>
        IsViewer(node) ? "BITMAP" :
        IsContextMenu(node) || (node.IsForm && !node.IsLayout && node.Element.LocalName.Equals("element", StringComparison.OrdinalIgnoreCase))
            ? "FRAME" : node.Type;

    private static string GetRenderResource(PangyaUiNode node, PangyaUiButtonState state,
        IReadOnlyDictionary<PangyaUiNode, string?>? areaImages = null)
    {
        if (node.Type.Equals("STATIC", StringComparison.OrdinalIgnoreCase))
            return string.Empty;
        if (IsArea(node) && areaImages?.TryGetValue(node, out string? image) == true)
            return image ?? string.Empty;
        if ((node.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) ||
             node.Type.Equals("CONTEXTMENU", StringComparison.OrdinalIgnoreCase)) &&
            node.Parameters.TryGetValue("resource", out string? resource) &&
            !string.IsNullOrWhiteSpace(resource))
            return resource;
        if (IsArea(node))
        {
            if (node.Name == "text" && string.Equals(node.Parent?.Name, "makeroom", StringComparison.OrdinalIgnoreCase))
                return "text_vs";
            if (node.Name == "text_map" && node.Parent?.Name == "map_ctrl" &&
                string.Equals(node.Parent.Parent?.Name, "makeroom", StringComparison.OrdinalIgnoreCase))
                return "text_vs_map";
        }
        return node.GetResource(state);
    }

    private Rectangle DrawFrame(Graphics? graphics, Rectangle bounds, PangyaUiNode definition, IReadOnlySet<string> symbols, bool showBlink,
        string? baseImageOverride = null, PangyaUiNode? textSource = null, bool isViewFocused = true,
        bool contextMenu = false, bool hideDescription = false, PangyaUiRenderOptions? textOptions = null,
        PangyaUiNode? frameInstance = null, bool useAuthoredBaseLayer = false)
    {
        XmlElement[] layers = definition.Element.ChildNodes.OfType<XmlElement>()
            .Where(child => child.LocalName.Equals("layer", StringComparison.OrdinalIgnoreCase))
            .Take(8)
            .ToArray();
        Rectangle? measured = bounds;
        PangyaUiNode? instance = frameInstance ?? textSource;
        PangyaUiFrameText? frameText = instance is null ? null : textOptions?.FrameText?.GetValueOrDefault(instance);
        double opacity = contextMenu && instance is not null
            ? textOptions?.MenuStates?.GetValueOrDefault(instance)?.Opacity ?? byte.MaxValue
            : frameText?.Opacity ?? byte.MaxValue;
        if (instance is not null && textOptions is not null)
        {
            if (instance.IsForm && !instance.IsLayout && !contextMenu)
            {
                float primary = textOptions.FormFrameAlpha?.GetValueOrDefault(instance, 1) ?? 1;
                float secondary = textOptions.FormFrameAlpha2?.GetValueOrDefault(instance, 1) ?? 1;
                opacity *= (float.IsFinite(primary) ? Math.Clamp(primary, 0, 1) : 1) *
                    (double)(float.IsFinite(secondary) ? Math.Clamp(secondary, 0, 1) : 1);
            }
            else opacity *= GetWindowAlphaScale(instance, textOptions, secondary: !contextMenu);
        }
        XmlElement? baseLayer = layers.FirstOrDefault();
        int baseType = ReadLayerValue(baseLayer, "type", 0);
        showBlink = showBlink && !contextMenu && baseType == 3 &&
            definition.Element["cfrm"]?.GetAttribute("filename") is { Length: > 0 } blinkPrefix &&
            GetBitmap(blinkPrefix + "00", symbols) is not null;
        bool dim = !(isViewFocused && (contextMenu || frameText?.UseCaptionFocus != false)) && !showBlink;
        bool baseVisible = baseLayer is not null && ReadLayerValue(baseLayer, "invisible", 0) == 0;
        bool alignBaseBottom = false;
        FrameSliceGeometry? sliceGeometry = null;
        int fallbackHeight = 0;
        Rectangle baseBounds = useAuthoredBaseLayer && baseLayer is not null
            ? GetFrameLayerBounds(bounds, baseLayer, out alignBaseBottom) : bounds;
        Rectangle baseGeometry = baseLayer is null ? Rectangle.Empty :
            DrawFrameLayer(baseVisible ? graphics : null, baseBounds, definition, symbols, "bfrm", ref sliceGeometry, out fallbackHeight,
            baseType, alignBaseBottom, showBlink: showBlink, imageOverride: baseImageOverride,
            dim: dim, opacity: opacity, slicedOnly: contextMenu);
        if (baseVisible) IncludeBounds(ref measured, baseGeometry);
        int descriptionHeight = sliceGeometry is { } initializedBase
            ? ClampCoordinate((long)initializedBase.Top + initializedBase.Center + initializedBase.Bottom)
            : fallbackHeight;
        bool useDescriptionEmoticons = frameText?.UseDescriptionEmoticons != false;
        if (graphics is null && textSource is not null && textOptions?.IncludeCaptionVisualBounds == true)
        {
            var bitmapText = textOptions.DrawCaptionText?.Target as IPangyaUiBitmapTextPasses;
            if ((int)opacity > 0)
            {
                string caption = FrameCaption();
                PointF captionOrigin = FrameCaptionOrigin(bounds);
                IncludeBounds(ref measured, MeasureFrameText(caption, captionOrigin, textOptions, bitmapText, style: 2));
                IncludeBounds(ref measured, MeasureCaptionArtwork(bitmapText, caption, captionOrigin, style: 2));
            }
            if (!hideDescription && (useDescriptionEmoticons || (int)opacity > 0))
            {
                PointF origin = DescriptionOrigin();
                foreach (string line in FrameDescription().Replace("\r\n", "\n").Split('\n'))
                {
                    float lineHeight = textOptions.CaptionFontHeight ?? 12;
                    Rectangle footerBounds = bitmapText is null
                        ? MeasureFrameText(line, origin, textOptions, bitmapText: null, style: 0)
                        : MeasureCaptionLineArtwork(bitmapText, line, origin, style: 0, lineHeight,
                            useDescriptionEmoticons ? symbols : null,
                            textOptions.EmoticonAnimationTime, includeGlyphs: true);
                    IncludeBounds(ref measured, footerBounds);
                    origin.Y += lineHeight;
                }
            }
        }
        if (graphics is not null && textSource is not null)
        {
            using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
            using var title = new SolidBrush(Color.FromArgb((int)opacity, Color.White));
            using var outline = new SolidBrush(Color.FromArgb((int)opacity, Color.Gray));
            using var descriptionBrush = new SolidBrush(Color.FromArgb(useDescriptionEmoticons
                ? byte.MaxValue : (int)opacity, 64, 64, 64));
            string caption = FrameCaption();
            string description = FrameDescription();
            if (caption.Length > 0)
                DrawStyledText(graphics, caption, title, outline,
                    FrameCaptionOrigin(bounds), format, 2, drawText: textOptions?.DrawCaptionText);
            if (!hideDescription && description.Length > 0)
                DrawTaggedCaption(graphics, description, descriptionBrush, title,
                    DescriptionOrigin(), format, 0,
                    emoticonSymbols: useDescriptionEmoticons ? symbols : null,
                    animationElapsed: textOptions?.EmoticonAnimationTime,
                    lineHeight: textOptions?.CaptionFontHeight, measureText: textOptions?.MeasureCaptionText,
                    drawText: textOptions?.DrawCaptionText, parseTags: false,
                    forceOpaqueText: useDescriptionEmoticons);
        }
        foreach (XmlElement layer in baseType == 0 ? layers.Skip(1) : [])
        {
            bool visible = ReadLayerValue(layer, "invisible", 0) == 0;
            Rectangle layerBounds = GetFrameLayerBounds(bounds, layer, out bool alignBottom);
            Rectangle layerGeometry = DrawFrameLayer(visible ? graphics : null, layerBounds, definition, symbols,
                "sfrm", ref sliceGeometry, out _,
                ReadLayerValue(layer, "type", 0), alignBottom, showBlink: showBlink,
                imageOverride: contextMenu ? "sfrm" : null,
                dim: dim, opacity: opacity, slicedOnly: contextMenu);
            if (visible) IncludeBounds(ref measured, layerGeometry);
        }
        return measured ?? bounds;

        string FrameCaption() => frameText?.Caption ??
            TranslateXmlText(textSource?.Element.GetAttribute("caption") ?? string.Empty, textOptions);

        string FrameDescription() => frameText?.Description ?? TranslateXmlText(
            textSource?.IsForm == true
                ? textSource.Element.GetAttribute("description")
                : textSource?.Parameters.GetValueOrDefault("description") ?? string.Empty,
            textOptions);

        PointF DescriptionOrigin() => new(bounds.X + 10f,
            bounds.Y + (float)descriptionHeight - (frameText?.UseCaptionOffset == true ? 17f : 26f));
    }

    private static Rectangle GetFrameLayerBounds(Rectangle bounds, XmlElement layer, out bool alignBottom)
    {
        int height = ReadLayerValue(layer, "height", -1);
        int bottom = ReadLayerValue(layer, "bottompos", 0);
        long position = bottom != 0 ? -(long)bottom : ReadLayerValue(layer, "pos", 0);
        Rectangle layerBounds = bounds;
        if (height >= 0) layerBounds.Height = height;
        alignBottom = position < 0;
        layerBounds.Y = ClampCoordinate(alignBottom
            ? (long)bounds.Y + bounds.Height + position - layerBounds.Height
            : (long)bounds.Y + position);
        return layerBounds;
    }

    private static int ReadLayerValue(XmlElement? layer, string name, int fallback) =>
        layer?.GetAttributeNode(name) is { } attribute ? ReadDecimalPrefix(attribute.Value) : fallback;

    private Rectangle GetFormFrameBounds(PangyaUiNode form, Rectangle bounds, PangyaUiRenderOptions options,
        PangyaUiNode? frameInstance = null)
    {
        IReadOnlySet<string> symbols = Symbols(options);
        PangyaUiNode? frame = _resources.TryResolve(form.GetResource(options.ButtonState), "FRAME", symbols);
        return frame?.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) == true
            ? GetBaseFrameBounds(bounds, frame, symbols, imageOverride: null, form, options, frameInstance) : bounds;
    }

    private Rectangle GetInstantiatedFormBounds(PangyaUiNode form, Rectangle bounds,
        PangyaUiRenderOptions options, HashSet<PangyaUiNode> visited, Point parentOrigin = default)
    {
        Rectangle frameBounds = GetFormFrameBounds(form, bounds, options, options.ResourceInstance?.Host);
        if (!visited.Add(form)) return frameBounds;
        Rectangle? result = null;
        IncludeBounds(ref result, frameBounds);
        foreach (PangyaUiNode child in form.Children.Where(child => child.IsVisible(Symbols(options))))
        {
            Rectangle childBounds = GetReferencedBounds(child, options, visited, AddOrigins(parentOrigin, bounds.Location));
            if (childBounds.IsEmpty) continue;
            childBounds.X = ClampCoordinate((long)childBounds.X + bounds.X);
            childBounds.Y = ClampCoordinate((long)childBounds.Y + bounds.Y);
            IncludeBounds(ref result, childBounds);
        }
        visited.Remove(form);
        return result ?? bounds;
    }

    private static Rectangle GetInstantiatedFormRectangle(PangyaUiNode host, PangyaUiNode form,
        Rectangle bounds, PangyaUiRenderOptions options)
    {
        if (options.WindowSizes?.ContainsKey(host) != true)
        {
            if (PangyaUiDimensionHelper.ParseSize(form.Element.GetAttribute("size")) is Size authoredSize)
                bounds.Size = new Size(Math.Max(0, authoredSize.Width), Math.Max(0, authoredSize.Height));
            else if (form.Bounds.Size is { Width: > 0, Height: > 0 } formSize)
                bounds.Size = formSize;
        }
        return bounds;
    }

    private Rectangle GetBaseFrameBounds(Rectangle bounds, PangyaUiNode definition,
        IReadOnlySet<string> symbols, string? imageOverride, PangyaUiNode textSource, PangyaUiRenderOptions options,
        PangyaUiNode? frameInstance = null)
    {
        frameInstance ??= textSource;
        return DrawFrame(null, bounds, definition, symbols, IsFrameBlinkVisible(frameInstance, options), baseImageOverride: imageOverride,
            textSource: options.IncludeCaptionVisualBounds ? textSource : null,
            isViewFocused: IsWindowViewFocused(frameInstance, options),
            hideDescription: IsPrivacyHidden(frameInstance, options) || IsPrivacyHidden(textSource, options),
            textOptions: options, frameInstance: frameInstance);
    }

    private static PointF FrameCaptionOrigin(Rectangle bounds) => new(bounds.X + 10f, bounds.Y + 7f);

    private static Rectangle MeasureFrameText(string caption, PointF origin, PangyaUiRenderOptions options,
        IPangyaUiBitmapTextPasses? bitmapText, int style)
    {
        if (caption.Length == 0) return Rectangle.Empty;
        float? width = bitmapText?.GetDrawAdvance(caption) ?? options.MeasureCaptionText?.Invoke(caption);
        float? height = options.CaptionFontHeight;
        if (width is null || height is null)
        {
            using var bitmap = new Bitmap(1, 1);
            using Graphics graphics = Graphics.FromImage(bitmap);
            using var format = (StringFormat)StringFormat.GenericTypographic.Clone();
            width ??= graphics.MeasureString(caption, SystemFonts.DefaultFont, PointF.Empty, format).Width;
            height ??= SystemFonts.DefaultFont.GetHeight(graphics);
        }
        return MeasureGlyphExtent(origin, width.Value, height.Value, style);
    }

    private static Rectangle MeasureGlyphExtent(PointF origin, float width, float height, int style)
    {
        if (width <= 0 || height <= 0 || !float.IsFinite(width) || !float.IsFinite(height))
            return Rectangle.Empty;
        int edge = style == 2 ? 1 : 0;
        return Rectangle.FromLTRB(
            ClampCoordinate((long)MathF.Floor(origin.X) - edge), ClampCoordinate((long)MathF.Floor(origin.Y) - edge),
            ClampCoordinate((long)MathF.Ceiling(origin.X + width) + (style is 1 or 2 or 4 ? 1 : 0)),
            ClampCoordinate((long)MathF.Ceiling(origin.Y + height) + (style is 2 or 4 ? 1 : 0)));
    }

    public Rectangle? GetStandaloneFramePreviewBounds(Size previewSize, PangyaUiRenderOptions options) =>
        GetStandaloneFrame(options) is { } frame
            ? DrawFrame(null, new Rectangle(Point.Empty, previewSize), frame, Symbols(options),
                showBlink: false, useAuthoredBaseLayer: true)
            : null;

    private static PangyaUiNode? GetStandaloneFrame(PangyaUiRenderOptions options)
    {
        PangyaUiNode? node = options.SelectedNode;
        if (node?.Element.LocalName.Equals("layer", StringComparison.OrdinalIgnoreCase) == true)
            node = node.Parent;
        return node is { Parent: null } &&
            node.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) && node.IsVisible(Symbols(options)) &&
            IsRenderVisible(node, options) && !IsRuntimeHidden(node, options)
            ? node : null;
    }

    private readonly record struct FrameSliceGeometry(int Left, int Middle, int Right, int Top, int Center, int Bottom);

    private Rectangle DrawFrameLayer(Graphics? graphics, Rectangle bounds, PangyaUiNode definition,
        IReadOnlySet<string> symbols, string tag, ref FrameSliceGeometry? previousGeometry, out int fallbackHeight,
        int type, bool alignBottom = false, bool showBlink = false,
        string? imageOverride = null, bool dim = false, double opacity = byte.MaxValue, bool slicedOnly = false)
    {
        fallbackHeight = 0;
        Color tint = ParseColor(definition.Element.GetAttribute("color"), Color.White, implicitAlpha: false);
        tint = Color.FromArgb((int)(tint.A * opacity / byte.MaxValue), tint);
        if (dim)
        {
            int argb = tint.ToArgb();
            tint = Color.FromArgb((argb & unchecked((int)0xff000000)) | ((argb & 0xffffff) * 4 / 5));
        }

        void DrawSlice(Image image, Rectangle destination)
        {
            if (graphics is null || tint.A == 0) return;
            GraphicsState state = graphics.Save();
            try
            {
                // Keep nearest-neighbor samples inside each slice at the shared edges.
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                if (tint.ToArgb() == Color.White.ToArgb())
                    DrawGraphicsImage(graphics, image, destination);
                else
                    DrawTintedImage(graphics, image, destination, 0, image.Width, image.Height, tint);
            }
            finally { graphics.Restore(state); }
        }
        XmlElement? frame = definition.Element.ChildNodes.OfType<XmlElement>()
            .FirstOrDefault(child => child.LocalName.Equals(tag,
                StringComparison.OrdinalIgnoreCase));
        string baseName = imageOverride ?? frame?.GetAttribute("filename") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(baseName)) return Rectangle.Empty;
        Image?[] slices = Enumerable.Range(0, 9)
            .Select(index => (tag == "bfrm" && (type == 1 && index >= 3 || type == 2 && index < 6))
                ? null : GetBitmap(baseName + index.ToString("00", CultureInfo.InvariantCulture), symbols))
            .ToArray();
        if (slices.All(image => image is null))
        {
            // The preview accepts a single-image base skin, but Fresh overlays
            // are assembled exclusively from numbered subframe bitmaps.
            if (slicedOnly || tag != "bfrm" || bounds.Width <= 0 || bounds.Height <= 0)
                return Rectangle.Empty;
            Image? direct = GetBitmap(baseName, symbols);
            if (direct is null) return Rectangle.Empty;
            fallbackHeight = bounds.Height;
            if (tint.A == 0) return Rectangle.Empty;
            DrawSlice(direct, bounds);
            return bounds;
        }

        // Fresh leaves the last layer's geometry in place if slice 00 is absent.
        if (slices[0] is Image firstSlice)
        {
            int left = firstSlice.Width;
            int right = slices[2]?.Width ?? slices[5]?.Width ?? slices[8]?.Width ?? 0;
            int top = type == 2 ? 0 : firstSlice.Height;
            int bottom = type == 1 ? 0 : slices[6]?.Height ?? slices[7]?.Height ?? 0;
            int middle = (int)Math.Max(0L, (long)bounds.Width - left - right);
            int center = type == 0 || (type == 3 && !slicedOnly)
                ? (int)Math.Max(0L, (long)bounds.Height - top - bottom) : 0;
            previousGeometry = new FrameSliceGeometry(left, middle, right, top, center, bottom);
        }
        // Context menus only draw types 0–2. Normal frames also draw type 3.
        if (type < 0 || type > (slicedOnly ? 2 : 3)) return Rectangle.Empty;
        if (previousGeometry is not { } geometry) return Rectangle.Empty;
        int[] widths = [geometry.Left, geometry.Middle, geometry.Right];
        int[] heights = [geometry.Top, geometry.Center, geometry.Bottom];
        long height = (long)geometry.Top + geometry.Center + geometry.Bottom;
        if (tint.A == 0) return Rectangle.Empty;
        Image?[]? blink = null;
        if (showBlink && type is 0 or 1 or 3)
        {
            string prefix = definition.Element["cfrm"]?.GetAttribute("filename") ?? string.Empty;
            if (prefix.Length > 0 && GetBitmap(prefix + "00", symbols) is Image first)
                blink = [first, GetBitmap(prefix + "01", symbols), GetBitmap(prefix + "02", symbols)];
        }
        Rectangle? renderedBounds = null;
        int y = alignBottom ? ClampCoordinate((long)bounds.Y + bounds.Height - height) : bounds.Y;
        for (int row = 0; row < 3; row++)
        {
            bool drawRow = row switch { 0 => type is 0 or 1 or 3, 1 => type is 0 or 3, _ => type is 0 or 2 or 3 };
            Image?[] rowImages = row == 0 && blink is not null ? blink : slices;
            int rowStart = ReferenceEquals(rowImages, slices) ? row * 3 : 0;
            int x = bounds.X;
            for (int column = 0; column < 3; column++)
            {
                Image? image = rowImages[rowStart] is null ? null : rowImages[rowStart + column];
                var target = new Rectangle(x, y, widths[column], heights[row]);
                if (drawRow && image is not null && target.Width > 0 && target.Height > 0)
                {
                    IncludeBounds(ref renderedBounds, target);
                    DrawSlice(image, target);
                }
                x = ClampCoordinate((long)x + widths[column]);
            }
            y = ClampCoordinate((long)y + heights[row]);
        }
        return renderedBounds ?? Rectangle.Empty;
    }

    private Rectangle GetReferencedBounds(PangyaUiNode node,
        PangyaUiRenderOptions options, HashSet<PangyaUiNode> visited, Point parentOrigin = default)
    {
        // Expanded controls are visited in the ordinary display pass. A nested
        // deferred window suppresses the same subtree during drawing and measuring.
        if (options.DeferredDisplayContainers?.Contains(node) == true ||
            IsRuntimeHidden(node, options) || !IsRenderVisible(node, options) ||
            IsAreaEntranceFinished(node, options) || !visited.Add(node))
            return Rectangle.Empty;
        Rectangle? result = null;
        Point absoluteOrigin = AddOrigins(parentOrigin, GetWindowOffset(node, options));
        IReadOnlySet<string> symbols = Symbols(options);
        string caption = GetStaticCaption(node, options);
        string resource = GetRenderResource(node, options.ButtonState, options.AreaImages);
        string resourceType = IsArea(node) && options.AreaImages?.ContainsKey(node) == true ? "BITMAP" : GetPreviewReferenceType(node);
        Image? areaBitmap = IsArea(node) ? options.AreaBitmapImages?.GetValueOrDefault(node) : null;
        PangyaUiNode? definition = areaBitmap is null
            ? _resources.TryResolve(resource, resourceType, symbols) : null;
        if (GetBitmapFilename(definition) is string bitmapFilename)
        {
            resource = bitmapFilename;
            definition = null;
        }
        if (IsContextMenu(node))
        {
            using var bitmap = new Bitmap(1, 1);
            using Graphics metrics = Graphics.FromImage(bitmap);
            IncludeBounds(ref result,
                RenderContextMenu(metrics, node, GetWindowBounds(node, options).Location, options, draw: false, out _));
        }
        else if (GetTabs(node, options) is { } tabs)
            IncludeBounds(ref result, RenderTabs(null, node, GetWindowBounds(node, options).Location, tabs, symbols, options));
        else if (IsTextButton(node))
        {
            string textButton = GetTextButtonCaption(node, options);
            using var bitmap = new Bitmap(1, 1);
            using Graphics metrics = Graphics.FromImage(bitmap);
            DrawTextButton(metrics, node, GetWindowBounds(node, options), textButton, options with { Pointer = null },
                out Rectangle measuredBounds, draw: false, clipOrigin: absoluteOrigin);
            IncludeBounds(ref result, measuredBounds);
        }
        else if ((options.IncludeCaptionVisualBounds || options.IncludeStaticCaptionBounds) &&
                 caption.Length > 0 && !IsPrivacyHidden(node, options) &&
                 node.Type.Equals("STATIC", StringComparison.OrdinalIgnoreCase))
            IncludeBounds(ref result, MeasureStaticCaptionBounds(node, GetWindowBounds(node, options),
                caption, options, absoluteOrigin));
        else if (GetList(node, options) is { } list)
            IncludeBounds(ref result, DrawList(null, node, GetWindowBounds(node, options), list, options));
        else if ((caption.Length > 0 || node.LayoutBase is not null) &&
                 definition?.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) != true &&
                 !(definition is { IsForm: true, IsLayout: false }))
            IncludeBounds(ref result, GetWindowBounds(node, options));
        else if (IsArea(node) && options.AreaOwnerDraw?.ContainsKey(node) == true)
        {
            Rectangle window = GetWindowBounds(node, options);
            IncludeBounds(ref result, window);
            Image? background = areaBitmap ?? (string.IsNullOrWhiteSpace(resource) ? null : _images.GetImage(resource));
            if (background is not null && GetAreaTint(node, options).A > 0)
            {
                Rectangle destination = ImageBounds(node, window, background);
                IncludeBounds(ref result, ClipImageBounds(node, window, destination, options, absoluteOrigin));
            }
            if (!options.SkipAreaEntranceEffects &&
                node.Parameters.GetValueOrDefault("entrance_effect") is { Length: > 0 } effect &&
                GetBitmap(resource, symbols) is Image image)
            {
                var (opening, duration, direction) = ReadAreaEntrance(effect);
                var rest = new Rectangle(GetWindowBounds(node, options).Location, image.Size);
                IncludeBounds(ref result, rest);
                if (opening > 0 || duration >= 0)
                {
                    int x = direction switch { "left" => -image.Width, "right" => image.Width, _ => 0 };
                    int y = direction switch { "top" => -image.Height, "bottom" => image.Height, _ => 0 };
                    IncludeBounds(ref result, new Rectangle(ClampCoordinate((long)rest.X + x),
                        ClampCoordinate((long)rest.Y + y), rest.Width, rest.Height));
                }
            }
        }
        else if (IsViewer(node))
            IncludeBounds(ref result, DrawViewer(null, node, GetWindowBounds(node, options), options));
        else if (IsEdit(node))
            IncludeBounds(ref result, MeasureEditBounds(node, GetWindowBounds(node, options), symbols, options));
        else if (IsGauge(node))
            IncludeBounds(ref result, MeasureGaugeBounds(node, GetWindowBounds(node, options), symbols, options));
        else if (IsImageGauge(node))
            IncludeBounds(ref result, RenderImageGauge(null, node, GetWindowBounds(node, options).Location, symbols, options));
        else if (HasButtonSkin(node, options))
            IncludeBounds(ref result, MeasureButtonBounds(node, GetWindowBounds(node, options), symbols, options));
        else if (definition?.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase) == true)
            IncludeBounds(ref result, GetBaseFrameBounds(GetWindowBounds(node, options), definition, symbols,
                node.Parameters.GetValueOrDefault("bgimg"), node, options));
        else if (definition is { IsForm: true, IsLayout: false })
            IncludeBounds(ref result, GetInstantiatedFormBounds(definition,
                GetInstantiatedFormRectangle(node, definition, GetWindowBounds(node, options), options),
                ForResourceChildren(node, options), visited, absoluteOrigin));
        else if (definition is not null && IsMacroItem(node))
        {
            // Measure the same children DrawNode instantiates directly under the host.
            if (visited.Add(definition))
            {
                var childOptions = ForResourceChildren(node, options);
                foreach (PangyaUiNode child in definition.Children.Where(child => child.IsVisible(symbols)))
                    IncludeBounds(ref result, GetReferencedBounds(child, childOptions, visited, absoluteOrigin));
                visited.Remove(definition);
            }
        }
        else if (definition is not null)
        {
            Rectangle referenced = GetReferencedBounds(definition, options, visited,
                AddOrigins(absoluteOrigin, GetWindowBounds(node, options).Location));
            if (!referenced.IsEmpty) referenced.Offset(GetWindowBounds(node, options).Location);
            IncludeBounds(ref result, referenced);
        }
        else if (!IsMacroItem(node))
        {
            Image? image = areaBitmap ?? (string.IsNullOrWhiteSpace(resource) ? null : _images.GetImage(resource));
            if (image is not null && IsRenderVisible(node, options) &&
                !IsTransparent(node, options.ButtonState) && (!IsArea(node) || GetAreaTint(node, options).A > 0))
            {
                Rectangle destination = ImageBounds(node, GetWindowBounds(node, options), image);
                IncludeBounds(ref result,
                    ApplyAreaEntrance(node, GetWindowBounds(node, options), image, options, ref destination) ??
                    ClipImageBounds(node, GetWindowBounds(node, options), destination, options, absoluteOrigin));
            }
        }

        foreach (PangyaUiNode child in node.Children.Where(child => child.IsVisible(symbols)))
        {
            Rectangle childBounds = GetReferencedBounds(child, options, visited,
                IsMacroItem(node) ? absoluteOrigin : AddOrigins(absoluteOrigin, GetWindowBounds(node, options).Location));
            if (!IsMacroItem(node) && !childBounds.IsEmpty)
                childBounds.Offset(GetWindowBounds(node, options).Location);
            IncludeBounds(ref result, childBounds);
        }
        if (IsCombo(node) && options.Combos?.GetValueOrDefault(node) is { IsOpen: true } combo)
        {
            Rectangle window = GetWindowBounds(node, options);
            window.Location = AddOrigins(window.Location, absoluteOrigin);
            Rectangle edit = GetEditBounds(node, window, symbols, options);
            Rectangle dropdown = DrawComboList(null, node, edit,
                ResolveReferencedComboSide(combo, edit.Y, options), symbols, options);
            if (!dropdown.IsEmpty)
            {
                dropdown.X = ClampCoordinate((long)dropdown.X - absoluteOrigin.X);
                dropdown.Y = ClampCoordinate((long)dropdown.Y - absoluteOrigin.Y);
                IncludeBounds(ref result, dropdown);
            }
        }
        visited.Remove(node);
        if (result is Rectangle movedBounds)
        {
            Point windowOffset = GetWindowOffset(node, options);
            movedBounds.X = ClampCoordinate((long)movedBounds.X + windowOffset.X);
            movedBounds.Y = ClampCoordinate((long)movedBounds.Y + windowOffset.Y);
            result = movedBounds;
        }
        return result ?? Rectangle.Empty;
    }

    private static bool IsMacroItem(PangyaUiNode node) =>
        node.Type.Equals("MACROITEM", StringComparison.OrdinalIgnoreCase);

    private static PangyaUiRenderOptions ForResourceChildren(PangyaUiNode host, PangyaUiRenderOptions options) =>
        options with { ResourceInstance = new PangyaUiResourceInstance(host, options.ResourceInstance) };

    private static bool IsMacroDefinition(PangyaUiNode node) =>
        IsMacroItem(node) && node.Element.LocalName.Equals("element", StringComparison.OrdinalIgnoreCase);

    private static bool IsArea(PangyaUiNode node) =>
        node.Type.Equals("AREA", StringComparison.OrdinalIgnoreCase);

    private static Color GetAreaTint(PangyaUiNode node, PangyaUiRenderOptions options)
    {
        Color tint = ParseColor(node.Parameters.GetValueOrDefault("color"), Color.White, implicitAlpha: false);
        byte opacity = options.AreaOpacity?.GetValueOrDefault(node, byte.MaxValue) ?? byte.MaxValue;
        return Color.FromArgb((int)(tint.A * GetWindowAlphaScale(node, options) * opacity / 255), tint);
    }

    private static Rectangle ImageBounds(PangyaUiNode node, Rectangle bounds, Image image)
    {
        if (IsArea(node) && ReadDecimalPrefix(node.Parameters.GetValueOrDefault("center")) != 0)
        {
            int x = ClampCoordinate(bounds.X + ((long)bounds.Width - image.Width + 1) / 2);
            int y = ClampCoordinate(bounds.Y + ((long)bounds.Height - image.Height + 1) / 2);
            return new Rectangle(new Point(x, y), image.Size);
        }
        return node.IsExplicitlyStretched && (IsArea(node) || (bounds.Width > 0 && bounds.Height > 0))
            ? bounds
            : new Rectangle(bounds.Location, image.Size);
    }

    private static Rectangle ClipImageBounds(PangyaUiNode node, Rectangle bounds, Rectangle destination,
        PangyaUiRenderOptions options, Point absoluteOrigin = default)
    {
        if (!IsArea(node)) return destination;
        bool? clippingEnabled = options.AreaClippingEnabled?.TryGetValue(node, out bool supplied) == true
            ? supplied : null;
        if (clippingEnabled == false) return destination;
        bool xmlClipping = ReadDecimalPrefix(node.Parameters.GetValueOrDefault("enable_clip")) == 1;
        if (clippingEnabled != true && !xmlClipping)
            return destination;

        if (options.AreaClipOverrides?.TryGetValue(node, out Rectangle runtimeClip) == true)
            return IntersectAreaClip(destination, runtimeClip, absoluteOrigin);

        if (node.Parameters.TryGetValue("clip_area", out string? area))
            return PangyaUiDimensionHelper.ParseWindowRectangle(area) is Rectangle explicitClip
                ? IntersectAreaClip(destination, explicitClip, absoluteOrigin) : Rectangle.Empty;
        return IntersectAreaClip(destination, bounds);
    }

    private static Rectangle IntersectAreaClip(Rectangle destination, Rectangle clip, Point absoluteOrigin = default)
    {
        if (destination.Width <= 0 || destination.Height <= 0 || clip.Width <= 0 || clip.Height <= 0)
            return Rectangle.Empty;
        long left = Math.Max((long)destination.X + absoluteOrigin.X, clip.X);
        long top = Math.Max((long)destination.Y + absoluteOrigin.Y, clip.Y);
        long right = Math.Min((long)destination.X + absoluteOrigin.X + destination.Width, (long)clip.X + clip.Width);
        long bottom = Math.Min((long)destination.Y + absoluteOrigin.Y + destination.Height, (long)clip.Y + clip.Height);
        return right <= left || bottom <= top ? Rectangle.Empty :
            new Rectangle(ClampCoordinate(left - absoluteOrigin.X), ClampCoordinate(top - absoluteOrigin.Y),
                (int)(right - left), (int)(bottom - top));
    }

    private static Point AddOrigins(Point first, Point second) =>
        new(ClampCoordinate((long)first.X + second.X), ClampCoordinate((long)first.Y + second.Y));

    public static bool AreaAcceptsMouse(PangyaUiNode node, PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsArea(node) || IsRuntimeHidden(node, options) || !IsRenderVisible(node, options) ||
            options?.DisabledNodes?.Contains(node) == true) return false;
        return options?.AreaMouseEvents?.TryGetValue(node, out bool enabled) == true ? enabled :
            ReadDecimalPrefix(node.Parameters.GetValueOrDefault("mouse_event")) != 0;
    }

    public static PangyaUiRenderOptions SetAreaMouseEvent(PangyaUiNode node, bool active,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsArea(node)) throw new ArgumentException("Expected an AREA control.", nameof(node));
        options ??= new();
        var flags = options.AreaMouseEvents?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        flags[node] = active;
        return options with { AreaMouseEvents = flags };
    }

    public static PangyaUiRenderOptions SetAreaClipArea(PangyaUiNode node, Rectangle area,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsArea(node)) throw new ArgumentException("Expected an AREA control.", nameof(node));
        options ??= new();
        var clips = options.AreaClipOverrides?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        clips[node] = area;
        return options with { AreaClipOverrides = clips };
    }

    public static PangyaUiRenderOptions EnableAreaClipping(PangyaUiNode node, bool enabled,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (!IsArea(node)) throw new ArgumentException("Expected an AREA control.", nameof(node));
        options ??= new();
        var flags = options.AreaClippingEnabled?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? [];
        flags[node] = enabled;
        return options with { AreaClippingEnabled = flags };
    }

    private static bool IsGroupBox(PangyaUiNode node) =>
        node.Type.Equals("GROUPBOX", StringComparison.OrdinalIgnoreCase);

    private static bool IsTransparent(PangyaUiNode node, PangyaUiButtonState state)
    {
        if (node.Type.Equals("TABBUTTON", StringComparison.OrdinalIgnoreCase))
            return state == PangyaUiButtonState.Normal;
        return IsGroupBox(node) ||
               node.Type.Equals("LISTBOX", StringComparison.OrdinalIgnoreCase) ||
               (node.IsForm && !node.IsLayout && string.IsNullOrWhiteSpace(node.GetResource(state)));
    }

    private static IReadOnlySet<string> Symbols(PangyaUiRenderOptions options) =>
        options.EnabledSymbols ??
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private static void IncludeBounds(ref Rectangle? aggregate, Rectangle bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return;
        aggregate = aggregate is Rectangle existing ? Rectangle.Union(existing, bounds) : bounds;
    }

    private static void DrawOutline(Graphics graphics, Rectangle bounds, Color color, float width)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return;
        using var pen = new Pen(color, width);
        graphics.DrawRectangle(pen, bounds);
    }

    private static Point GetWindowOffset(PangyaUiNode node, PangyaUiRenderOptions? options) =>
        IsMacroItem(node) ? Point.Empty : options?.WindowOffsets?.GetValueOrDefault(node) ?? Point.Empty;

    private static Rectangle GetWindowBounds(PangyaUiNode node, PangyaUiRenderOptions? options)
    {
        Rectangle bounds = node.Bounds;
        if (!IsMacroItem(node) && options?.WindowSizes?.TryGetValue(node, out Size size) == true)
            bounds.Size = new Size(Math.Max(0, size.Width), Math.Max(0, size.Height));
        return bounds;
    }

    private static Point GetParentOffset(PangyaUiNode node, PangyaUiRenderOptions? options)
    {
        long x = 0;
        long y = 0;
        for (PangyaUiNode? parent = node.Parent; parent is not null; parent = parent.Parent)
        {
            if (IsMacroItem(parent)) continue;
            x += parent.Bounds.X;
            y += parent.Bounds.Y;
            Point windowOffset = GetWindowOffset(parent, options);
            x += windowOffset.X;
            y += windowOffset.Y;
        }
        return new Point(ClampCoordinate(x), ClampCoordinate(y));
    }

    public static Rectangle GetPreviewCanvasBounds(PangyaUiDocument document, PangyaUiNode? selectedForm,
        PangyaUiRenderOptions options)
    {
        if (selectedForm is null) return new Rectangle(Point.Empty, document.CanvasSize);
        Rectangle form = GetAbsoluteBounds(selectedForm, options);
        Point origin = options.AnchorCanvasToSelectedForm && !selectedForm.IsLayout ? form.Location : Point.Empty;
        bool hasDeclaredSize = !selectedForm.IsLayout && !selectedForm.IsRefresh &&
            PangyaUiDimensionHelper.ParseSize(selectedForm.Element.GetAttribute("size")) is not null;
        Size size = hasDeclaredSize || form.Size is { Width: > 0, Height: > 0 } ? form.Size : document.CanvasSize;
        return new Rectangle(origin, size);
    }

    private static Rectangle GetAbsoluteBounds(PangyaUiNode node, PangyaUiRenderOptions? options)
    {
        Point offset = GetParentOffset(node, options);
        Point windowOffset = GetWindowOffset(node, options);
        return new Rectangle(
            ClampCoordinate((long)GetWindowBounds(node, options).X + offset.X + windowOffset.X),
            ClampCoordinate((long)GetWindowBounds(node, options).Y + offset.Y + windowOffset.Y),
            GetWindowBounds(node, options).Width,
            GetWindowBounds(node, options).Height);
    }

    private static int ClampCoordinate(long value) =>
        (int)Math.Clamp(value, int.MinValue, int.MaxValue);

    private static void DrawDebugBounds(Graphics graphics, PangyaUiNode node,
        Rectangle bounds, float scale)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return;
        DrawOutline(graphics, bounds, Color.FromArgb(150, Color.DeepSkyBlue), scale);
        using var font = new Font(FontFamily.GenericSansSerif, 8f * scale);
        SizeF labelSize = graphics.MeasureString(node.DisplayName, font);
        var labelBounds = new RectangleF(bounds.X, bounds.Y, labelSize.Width, labelSize.Height);
        using var background = new SolidBrush(Color.FromArgb(180, 35, 75, 95));
        using var foreground = new SolidBrush(Color.White);
        graphics.FillRectangle(background, labelBounds);
        graphics.DrawString(node.DisplayName, font, foreground, labelBounds.Location);
    }
}
