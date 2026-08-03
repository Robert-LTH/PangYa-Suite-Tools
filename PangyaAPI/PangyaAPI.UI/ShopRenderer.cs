using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace PangyaAPI.UI;

public sealed record ShopRenderText(
    string NoItems,
    string CartSummaryFormat,
    string BalancesFormat,
    string FilterFormat,
    string EditHint);

public enum ShopRenderMode
{
    Simulation,
    Editor,
}

public sealed record ShopRenderState(
    int CategoryIndex,
    int Page,
    bool Rental,
    string Filter,
    string? HoveredElement,
    ShopSession Session,
    ShopRenderMode Mode = ShopRenderMode.Simulation);

public sealed record ShopVisibleItem(
    ShopCatalogItem Item,
    Rectangle Bounds,
    Rectangle IconBounds);

public sealed record ShopRenderResult(
    int CategoryIndex,
    int Page,
    int MaximumPage,
    IReadOnlyList<ShopVisibleItem> VisibleItems);

public sealed class ShopRenderer
{
    public const int ItemsPerPage = 24;
    public static Rectangle CatalogBounds { get; } = new(384, 148, 405, 375);
    public static Rectangle ScrollBarBounds { get; } = new(780, 150, 9, 366);

    private readonly IPangyaImageProvider _images;

    public ShopRenderer(IPangyaImageProvider images) =>
        _images = images ?? throw new ArgumentNullException(nameof(images));

    public ShopRenderResult Render(Graphics graphics, ShopLayout layout,
        IReadOnlyList<ShopCatalogItem> catalog, ShopRenderState state,
        ShopRenderText text, CultureInfo? culture = null)
    {
        ArgumentNullException.ThrowIfNull(graphics);
        ArgumentNullException.ThrowIfNull(layout);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(text);
        culture ??= CultureInfo.CurrentCulture;
        graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
        foreach (ShopLayoutElement element in layout.Elements.Where(element =>
                     IsRenderableElement(element.Type)))
            DrawElement(graphics, element, state);

        string[] categories = Categories(catalog);
        var visible = new List<ShopVisibleItem>();
        int categoryIndex = categories.Length == 0
            ? 0
            : Math.Clamp(state.CategoryIndex, 0, categories.Length - 1);
        int page = 0;
        int maximumPage = 0;
        if (categories.Length == 0)
        {
            DrawText(graphics, text.NoItems, new Rectangle(400, 180, 360, 80),
                Color.White, 11, ContentAlignment.MiddleCenter);
        }
        else
        {
            string category = categories[categoryIndex];
            var categoryBounds = new Rectangle(392, 82, 330, 33);
            using (var brush = new LinearGradientBrush(categoryBounds,
                       Color.FromArgb(235, 19, 125, 197),
                       Color.FromArgb(235, 8, 65, 120), 90f))
                graphics.FillRectangle(brush, categoryBounds);
            graphics.DrawRectangle(Pens.LightSkyBlue, categoryBounds);
            DrawText(graphics,
                $"{category}  ({categoryIndex + 1}/{categories.Length})",
                new Rectangle(401, 84, 310, 28), Color.White, 11,
                ContentAlignment.MiddleLeft);
            ShopCatalogItem[] filtered = Filter(catalog, category, state.Filter);
            maximumPage = GetMaximumPage(filtered.Length);
            page = Math.Clamp(state.Page, 0, maximumPage);
            ShopCatalogItem[] pageItems = filtered.Skip(page * ItemsPerPage)
                .Take(ItemsPerPage).ToArray();
            for (int index = 0; index < pageItems.Length; index++)
            {
                int column = index % 4;
                int row = index / 4;
                var bounds = new Rectangle(392 + column * 98, 150 + row * 61, 92, 56);
                ShopCatalogItem item = pageItems[index];
                using var background = new SolidBrush(Color.FromArgb(185, 20, 24, 30));
                graphics.FillRectangle(background, bounds);
                graphics.DrawRectangle(Pens.Gray, bounds);
                var iconBounds = new Rectangle(bounds.X + 3, bounds.Y + 3, 38, 38);
                Image? icon = string.IsNullOrWhiteSpace(item.IconPath)
                    ? null
                    : _images.GetImageByPath(item.IconPath);
                if (icon is null)
                    DrawMissingIcon(graphics, iconBounds);
                else
                    graphics.DrawImage(icon, iconBounds);
                DrawText(graphics, item.Name,
                    new Rectangle(bounds.X + 44, bounds.Y + 2, 45, 31),
                    Color.White, 7, ContentAlignment.TopLeft);
                uint price = state.Rental && item.RentalPrice != 0
                    ? item.RentalPrice
                    : item.PurchasePrice;
                DrawText(graphics, price.ToString("N0", culture),
                    new Rectangle(bounds.X + 43, bounds.Y + 31, 46, 14),
                    item.IsCash ? Color.Gold : Color.LightGreen, 7,
                    ContentAlignment.MiddleRight);
                DrawText(graphics, $"S:{item.ShopFlags:X2} M:{item.MoneyFlags:X2}",
                    new Rectangle(bounds.X + 43, bounds.Y + 43, 47, 11),
                    Color.LightSkyBlue, 6, ContentAlignment.MiddleRight);
                string? banner = GetBannerResource(item);
                if (banner is not null)
                {
                    Image? bannerImage = _images.GetImage(banner);
                    if (bannerImage is not null)
                        graphics.DrawImage(bannerImage,
                            new Rectangle(bounds.X + 3, bounds.Y + 3, 37, 37));
                }
                visible.Add(new ShopVisibleItem(item, bounds, iconBounds));
            }
            DrawScrollBar(graphics, page, maximumPage);
        }

        if (state.Mode == ShopRenderMode.Simulation)
        {
            (ulong pang, ulong cookies) = state.Session.Totals(state.Rental);
            DrawText(graphics, string.Format(culture, text.CartSummaryFormat,
                    state.Session.Cart.Count, pang, cookies),
                new Rectangle(15, 408, 335, 45), Color.White, 9,
                ContentAlignment.MiddleLeft);
            DrawText(graphics, string.Format(culture, text.BalancesFormat,
                    state.Session.Pang, state.Session.Cookies),
                new Rectangle(15, 465, 335, 40), Color.White, 9,
                ContentAlignment.MiddleLeft);
        }
        if (state.Filter.Length != 0)
            DrawText(graphics, string.Format(culture, text.FilterFormat, state.Filter),
                new Rectangle(15, 330, 330, 25), Color.White, 9,
                ContentAlignment.MiddleLeft);
        DrawText(graphics, text.EditHint, new Rectangle(20, 360, 330, 35),
            Color.LightSkyBlue, 8, ContentAlignment.MiddleLeft);
        return new ShopRenderResult(categoryIndex, page, maximumPage, visible);
    }

