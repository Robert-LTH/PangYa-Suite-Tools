using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace PangyaAPI.UI;

public sealed record ShopLayoutElement(string Type, string Name, Rectangle Bounds,
    IReadOnlyDictionary<string, string> Parameters);

public sealed record ShopLayout(Size Size, IReadOnlyList<ShopLayoutElement> Elements);

public static class ShopLayoutParser
{
    private const int MaximumElements = 2048;

    public static ShopLayout Load(string shopXmlPath, string predefinedXmlPath)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        XmlDocument shop = LoadDocument(shopXmlPath);
        XmlDocument predefined = LoadDocument(predefinedXmlPath);
        XmlElement form = FindShopMainForm(shop)
            ?? throw new InvalidDataException(CreateMissingShopMainMessage(shop));
        Size size = ParseFormSize(form);
        if (size.Width <= 0 || size.Height <= 0 || size.Width > 4096 || size.Height > 4096)
            throw new InvalidDataException("The shop form dimensions are invalid.");

        var result = new List<ShopLayoutElement>();
        AddItems(form, shop, predefined, result, []);
        if (result.Count > MaximumElements)
            throw new InvalidDataException("The expanded shop layout contains too many elements.");
        return new ShopLayout(size, result);
    }

    private static XmlDocument LoadDocument(string path)
    {
        var info = new FileInfo(path);
        if (!info.Exists) throw new FileNotFoundException(
            "The PangYa shop XML file was not found.", path);
        if (info.Length > 4 * 1024 * 1024)
            throw new InvalidDataException("The PangYa shop XML file is too large.");
        Encoding encoding = PangyaXmlCompatibility.DetectEncoding(path);
        string xml = PangyaXmlCompatibility.EscapeBareAmpersands(
            File.ReadAllText(path, encoding));
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            MaxCharactersInDocument = 4 * 1024 * 1024,
        };
        using var textReader = new StringReader(xml);
        using XmlReader reader = XmlReader.Create(textReader, settings, path);
        var document = new XmlDocument { XmlResolver = null };
        document.Load(reader);
        return document;
    }

    private static void AddItems(XmlElement owner, XmlDocument shop, XmlDocument predefined,
                                 List<ShopLayoutElement> output, HashSet<string> macroStack)
    {
        foreach (XmlElement item in ChildElements(owner, "item"))
        {
            string type = GetAttribute(item, "type");
            if (type.Equals("MACROITEM", StringComparison.OrdinalIgnoreCase))
            {
                string resource = GetAttribute(item, "resource");
                if (string.IsNullOrWhiteSpace(resource))
                    continue;

                XmlElement? macro = FindMacro(predefined, shop, resource);
                if (macro == null)
                    continue;

                if (!macroStack.Add(resource))
                    throw new InvalidDataException($"The layout macro '{resource}' contains a recursive reference.");
                AddItems(macro, shop, predefined, output, macroStack);
                macroStack.Remove(resource);
                continue;
            }

            Rectangle bounds = ParseBounds(item);
            var parameters = ChildElements(item, "param")
                .Where(param => HasAttribute(param, "name"))
                .GroupBy(param => GetAttribute(param, "name"), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => GetAttribute(group.Last(), "var"), StringComparer.OrdinalIgnoreCase);
            output.Add(new ShopLayoutElement(type, GetAttribute(item, "name"), bounds, parameters));
        }
    }

    private static XmlElement? FindMacro(XmlDocument predefined, XmlDocument shop, string resource) =>
        FindLayoutDefinition(predefined, resource) ?? FindLayoutDefinition(shop, resource);

    private static XmlElement? FindShopMainForm(XmlDocument document)
    {
        XmlElement[] matches = DescendantElements(document.DocumentElement)
            .Where(IsShopMainElement)
            .ToArray();

        return matches.FirstOrDefault(IsExplicitFormElement)
            ?? matches.FirstOrDefault(element => !HasExplicitNonFormType(element))
            ?? matches.FirstOrDefault();
    }

    private static XmlElement? FindElement(XmlDocument document, string name, string? type = null) =>
        DescendantElements(document.DocumentElement)
            .Where(element => element.LocalName.Equals("element", StringComparison.OrdinalIgnoreCase))
            .FirstOrDefault(element =>
                GetAttribute(element, "name").Equals(name, StringComparison.OrdinalIgnoreCase) &&
                (type == null || GetAttribute(element, "type").Equals(type, StringComparison.OrdinalIgnoreCase)));

    private static XmlElement? FindLayoutDefinition(XmlDocument document, string name) =>
        DescendantElements(document.DocumentElement)
            .FirstOrDefault(element =>
                element.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                GetAttribute(element, "name").Equals(name, StringComparison.OrdinalIgnoreCase) ||
                GetAttribute(element, "id").Equals(name, StringComparison.OrdinalIgnoreCase));

    private static bool IsShopMainElement(XmlElement element) =>
        element.LocalName.Equals("shopmain", StringComparison.OrdinalIgnoreCase) ||
        AttributeValueEquals(element, "name", "shopmain") ||
        AttributeValueEquals(element, "id", "shopmain") ||
        AttributeValueEquals(element, "form", "shopmain") ||
        element.Attributes.OfType<XmlAttribute>()
            .Any(attribute => attribute.Value.Equals("shopmain", StringComparison.OrdinalIgnoreCase));

    private static bool IsExplicitFormElement(XmlElement element) =>
        element.LocalName.Equals("form", StringComparison.OrdinalIgnoreCase) ||
        GetAttribute(element, "type").Equals("FORM", StringComparison.OrdinalIgnoreCase);

    private static bool HasExplicitNonFormType(XmlElement element) =>
        HasAttribute(element, "type") &&
        !GetAttribute(element, "type").Equals("FORM", StringComparison.OrdinalIgnoreCase);

    private static bool AttributeValueEquals(XmlElement element, string attributeName, string expected) =>
        GetAttribute(element, attributeName).Equals(expected, StringComparison.OrdinalIgnoreCase);

    private static bool IsBaseElement(XmlElement element) =>
        element.LocalName.Equals("base", StringComparison.OrdinalIgnoreCase) ||
        AttributeValueEquals(element, "name", "base") ||
        AttributeValueEquals(element, "id", "base") ||
        AttributeValueEquals(element, "type", "base");

    private static IEnumerable<XmlElement> ChildElements(XmlElement owner, string localName) =>
        owner.ChildNodes
            .OfType<XmlElement>()
            .Where(element => element.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase));

    private static IEnumerable<XmlElement> DescendantElements(XmlElement? root)
    {
        if (root == null) yield break;

        yield return root;
        foreach (XmlElement child in root.ChildNodes.OfType<XmlElement>())
        {
            foreach (XmlElement descendant in DescendantElements(child))
                yield return descendant;
        }
    }

    private static Rectangle ParseBounds(XmlElement item)
    {
        if (HasAttribute(item, "rect"))
        {
            int[] values = ParseNumbers(GetAttribute(item, "rect"), 4, $"{GetAttribute(item, "name")} rect");
            if (values[2] < values[0] || values[3] < values[1]) throw new InvalidDataException("A shop rectangle is inverted.");
            return Rectangle.FromLTRB(values[0], values[1], values[2], values[3]);
        }
        if (HasAttribute(item, "pos"))
        {
            Size point = ParsePair(GetAttribute(item, "pos"), $"{GetAttribute(item, "name")} pos");
            return new Rectangle(point.Width, point.Height, 0, 0);
        }
        return Rectangle.Empty;
    }

    private static Size ParseFormSize(XmlElement form)
    {
        if (TryParseElementSize(form, "shopmain", out Size formSize))
            return formSize;

        foreach (XmlElement baseElement in DescendantElements(form).Skip(1).Where(IsBaseElement))
        {
            if (TryParseElementSize(baseElement, "shopmain base", out Size baseSize))
                return baseSize;
        }

        string described = string.Join(", ", form.Attributes.OfType<XmlAttribute>()
            .Select(attribute => $"{attribute.Name}='{attribute.Value}'"));
        throw new InvalidDataException(
            string.IsNullOrWhiteSpace(described)
                ? "Invalid shopmain size: no size, width/height, rect, or inline base element was found."
                : $"Invalid shopmain size: no usable size, width/height, rect, or inline base element was found on shopmain ({described}).");
    }

    private static bool TryParseElementSize(XmlElement element, string label, out Size size)
    {
        if (HasAttribute(element, "size"))
        {
            size = ParsePair(GetAttribute(element, "size"), $"{label} size");
            return true;
        }

        string width = FirstAttribute(element, "width", "w", "cx");
        string height = FirstAttribute(element, "height", "h", "cy");
        if (!string.IsNullOrWhiteSpace(width) || !string.IsNullOrWhiteSpace(height))
        {
            if (int.TryParse(width, out int parsedWidth) &&
                int.TryParse(height, out int parsedHeight))
            {
                size = new Size(parsedWidth, parsedHeight);
                return true;
            }

            throw new InvalidDataException($"Invalid {label} size: width='{width}', height='{height}'.");
        }

        string rect = FirstAttribute(element, "rect", "bounds");
        if (!string.IsNullOrWhiteSpace(rect))
        {
            int[] values = ParseNumbers(rect, 4, $"{label} rect");
            if (values[2] < values[0] || values[3] < values[1])
                throw new InvalidDataException($"The {label} rectangle is inverted.");
            size = new Size(values[2] - values[0], values[3] - values[1]);
            return true;
        }

        if (!string.IsNullOrWhiteSpace(element.InnerText) && !ChildElements(element, "item").Any())
        {
            int[] values = ParseNumbers(element.InnerText, 2, $"{label} text size");
            size = new Size(values[0], values[1]);
            return true;
        }

        size = Size.Empty;
        return false;
    }

    private static Size ParsePair(string value, string label)
    {
        int[] numbers = ParseNumbers(value, 2, label);
        return new Size(numbers[0], numbers[1]);
    }

    private static int[] ParseNumbers(string value, int count, string label)
    {
        int[] numbers = Regex.Matches(value, @"-?\d+")
            .Select(match => int.Parse(match.Value))
            .ToArray();
        if (numbers.Length != count)
            throw new InvalidDataException($"Invalid {label}: '{value}'.");
        return numbers;
    }

    private static bool HasAttribute(XmlElement element, string name) =>
        element.Attributes.OfType<XmlAttribute>()
            .Any(attribute => attribute.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static string GetAttribute(XmlElement element, string name) =>
        element.Attributes.OfType<XmlAttribute>()
            .FirstOrDefault(attribute => attribute.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))
            ?.Value ?? string.Empty;

    private static string FirstAttribute(XmlElement element, params string[] names)
    {
        foreach (string name in names)
        {
            string value = GetAttribute(element, name);
            if (!string.IsNullOrWhiteSpace(value)) return value;
        }

        return string.Empty;
    }

    private static string CreateMissingShopMainMessage(XmlDocument document)
    {
        string[] candidates = DescendantElements(document.DocumentElement)
            .Where(element => element.LocalName.Equals("form", StringComparison.OrdinalIgnoreCase) ||
                              GetAttribute(element, "type").Equals("FORM", StringComparison.OrdinalIgnoreCase))
            .Select(DescribeElement)
            .Take(8)
            .ToArray();

        return candidates.Length == 0
            ? "shop.xml does not contain the shopmain form. No FORM elements were found."
            : $"shop.xml does not contain the shopmain form. FORM candidates found: {string.Join(", ", candidates)}.";
    }

    private static string DescribeElement(XmlElement element)
    {
        string name = GetAttribute(element, "name");
        string id = GetAttribute(element, "id");
        string type = GetAttribute(element, "type");
        string identifier = string.IsNullOrWhiteSpace(name)
            ? string.IsNullOrWhiteSpace(id) ? element.LocalName : $"id='{id}'"
            : $"name='{name}'";
        return string.IsNullOrWhiteSpace(type) ? $"<{element.LocalName} {identifier}>" : $"<{element.LocalName} type='{type}' {identifier}>";
    }

}

