using PangyaAPI.WFT;
using System.Collections.Concurrent;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace PangYa_Suite_Tools;

internal static class WftGlyphRenderer
{
    public static Bitmap CreateBitmap(WftGlyph glyph, Color color)
    {
        var bitmap = new Bitmap(glyph.CellWidth, glyph.CellHeight, PixelFormat.Format32bppArgb);
        BitmapData data = bitmap.LockBits(
            new Rectangle(0, 0, bitmap.Width, bitmap.Height),
            ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            byte[] pixels = GC.AllocateUninitializedArray<byte>(checked(data.Stride * data.Height));
            ReadOnlySpan<byte> coverage = glyph.Coverage.Span;
            for (int y = 0; y < glyph.CellHeight; y++)
            {
                for (int x = 0; x < glyph.CellWidth; x++)
                {
                    int target = y * data.Stride + x * 4;
                    pixels[target] = color.B;
                    pixels[target + 1] = color.G;
                    pixels[target + 2] = color.R;
                    pixels[target + 3] = (byte)(coverage[y * glyph.CellWidth + x] * color.A / 255);
                }
            }
            Marshal.Copy(pixels, 0, data.Scan0, pixels.Length);
        }
        finally
        {
            bitmap.UnlockBits(data);
        }
        return bitmap;
    }

    public static Bitmap RenderText(WftFont font, string text, int scale,
        Color foreground, Color background, CancellationToken cancellationToken)
    {
        scale = Math.Clamp(scale, 1, 16);
        text = text.Length > 2048 ? text[..2048] : text;
        var glyphs = new Dictionary<ushort, WftGlyph>();
        WftGlyph Glyph(ushort codePoint)
        {
            if (!glyphs.TryGetValue(codePoint, out WftGlyph? glyph))
            {
                cancellationToken.ThrowIfCancellationRequested();
                glyph = font.ReadGlyph(codePoint);
                glyphs.Add(codePoint, glyph);
            }
            return glyph;
        }

        int spaceAdvance = Math.Max(1, (int)Glyph(WftFont.FirstCodePoint).AdvanceWidth);
        var positions = new List<(WftGlyph Glyph, int X, int Y)>();
        int x = 0;
        int y = 0;
        int maximumX = 1;
        foreach (char character in text)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (character == '\r') continue;
            if (character == '\n')
            {
                maximumX = Math.Max(maximumX, x);
                x = 0;
                y += font.CellSize;
                continue;
            }
            if (character == '\t')
            {
                int tabWidth = spaceAdvance * 4;
                x = checked(((x / tabWidth) + 1) * tabWidth);
                continue;
            }
            if (character < WftFont.FirstCodePoint || character > font.LastCodePoint)
                continue;
            WftGlyph glyph = Glyph(character);
            positions.Add((glyph, x, y));
            x = checked(x + Math.Max(1, (int)glyph.AdvanceWidth));
        }
        maximumX = Math.Max(maximumX, x);
        int logicalHeight = checked(y + font.CellSize);
        int width = Math.Clamp(checked(maximumX * scale), 1, 8192);
        int height = Math.Clamp(checked(logicalHeight * scale), 1, 8192);
        var result = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using Graphics graphics = Graphics.FromImage(result);
        graphics.Clear(background);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        foreach ((WftGlyph glyph, int glyphX, int glyphY) in positions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using Bitmap glyphBitmap = CreateBitmap(glyph, foreground);
            var destination = new Rectangle(glyphX * scale, glyphY * scale,
                glyph.CellWidth * scale, glyph.CellHeight * scale);
            graphics.DrawImage(glyphBitmap, destination);
        }
        return result;
    }
}

internal sealed class WftBitmapCache(int capacity) : IDisposable
{
    private readonly int _capacity = capacity;
    private readonly Dictionary<ushort, CacheEntry> _entries = [];
    private long _stamp;

    public bool TryGet(ushort codePoint, out Bitmap? bitmap)
    {
        if (_entries.TryGetValue(codePoint, out CacheEntry? entry))
        {
            entry.Stamp = ++_stamp;
            bitmap = entry.Bitmap;
            return true;
        }
        bitmap = null;
        return false;
    }

    public void Add(ushort codePoint, Bitmap bitmap)
    {
        if (_entries.Remove(codePoint, out CacheEntry? existing)) existing.Bitmap.Dispose();
        _entries.Add(codePoint, new CacheEntry(bitmap, ++_stamp));
        if (_entries.Count <= _capacity) return;
        KeyValuePair<ushort, CacheEntry> oldest = _entries.MinBy(pair => pair.Value.Stamp);
        _entries.Remove(oldest.Key);
        oldest.Value.Bitmap.Dispose();
    }

    public void Clear()
    {
        foreach (CacheEntry entry in _entries.Values) entry.Bitmap.Dispose();
        _entries.Clear();
    }

    public void Dispose() => Clear();

    private sealed class CacheEntry(Bitmap bitmap, long stamp)
    {
        public Bitmap Bitmap { get; } = bitmap;
        public long Stamp { get; set; } = stamp;
    }
}

internal sealed class WftGlyphGrid : ScrollableControl
{
    private const int CellWidth = 78;
    private const int CellHeight = 72;
    private readonly WftBitmapCache _cache = new(256);
    private readonly ConcurrentDictionary<ushort, byte> _pending = new();
    private readonly SemaphoreSlim _decodeSlots = new(4);
    private WftFont? _wftFont;
    private int _generation;
    private ushort? _selectedCodePoint;

    public WftGlyphGrid()
    {
        Name = "wftGlyphGrid";
        AutoScroll = true;
        DoubleBuffered = true;
        BackColor = Color.FromArgb(31, 34, 38);
        TabStop = true;
    }

    public event EventHandler<ushort>? GlyphSelected;