    public ShopLayoutElement? HitTestElement(ShopLayout layout, Point point)
    {
        ArgumentNullException.ThrowIfNull(layout);
        return layout.Elements.LastOrDefault(element =>
        {
            if (element.Type is not ("BUTTON" or "TEXTBUTTON" or "TABBUTTON"))
                return false;
            Rectangle bounds = element.Bounds;
            if (bounds.Width == 0 || bounds.Height == 0)
            {
                string? resource = element.Parameters.GetValueOrDefault("normal");
                if (resource is null) return false;
                Image? image = _images.GetImage(resource);
                if (image is null) return false;
                bounds.Size = image.Size;
            }
            return bounds.Contains(point);
        });
    }

    public static ShopVisibleItem? HitTestItem(ShopRenderResult result, Point point) =>
        result.VisibleItems.LastOrDefault(item => item.Bounds.Contains(point));

    public static int ScrollPageFromPoint(Point point, int maximumPage)
    {
        if (!ScrollBarBounds.Contains(point) || maximumPage <= 0) return 0;
        return Math.Clamp(
            (point.Y - ScrollBarBounds.Y) * (maximumPage + 1) /
            ScrollBarBounds.Height, 0, maximumPage);
    }

    public static int GetMaximumPage(int itemCount) =>
        Math.Max(0, (itemCount - 1) / ItemsPerPage);

    public static string? GetBannerResource(ShopCatalogItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if ((item.MoneyFlags & 0x40) != 0 || (item.ShopFlags & 0x08) != 0)
            return "mark_surprise_sale";
        if ((item.MoneyFlags & 0x20) != 0) return "mark_hot";
        if ((item.MoneyFlags & 0x02) != 0) return "mark_new";
        return null;
    }