public sealed class ShopCatalogItem
{
    public ShopCatalogItem(string category, uint itemId, string name, string iconId, uint price,
        uint discountPrice, uint rentalPrice, bool isCash, string iconPath, string entryName = "", int recordIndex = -1,
        byte shopFlags = 0, byte moneyFlags = 0, byte timeFlag = 0, byte time = 0,
        DateTime? startDate = null, DateTime? endDate = null)
    {
        Category = category; ItemId = itemId; Name = name; IconId = iconId; Price = price;
        DiscountPrice = discountPrice; RentalPrice = rentalPrice; IsCash = isCash;
        IconPath = iconPath; EntryName = entryName; RecordIndex = recordIndex; ShopFlags = shopFlags;
        MoneyFlags = moneyFlags; TimeFlag = timeFlag; Time = time; StartDate = startDate; EndDate = endDate;
    }
    public string Category { get; }
    public uint ItemId { get; }
    public string Name { get; }
    public string IconId { get; set; }
    public uint Price { get; set; }
    public uint DiscountPrice { get; set; }
    public uint RentalPrice { get; set; }
    public bool IsCash { get; }
    public string IconPath { get; set; }
    public string EntryName { get; }
    public int RecordIndex { get; }
    public byte ShopFlags { get; set; }
    public byte MoneyFlags { get; set; }
    public byte TimeFlag { get; set; }
    public byte Time { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public uint PurchasePrice => DiscountPrice != 0 ? DiscountPrice : Price;
}

public sealed class ShopSession
{
    private readonly List<ShopCatalogItem> _cart = [];
    public ulong Pang { get; private set; } = 1_000_000;
    public ulong Cookies { get; private set; } = 10_000;
    public IReadOnlyList<ShopCatalogItem> Cart => _cart;
    public void Add(ShopCatalogItem item) => _cart.Add(item);
    public void Clear() => _cart.Clear();
    public (ulong Pang, ulong Cookies) Totals(bool rental) =>
        (_cart.Where(item => !item.IsCash).Aggregate(0UL, (total, item) => total + (rental && item.RentalPrice != 0 ? item.RentalPrice : item.PurchasePrice)),
         _cart.Where(item => item.IsCash).Aggregate(0UL, (total, item) => total + (rental && item.RentalPrice != 0 ? item.RentalPrice : item.PurchasePrice)));
    public bool TryCheckout(bool rental)
    {
        (ulong pang, ulong cookies) = Totals(rental);
        if (pang > Pang || cookies > Cookies) return false;
        Pang -= pang;
        Cookies -= cookies;
        _cart.Clear();
        return true;
    }
}
