using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

namespace PangyaAPI.UI;

public sealed record PangyaUiRenderOptions(
    PangyaUiButtonState ButtonState = PangyaUiButtonState.Normal,
    IReadOnlySet<string>? EnabledSymbols = null,
    bool ShowDebugBounds = false,
    PangyaUiNode? SelectedNode = null,
    float StrokeScale = 1f);

public sealed class PangyaUiRenderer
{
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
        options ??= new PangyaUiRenderOptions();
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        using var canvasBrush = new SolidBrush(Color.FromArgb(46, 49, 55));
        Size canvasSize = selectedForm?.Bounds.Size is { Width: > 0, Height: > 0 } size
            ? size
            : document.CanvasSize;
        graphics.FillRectangle(canvasBrush, new Rectangle(Point.Empty, canvasSize));

        foreach (PangyaUiNode node in GetRenderOrder(document, selectedForm,
                     options.SelectedNode, options.EnabledSymbols))
        {
            DrawNode(graphics, node, GetParentOffset(node), options,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            if (options.ShowDebugBounds)
                DrawDebugBounds(graphics, node, GetAbsoluteBounds(node), options.StrokeScale);
        }

        if (options.SelectedNode is not null)
        {
            Rectangle selectedBounds = GetRenderedBounds(options.SelectedNode, options);
            if (selectedBounds.Width <= 0 || selectedBounds.Height <= 0)
                selectedBounds = GetAbsoluteBounds(options.SelectedNode);
            DrawOutline(graphics, selectedBounds, Color.Orange, 2f * options.StrokeScale);
        }
    }

    public IReadOnlyList<PangyaUiNode> GetRenderOrder(PangyaUiDocument document,
        PangyaUiNode? selectedForm = null, PangyaUiNode? selectedNode = null,
        IReadOnlySet<string>? enabledSymbols = null)
    {
        ArgumentNullException.ThrowIfNull(document);
        IReadOnlyList<PangyaUiNode> visible = selectedForm is null
            ? []
            : document.GetNodesForForm(selectedForm, enabledSymbols);
        if (selectedNode is null || selectedNode.IsForm || !visible.Contains(selectedNode))
            return visible;
        return [.. visible.Where(node => !ReferenceEquals(node, selectedNode)), selectedNode];
    }

    public PangyaUiNode? HitTest(PangyaUiDocument document, PangyaUiNode? selectedForm,
        Point point, PangyaUiRenderOptions? options = null)
    {
        options ??= new PangyaUiRenderOptions();
        return GetRenderOrder(document, selectedForm, options.SelectedNode,
                options.EnabledSymbols)
            .LastOrDefault(node =>
            {
                Rectangle bounds = GetRenderedBounds(node, options);
                return bounds.Width > 0 && bounds.Height > 0 && bounds.Contains(point);
            });
    }

    public Rectangle GetRenderedBounds(PangyaUiNode node,
        PangyaUiRenderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        options ??= new PangyaUiRenderOptions();
        IReadOnlySet<string> symbols = Symbols(options);
        Rectangle absoluteBounds = GetAbsoluteBounds(node);
        if (!node.IsRenderVisible || IsTransparent(node, options.ButtonState))
            return options.ShowDebugBounds ? absoluteBounds : Rectangle.Empty;

        string resource = node.GetResource(options.ButtonState);
        PangyaUiNode? definition = _resources.TryResolve(resource, node.Type, symbols);
        if (definition is not null)
        {
            Rectangle bounds = absoluteBounds;
            if (bounds.Width > 0 && bounds.Height > 0) return bounds;
            Rectangle referenced = GetReferencedBounds(definition, options,
                new HashSet<PangyaUiNode>(ReferenceEqualityComparer.Instance));
            referenced.Offset(bounds.Location);
            return referenced;
        }
        Image? image = string.IsNullOrWhiteSpace(resource) ? null : _images.GetImage(resource);
        if (image is null) return options.ShowDebugBounds ? absoluteBounds : Rectangle.Empty;
        return ImageBounds(node, absoluteBounds, image);
    }

    private void DrawNode(Graphics graphics, PangyaUiNode node, Point offset,
        PangyaUiRenderOptions options, HashSet<string> resourceChain)
    {
        if (!node.IsRenderVisible || IsTransparent(node, options.ButtonState)) return;
        IReadOnlySet<string> symbols = Symbols(options);
        Rectangle nodeBounds = node.Bounds;
        nodeBounds.Offset(offset);
        string resource = node.GetResource(options.ButtonState);
        PangyaUiNode? definition = _resources.TryResolve(resource, node.Type, symbols);
        if (definition is not null)
        {
            if (!resourceChain.Add(resource)) return;
            if (definition.Type.Equals("FRAME", StringComparison.OrdinalIgnoreCase))
                DrawFrame(graphics, nodeBounds, definition);
            else
            {
                Point childOffset = new(nodeBounds.X, nodeBounds.Y);
                foreach (PangyaUiNode child in definition.Children.Where(child =>
                             child.IsVisible(symbols)))
                    DrawNode(graphics, child, childOffset, options, resourceChain);
            }
            resourceChain.Remove(resource);
            return;
        }

        Image? image = string.IsNullOrWhiteSpace(resource) ? null : _images.GetImage(resource);
        if (image is not null) graphics.DrawImage(image, ImageBounds(node, nodeBounds, image));
    }