    private void DrawElement(Graphics graphics, ShopLayoutElement element,
        ShopRenderState state)
    {
        string? resource = null;
        if (element.Type.Equals("AREA", StringComparison.OrdinalIgnoreCase))
            element.Parameters.TryGetValue("bgimg", out resource);
        else
        {
            bool selected =
                element.Name.Equals("sidetab_buy", StringComparison.OrdinalIgnoreCase)
                    ? !state.Rental
                    : element.Name.Equals("sidetab_rental",
                        StringComparison.OrdinalIgnoreCase) && state.Rental;
            if (selected)
            {
                element.Parameters.TryGetValue("selected", out resource);
                if (resource is null)
                    element.Parameters.TryGetValue("below_selected", out resource);
            }
            if (resource is null && string.Equals(state.HoveredElement,
                    element.Name, StringComparison.OrdinalIgnoreCase))
            {
                element.Parameters.TryGetValue("over", out resource);
                if (resource is null)
                    element.Parameters.TryGetValue("below_over", out resource);
            }
        }
        resource ??= element.Parameters.GetValueOrDefault("normal");
        resource ??= element.Parameters.GetValueOrDefault("selected");
        resource ??= element.Parameters.GetValueOrDefault("below_selected");
        resource ??= element.Parameters.GetValueOrDefault("over");
        resource ??= element.Parameters.GetValueOrDefault("below_over");
        resource ??= element.Parameters.GetValueOrDefault("sepImg");
        if (string.IsNullOrWhiteSpace(resource)) return;
        Image? image = _images.GetImage(resource);
        if (image is null) return;
        Rectangle destination = element.Bounds;
        if (destination.Width == 0 || destination.Height == 0)
            destination = new Rectangle(destination.Location, image.Size);
        graphics.DrawImage(image, destination);
    }

    private static bool IsRenderableElement(string type) =>
        type.Equals("AREA", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("BUTTON", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("TEXTBUTTON", StringComparison.OrdinalIgnoreCase) ||
        type.Equals("TABBUTTON", StringComparison.OrdinalIgnoreCase);

    private static string[] Categories(IReadOnlyList<ShopCatalogItem> catalog) =>
        catalog.Select(item => item.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(value => value, StringComparer.OrdinalIgnoreCase).ToArray();

    private static ShopCatalogItem[] Filter(IReadOnlyList<ShopCatalogItem> catalog,
        string category, string filter)
    {
        IEnumerable<ShopCatalogItem> query = catalog.Where(item =>
            item.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        if (filter.Length != 0)
            query = query.Where(item => item.Name.Contains(filter,
                StringComparison.CurrentCultureIgnoreCase));
        return query.ToArray();
    }

    private static void DrawScrollBar(Graphics graphics, int page, int maximumPage)
    {
        using var track = new SolidBrush(Color.FromArgb(180, 18, 37, 52));
        graphics.FillRectangle(track, ScrollBarBounds);
        graphics.DrawRectangle(Pens.SteelBlue, ScrollBarBounds);
        int pageCount = maximumPage + 1;
        int thumbHeight = Math.Max(28, ScrollBarBounds.Height / pageCount);
        int travel = ScrollBarBounds.Height - thumbHeight;
        int thumbY = ScrollBarBounds.Y +
                     (maximumPage == 0 ? 0 : travel * page / maximumPage);
        using var thumb = new SolidBrush(Color.FromArgb(230, 67, 169, 230));
        graphics.FillRectangle(thumb, new Rectangle(ScrollBarBounds.X + 1, thumbY,
            ScrollBarBounds.Width - 1, thumbHeight));
    }

    private static void DrawMissingIcon(Graphics graphics, Rectangle bounds)
    {
        using var background = new SolidBrush(Color.FromArgb(190, 55, 60, 68));
        using var pen = new Pen(Color.LightGray, 2);
        graphics.FillRectangle(background, bounds);
        graphics.DrawRectangle(Pens.DimGray, bounds);
        graphics.DrawLine(pen, bounds.Left + 8, bounds.Top + 8,
            bounds.Right - 8, bounds.Bottom - 8);
        graphics.DrawLine(pen, bounds.Right - 8, bounds.Top + 8,
            bounds.Left + 8, bounds.Bottom - 8);
    }

    private static void DrawText(Graphics graphics, string value, Rectangle bounds,
        Color color, float size, ContentAlignment alignment)
    {
        (StringAlignment horizontal, StringAlignment vertical) = alignment switch
        {
            ContentAlignment.MiddleCenter => (StringAlignment.Center, StringAlignment.Center),
            ContentAlignment.MiddleRight => (StringAlignment.Far, StringAlignment.Center),
            ContentAlignment.MiddleLeft => (StringAlignment.Near, StringAlignment.Center),
            _ => (StringAlignment.Near, StringAlignment.Near)
        };
        using var font = new Font("Segoe UI", size, FontStyle.Regular, GraphicsUnit.Point);
        using var brush = new SolidBrush(color);
        using var format = new StringFormat
        {
            Alignment = horizontal,
            LineAlignment = vertical,
            Trimming = StringTrimming.EllipsisCharacter
        };
        graphics.DrawString(value, font, brush, bounds, format);
    }
}
