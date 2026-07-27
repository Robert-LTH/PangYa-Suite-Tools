using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;

namespace PangyaAPI.WFT;

public sealed class WftFont : IDisposable
{
    public const ushort FirstCodePoint = 0x0020;
    public const ushort LegacyLastCodePoint = 0xFFFE;
    public const ushort MaximumCodePoint = 0xFFFF;
    public const int LegacyGlyphCount = LegacyLastCodePoint - FirstCodePoint + 1;
    public const int MaximumGlyphCount = MaximumCodePoint - FirstCodePoint + 1;
    private const int HeaderSize = 16;

    private readonly SafeFileHandle _handle;
    private readonly int _rowStride;
    private readonly int _bitmapByteCount;
    private readonly int _recordSize;
    private bool _disposed;

    internal WftFont(string path, SafeFileHandle handle, int cellSize, WftCoverageMode coverageMode,
        uint reserved, int glyphCount, int rowStride, int bitmapByteCount, int recordSize)
    {
        Path = path;
        _handle = handle;
        CellSize = cellSize;
        CoverageMode = coverageMode;
        Reserved = reserved;
        GlyphCount = glyphCount;
        _rowStride = rowStride;
        _bitmapByteCount = bitmapByteCount;
        _recordSize = recordSize;
    }

    public string Path { get; }
    public int CellSize { get; }
    public WftCoverageMode CoverageMode { get; }
    public uint Reserved { get; }
    public int GlyphCount { get; }
    public ushort LastCodePoint => checked((ushort)(FirstCodePoint + GlyphCount - 1));

    public WftGlyph ReadGlyph(ushort codePoint)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (codePoint < FirstCodePoint || codePoint > LastCodePoint)
            throw new ArgumentOutOfRangeException(nameof(codePoint),
                $"WFT glyphs range from U+{FirstCodePoint:X4} through U+{LastCodePoint:X4}.");

        long recordOffset = checked(HeaderSize +
            (long)(codePoint - FirstCodePoint) * _recordSize);
        byte[] record = GC.AllocateUninitializedArray<byte>(_recordSize);
        WftFontReader.ReadExactly(_handle, record, recordOffset);

        byte[] coverage = GC.AllocateUninitializedArray<byte>(checked(CellSize * CellSize));
        for (int y = 0; y < CellSize; y++)
        {
            ReadOnlySpan<byte> row = record.AsSpan(y * _rowStride, _rowStride);
            int target = y * CellSize;
            for (int x = 0; x < CellSize; x++)
            {
                coverage[target + x] = CoverageMode == WftCoverageMode.Antialiased
                    ? (byte)(((x & 1) == 0 ? row[x >> 1] >> 4 : row[x >> 1] & 0x0F) * 17)
                    : (byte)((row[x >> 3] & (1 << (7 - (x & 7)))) != 0 ? 255 : 0);
            }
        }

        ushort advance = BinaryPrimitives.ReadUInt16LittleEndian(
            record.AsSpan(_bitmapByteCount, sizeof(ushort)));
        return new WftGlyph(codePoint, CellSize, advance, coverage);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _handle.Dispose();
    }
}