    private void DrawFrame(Graphics graphics, Rectangle bounds, PangyaUiNode definition)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0) return;
        XmlElement? frame = definition.Element.ChildNodes.OfType<XmlElement>()
            .LastOrDefault(child => child.LocalName.Equals("bfrm",
                StringComparison.OrdinalIgnoreCase));
        string baseName = frame?.GetAttribute("filename") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(baseName)) return;
        if (Path.HasExtension(baseName))
        {
            Image? direct = _images.GetImage(baseName);
            if (direct is not null) graphics.DrawImage(direct, bounds);
            return;
        }

        Image?[] slices = Enumerable.Range(0, 9)
            .Select(index => _images.GetImage(baseName + index.ToString("00",
                System.Globalization.CultureInfo.InvariantCulture)))
            .ToArray();
        if (slices.All(image => image is null))
        {
            Image? direct = _images.GetImage(baseName);
            if (direct is not null) graphics.DrawImage(direct, bounds);
            return;
        }

        int left = Math.Min(slices[0]?.Width ?? slices[3]?.Width ?? 0, bounds.Width);
        int right = Math.Min(slices[2]?.Width ?? slices[5]?.Width ?? 0,
            Math.Max(0, bounds.Width - left));
        int top = Math.Min(slices[0]?.Height ?? slices[1]?.Height ?? 0, bounds.Height);
        int bottom = Math.Min(slices[6]?.Height ?? slices[7]?.Height ?? 0,
            Math.Max(0, bounds.Height - top));
        int[] widths = [left, Math.Max(0, bounds.Width - left - right), right];
        int[] heights = [top, Math.Max(0, bounds.Height - top - bottom), bottom];
        int slice = 0;
        int y = bounds.Y;
        for (int row = 0; row < 3; row++)
        {
            int x = bounds.X;
            for (int column = 0; column < 3; column++)
            {
                Image? image = slices[slice++];
                var target = new Rectangle(x, y, widths[column], heights[row]);
                if (image is not null && target.Width > 0 && target.Height > 0)
                    graphics.DrawImage(image, target);
                x += widths[column];
            }
            y += heights[row];
        }
    }

    private Rectangle GetReferencedBounds(PangyaUiNode node,
        PangyaUiRenderOptions options, HashSet<PangyaUiNode> visited)
    {
        if (!visited.Add(node)) return Rectangle.Empty;
        Rectangle? result = null;
        IReadOnlySet<string> symbols = Symbols(options);
        string resource = node.GetResource(options.ButtonState);
        PangyaUiNode? definition = _resources.TryResolve(resource, node.Type, symbols);
        if (definition is not null)
        {
            Rectangle referenced = GetReferencedBounds(definition, options, visited);
            referenced.Offset(node.Bounds.Location);
            IncludeBounds(ref result, referenced);
        }
        else
        {
            Image? image = string.IsNullOrWhiteSpace(resource) ? null : _images.GetImage(resource);
            if (image is not null && node.IsRenderVisible &&
                !IsTransparent(node, options.ButtonState))
                IncludeBounds(ref result, ImageBounds(node, node.Bounds, image));
        }

        foreach (PangyaUiNode child in node.Children.Where(child => child.IsVisible(symbols)))
            IncludeBounds(ref result, GetReferencedBounds(child, options, visited));
        visited.Remove(node);
        return result ?? Rectangle.Empty;
    }

    private static Rectangle ImageBounds(PangyaUiNode node, Rectangle bounds, Image image) =>
        node.IsExplicitlyStretched && bounds.Width > 0 && bounds.Height > 0
            ? bounds
            : new Rectangle(bounds.Location, image.Size);

    private static bool IsTransparent(PangyaUiNode node, PangyaUiButtonState state)
    {
        if (node.Type.Equals("TABBUTTON", StringComparison.OrdinalIgnoreCase))
            return state == PangyaUiButtonState.Normal;
        return node.Type.Equals("GROUPBOX", StringComparison.OrdinalIgnoreCase) ||
               node.Type.Equals("LISTBOX", StringComparison.OrdinalIgnoreCase) ||
               (node.IsForm && string.IsNullOrWhiteSpace(node.GetResource(state)));
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

    private static Point GetParentOffset(PangyaUiNode node)
    {
        long x = 0;
        long y = 0;
        for (PangyaUiNode? parent = node.Parent; parent is not null; parent = parent.Parent)
        {
            x += parent.Bounds.X;
            y += parent.Bounds.Y;
        }
        return new Point(ClampCoordinate(x), ClampCoordinate(y));
    }

    private static Rectangle GetAbsoluteBounds(PangyaUiNode node)
    {
        Point offset = GetParentOffset(node);
        return new Rectangle(
            ClampCoordinate((long)node.Bounds.X + offset.X),
            ClampCoordinate((long)node.Bounds.Y + offset.Y),
            node.Bounds.Width,
            node.Bounds.Height);
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
