using PangyaAPI.WFT;
using PangyaAPI.UI;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace PangYa_Suite_Tools.Shop;

internal sealed class PangyaPreviewBitmapFont : IPangyaUiBitmapTextPasses
{
    private readonly WftGlyph[] _glyphs;
    private readonly WftGlyph[]? _koreanGlyphs;
    private readonly Func<string, Image?>? _getOverlay;
    private static readonly Regex OverlayFields = new(@"(?<key>src|size|rect|scale|offset)=(?<value>.*?)(?=\s+(?:src|size|rect|scale|offset)=|$)",
        RegexOptions.CultureInvariant);
    private readonly Color?[] _colorSet = [null, Color.Red, Color.Lime, Color.Blue, null, null, null, null, null, null];
    private static readonly Encoding ShiftJis = CodePagesEncodingProvider.Instance.GetEncoding(932,
        new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?"))
        ?? throw new InvalidOperationException("Shift-JIS encoding is unavailable.");
    private static readonly Encoding Korean = CodePagesEncodingProvider.Instance.GetEncoding(949,
        new EncoderReplacementFallback("?"), new DecoderReplacementFallback("?"))
        ?? throw new InvalidOperationException("Korean code page 949 is unavailable.");
    public int Space { get; private set; } = 1;
    public bool FixedWidth { get; private set; }

    public void SetFixedWidth(bool fixedWidth) => FixedWidth = fixedWidth;

    public void SetColorSet(int index, Color color)
    {
        if (index is < 1 or > 9) throw new ArgumentOutOfRangeException(nameof(index));
        _colorSet[index] = color;
    }

    public int SetSpace(int space)
    {
        int previous = Space;
        Space = space;
        return previous;
    }

    public PangyaPreviewBitmapFont(WftFont font, CancellationToken cancellationToken, WftFont? koreanFallback = null,
        Func<string, Image?>? getOverlay = null)
    {
        _getOverlay = getOverlay;
        Height = font.CellSize;
        _glyphs = ReadGlyphs(font, cancellationToken);
        if (koreanFallback is not null) _koreanGlyphs = ReadGlyphs(koreanFallback, cancellationToken);
    }

    private static WftGlyph[] ReadGlyphs(WftFont font, CancellationToken cancellationToken)
    {
        var glyphs = new WftGlyph[font.GlyphCount];
        for (int index = 0; index < glyphs.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            glyphs[index] = font.ReadGlyph((ushort)(WftFont.FirstCodePoint + index));
        }
        return glyphs;
    }

    public int Height { get; }

    public float Measure(string text) => MeasureCore(text, forDrawing: false);

    public float GetDrawAdvance(string text) => MeasureCore(text, forDrawing: true);

    private float MeasureCore(string text, bool forDrawing)
    {
        float width = 0;
        foreach ((Rune character, _, string? overlay) in Characters(text))
        {
            if (overlay is not null) continue;
            if (character.Value == ' ')
                width += FixedWidth && !forDrawing ? Height : SpaceGlyphWidth;
            else if (FixedWidth && !forDrawing)
                width += Height;
            else if (GetGlyph(character, out bool multibyte) is { } glyph)
                width += Advance(glyph, multibyte);
        }
        return width;
    }

    public void Draw(Graphics graphics, string text, Brush brush, PointF origin)
    {
        if (text.Contains(@"\<", StringComparison.Ordinal)) DrawOverlays(graphics, text, brush, origin);
        DrawGlyphs(graphics, text, brush, origin);
    }

    public void DrawOverlays(Graphics graphics, string text, Brush brush, PointF origin, float fontScale = 1)
    {
        if (_getOverlay is not null) DrawPass(graphics, text, brush, origin, overlays: true, fontScale);
    }

    public void DrawGlyphs(Graphics graphics, string text, Brush brush, PointF origin) =>
        DrawPass(graphics, text, brush, origin, overlays: false);

    public RectangleF GetOverlayBounds(string text, PointF origin, float fontScale = 1) =>
        _getOverlay is null ? RectangleF.Empty :
            DrawPass(null, text, Brushes.White, origin, overlays: true, fontScale);

    private RectangleF DrawPass(Graphics? graphics, string text, Brush brush, PointF origin, bool overlays,
        float fontScale = 1)
    {
        Color color = brush is SolidBrush solid ? solid.Color : Color.Empty;
        using var coverageBrush = !overlays && brush is SolidBrush ? new SolidBrush(color) : null;
        RectangleF measured = RectangleF.Empty;
        // WFont sets coordinate mode 0x400. WOverlay floors the destination
        // before rendering, including half-pixel origins from centered text.
        float x = origin.X;
        float top = MathF.Floor(origin.Y);
        foreach ((Rune character, int colorIndex, string? overlay) in Characters(text))
        {
            Color glyphColor = _colorSet[colorIndex] is Color palette
                ? Color.FromArgb(color.A, palette.R * color.R >> 8,
                    palette.G * color.G >> 8, palette.B * color.B >> 8)
                : color;
            if (overlay is not null)
            {
                if (overlays)
                {
                    RectangleF destination = DrawOverlay(graphics, overlay,
                        new PointF(origin.X + (x - origin.X) * fontScale, origin.Y), glyphColor);
                    if (destination.Width > 0 && destination.Height > 0)
                        measured = measured.IsEmpty ? destination : RectangleF.Union(measured, destination);
                }
                continue;
            }
            // GetTexture handles spaces before the fixed-width branch, unlike
            // GetTextWidthInside: retain that native measurement/drawing difference.
            if (character.Value == ' ') { x += SpaceGlyphWidth; continue; }
            if (GetGlyph(character, out bool multibyte) is not { } glyph) continue;
            float advance = Advance(glyph, multibyte);
            if (!overlays) DrawGlyph(glyph, MathF.Floor(x), advance, glyphColor);
            x += advance;
        }
        return measured;

        void DrawGlyph(WftGlyph glyph, float left, float advance, Color glyphColor)
        {
            // Fresh writes whole cells at the pen position. The next glyph or
            // space overwrites the overhang, and the final texture ends at the pen.
            int visibleWidth = (int)Math.Clamp(advance, 0, Height);
            ReadOnlySpan<byte> coverage = glyph.Coverage.Span;
            for (int y = 0; y < Height; y++)
            {
                int sourceY = SampleCoordinate(y, Height, glyph.CellHeight);
                ReadOnlySpan<byte> row = coverage.Slice(sourceY * glyph.CellWidth, glyph.CellWidth);
                int column = 0;
                while (column < visibleWidth)
                {
                    byte alpha = row[SampleCoordinate(column, Height, glyph.CellWidth)];
                    if (alpha == 0)
                    {
                        column++;
                        continue;
                    }
                    int start = column++;
                    while (column < visibleWidth && row[SampleCoordinate(column, Height, glyph.CellWidth)] == alpha)
                        column++;
                    if (coverageBrush is not null)
                        coverageBrush.Color = Color.FromArgb(glyphColor.A * alpha / byte.MaxValue, glyphColor);
                    graphics?.FillRectangle(coverageBrush ?? brush, left + start, top + y, column - start, 1);
                }
            }
        }
    }

    private int SpaceGlyphWidth => Height * 14 / 36;

    private float Advance(WftGlyph glyph, bool multibyte)
    {
        int width = (glyph.AdvanceWidth * Height + glyph.CellWidth / 2) / glyph.CellWidth;
        return FixedWidth ? Height : (float)((long)Space + width - (multibyte ? 1 : 0));
    }

    private static int SampleCoordinate(int pixel, int destinationSize, int sourceSize) =>
        (2 * pixel + 1) * sourceSize / (2 * destinationSize);

    private RectangleF DrawOverlay(Graphics? graphics, string markup, PointF pen, Color color)
    {
        if (_getOverlay is null || markup.Length > 1023) return RectangleF.Empty;
        var fields = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match match in OverlayFields.Matches(markup))
            fields.TryAdd(match.Groups["key"].Value, match.Groups["value"].Value.Trim());
        string? filename = fields.GetValueOrDefault("src");
        if (string.IsNullOrWhiteSpace(filename) || filename.Any(char.IsWhiteSpace) || Path.IsPathRooted(filename))
            return RectangleF.Empty;

        float[] size = ReadNumbers("size", [256, 256]);
        float[] rect = ReadNumbers("rect", [0, 0, size[0], size[1]]);
        float scale = ReadNumbers("scale", [1])[0];
        float[] offset = ReadNumbers("offset", [0, 0]);
        if (size[0] <= 0 || size[1] <= 0 || rect[2] <= 0 || rect[3] <= 0 || scale <= 0 ||
            _getOverlay(filename) is not Image image) return RectangleF.Empty;

        var source = new RectangleF(rect[0] / size[0] * image.Width, rect[1] / size[1] * image.Height,
            rect[2] / size[0] * image.Width, rect[3] / size[1] * image.Height);
        float x = MathF.Floor(pen.X + offset[0]), y = MathF.Floor(pen.Y + offset[1]);
        float right = x + rect[2] * scale, bottom = y + rect[3] * scale;
        if (!float.IsFinite(right) || !float.IsFinite(bottom) || !float.IsFinite(source.Right) ||
            !float.IsFinite(source.Bottom)) return RectangleF.Empty;
        var destination = RectangleF.FromLTRB(x, y, right, bottom);
        if (graphics is null) return destination;

        using var attributes = new ImageAttributes();
        attributes.SetColorMatrix(new ColorMatrix
        {
            Matrix00 = color.R / 255f, Matrix11 = color.G / 255f,
            Matrix22 = color.B / 255f, Matrix33 = color.A / 255f
        });
        GraphicsState state = graphics.Save();
        try
        {
            graphics.PixelOffsetMode = PixelOffsetMode.Half;
            PointF[] corners = [new(x, y), new(right, y), new(x, bottom)];
            graphics.DrawImage(image, corners,
                source, GraphicsUnit.Pixel, attributes);
        }
        finally { graphics.Restore(state); }
        return destination;

        float[] ReadNumbers(string key, float[] defaults)
        {
            if (!fields.TryGetValue(key, out string? value)) return defaults;
            string[] parts = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != defaults.Length) return defaults;
            var numbers = new float[defaults.Length];
            for (int index = 0; index < numbers.Length; index++)
                if (!float.TryParse(parts[index], NumberStyles.Float, CultureInfo.InvariantCulture,
                        out numbers[index]) || !float.IsFinite(numbers[index]))
                    return defaults;
            return numbers;
        }
    }

    private static IEnumerable<(Rune Character, int ColorIndex, string? Overlay)> Characters(string text)
    {
        int end = text.IndexOf('\0');
        if (end < 0) end = text.Length;
        int colorIndex = 0;
        for (int index = 0; index < end;)
        {
            if (text[index] == '\\')
            {
                if (++index == end) yield break;
                char next = text[index];
                if (next is >= '0' and <= '9')
                {
                    colorIndex = next - '0';
                    index++;
                }
                else if (next == '<' && text.IndexOf('>', index + 1, end - index - 1) is int close && close >= 0)
                {
                    yield return (default, colorIndex, text[(index + 1)..close]);
                    index = close + 1;
                }
                continue;
            }
            if (!Rune.TryGetRuneAt(text, index, out Rune character)) character = Rune.ReplacementChar;
            yield return (character, colorIndex, null);
            index += character.Utf16SequenceLength;
        }
    }

    private WftGlyph? GetGlyph(Rune character, out bool multibyte)
    {
        Span<char> unicode = stackalloc char[2];
        // Code-page fallback can emit one '?' per UTF-16 surrogate. Replace an
        // unsupported supplementary rune once so it cannot look like a two-byte glyph.
        int chars = (character.IsBmp ? character : new Rune('?')).EncodeToUtf16(unicode);
        Span<byte> bytes = stackalloc byte[8];
        int count = ShiftJis.GetBytes(unicode[..chars], bytes);
        WftGlyph[] glyphs = _glyphs;
        if (_koreanGlyphs is not null && count == 1 && bytes[0] == '?' && character.IsBmp && character.Value != '?')
        {
            count = Korean.GetBytes(unicode[..chars], bytes);
            if (count == 2) glyphs = _koreanGlyphs;
        }
        multibyte = count == 2;
        int code = multibyte ? bytes[0] << 8 | bytes[1] : bytes[0];
        if (code < 0x20) code = '$';
        int index = code - WftFont.FirstCodePoint;
        return (uint)index < (uint)glyphs.Length ? glyphs[index] : null;
    }
}