    public ushort? SelectedCodePoint
    {
        get => _selectedCodePoint;
        private set
        {
            if (_selectedCodePoint == value) return;
            _selectedCodePoint = value;
            Invalidate();
        }
    }

    public void LoadFont(WftFont? font)
    {
        _generation++;
        _wftFont = font;
        _pending.Clear();
        _cache.Clear();
        SelectedCodePoint = null;
        UpdateScrollArea();
        Invalidate();
    }

    public void SelectCodePoint(ushort codePoint)
    {
        if (_wftFont is null || codePoint < WftFont.FirstCodePoint ||
            codePoint > _wftFont.LastCodePoint)
            return;
        SelectedCodePoint = codePoint;
        int columns = ColumnCount;
        int index = codePoint - WftFont.FirstCodePoint;
        int row = index / columns;
        AutoScrollPosition = new Point(0, row * CellHeight);
        GlyphSelected?.Invoke(this, codePoint);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateScrollArea();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (_wftFont is null || e.Button != MouseButtons.Left) return;
        Point logical = new(e.X - AutoScrollPosition.X, e.Y - AutoScrollPosition.Y);
        int column = logical.X / CellWidth;
        int row = logical.Y / CellHeight;
        int index = row * ColumnCount + column;
        if (column < 0 || column >= ColumnCount || index < 0 || index >= _wftFont.GlyphCount)
            return;
        SelectCodePoint((ushort)(WftFont.FirstCodePoint + index));
        Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_wftFont is null) return;
        e.Graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y);
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        int columns = ColumnCount;
        int firstRow = Math.Max(0, -AutoScrollPosition.Y / CellHeight);
        int lastRow = Math.Min((_wftFont.GlyphCount - 1) / columns,
            (-AutoScrollPosition.Y + ClientSize.Height) / CellHeight + 1);
        using var border = new Pen(Color.FromArgb(70, 78, 86));
        using var selected = new Pen(Color.DeepSkyBlue, 2);
        using var textBrush = new SolidBrush(Color.Gainsboro);
        using var glyphBackground = new SolidBrush(Color.FromArgb(16, 18, 20));
        for (int row = firstRow; row <= lastRow; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                int index = row * columns + column;
                if (index >= _wftFont.GlyphCount) break;
                ushort codePoint = (ushort)(WftFont.FirstCodePoint + index);
                var cell = new Rectangle(column * CellWidth, row * CellHeight, CellWidth, CellHeight);
                e.Graphics.DrawRectangle(SelectedCodePoint == codePoint ? selected : border, cell);
                var imageBounds = new Rectangle(cell.X + 23, cell.Y + 4, 32, 32);
                e.Graphics.FillRectangle(glyphBackground, imageBounds);
                if (_cache.TryGet(codePoint, out Bitmap? bitmap) && bitmap is not null)
                    e.Graphics.DrawImage(bitmap, imageBounds);
                else
                    QueueDecode(codePoint, _wftFont, _generation);
                string character = char.IsControl((char)codePoint) ||
                                   char.IsSurrogate((char)codePoint) ||
                                   codePoint >= 0xFFFE
                    ? "·"
                    : ((char)codePoint).ToString();
                e.Graphics.DrawString($"{character}  U+{codePoint:X4}", Font, textBrush,
                    new RectangleF(cell.X + 4, cell.Y + 43, CellWidth - 8, 24));
            }
        }
    }

    private int ColumnCount => Math.Max(1, ClientSize.Width / CellWidth);

    private void UpdateScrollArea()
    {
        int rows = ((_wftFont?.GlyphCount ?? 0) + ColumnCount - 1) / ColumnCount;
        AutoScrollMinSize = _wftFont is null ? Size.Empty : new Size(0, rows * CellHeight);
    }

    private void QueueDecode(ushort codePoint, WftFont font, int generation)
    {
        if (!_pending.TryAdd(codePoint, 0)) return;
        _ = Task.Run(async () =>
        {
            Bitmap? bitmap = null;
            try
            {
                await _decodeSlots.WaitAsync().ConfigureAwait(false);
                try
                {
                    bitmap = WftGlyphRenderer.CreateBitmap(font.ReadGlyph(codePoint), Color.White);
                }
                finally
                {
                    _decodeSlots.Release();
                }
                if (IsDisposed || generation != _generation)
                {
                    bitmap.Dispose();
                    return;
                }
                BeginInvoke(() =>
                {
                    if (generation == _generation)
                    {
                        _cache.Add(codePoint, bitmap);
                        bitmap = null;
                        Invalidate();
                    }
                });
            }
            catch (Exception ex) when (ex is IOException or ObjectDisposedException or
                                       InvalidDataException or InvalidOperationException)
            {
                bitmap?.Dispose();
            }
            finally
            {
                _pending.TryRemove(codePoint, out _);
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _generation++;
            _cache.Dispose();
            _decodeSlots.Dispose();
        }
        base.Dispose(disposing);
    }
}

internal sealed class NearestNeighborPictureBox : Control
{
    private Image? _image;

    public NearestNeighborPictureBox()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(20, 22, 25);
    }

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(
        System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public Image? Image
    {
        get => _image;
        set
        {
            if (ReferenceEquals(_image, value)) return;
            _image?.Dispose();
            _image = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_image is null) return;
        int scale = Math.Max(1, Math.Min(ClientSize.Width / _image.Width,
            ClientSize.Height / _image.Height));
        var destination = new Rectangle(
            (ClientSize.Width - _image.Width * scale) / 2,
            (ClientSize.Height - _image.Height * scale) / 2,
            _image.Width * scale, _image.Height * scale);
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        e.Graphics.DrawImage(_image, destination);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image?.Dispose();
            _image = null;
        }
        base.Dispose(disposing);
    }
}
