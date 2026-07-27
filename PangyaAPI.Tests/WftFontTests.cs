using PangyaAPI.WFT;
using System.Buffers.Binary;

namespace PangyaAPI.Tests;

public sealed class WftFontTests : IDisposable
{
    private readonly TemporaryDirectory _directory = new();

    [Fact]
    public void Reader_DecodesMonochromeGlyphsAndBoundaryOffsets()
    {
        string path = CreateFont(WftCoverageMode.Monochrome, 2, 0x12345678,
            (WftFont.FirstCodePoint, [0b1000_0000, 0b0100_0000], 3),
            (WftFont.MaximumCodePoint, [0b1100_0000, 0], 4));

        using WftFont font = WftFontReader.Open(path);

        Assert.Equal(2, font.CellSize);
        Assert.Equal(WftCoverageMode.Monochrome, font.CoverageMode);
        Assert.Equal(0x12345678u, font.Reserved);
        Assert.Equal(WftFont.MaximumGlyphCount, font.GlyphCount);
        Assert.Equal(WftFont.MaximumCodePoint, font.LastCodePoint);
        WftGlyph first = font.ReadGlyph(WftFont.FirstCodePoint);
        Assert.Equal((ushort)3, first.AdvanceWidth);
        Assert.Equal([255, 0, 0, 255], first.Coverage.ToArray());
        Assert.Equal([255, 255, 0, 0],
            font.ReadGlyph(WftFont.MaximumCodePoint).Coverage.ToArray());
    }

    [Fact]
    public void Reader_AcceptsLegacyRangeEndingAtFffe()
    {
        string path = CreateFont(WftCoverageMode.Monochrome, 1, 0,
            WftFont.LegacyGlyphCount);

        using WftFont font = WftFontReader.Open(path);

        Assert.Equal(WftFont.LegacyGlyphCount, font.GlyphCount);
        Assert.Equal(WftFont.LegacyLastCodePoint, font.LastCodePoint);
        Assert.Equal((ushort)0, font.ReadGlyph(WftFont.LegacyLastCodePoint).AdvanceWidth);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            font.ReadGlyph(WftFont.MaximumCodePoint));
    }

    [Fact]
    public void Reader_DecodesFourBitCoverage()
    {
        string path = CreateFont(WftCoverageMode.Antialiased, 2, 0,
            (0x0041, [0xF8, 0x40], 2));

        using WftFont font = WftFontReader.Open(path);
        WftGlyph glyph = font.ReadGlyph(0x0041);

        Assert.Equal([255, 136, 68, 0], glyph.Coverage.ToArray());
        Assert.Equal((ushort)2, glyph.AdvanceWidth);
    }

    [Theory]
    [InlineData("signature")]
    [InlineData("mode")]
    [InlineData("size")]
    [InlineData("truncated")]
    [InlineData("trailing")]
    public void Reader_RejectsInvalidFiles(string corruption)
    {
        string path = CreateFont(WftCoverageMode.Monochrome, 1, 0);
        byte[] bytes = File.ReadAllBytes(path);
        switch (corruption)
        {
            case "signature":
                bytes[0] = 0;
                break;
            case "mode":
                BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8), 2);
                break;
            case "size":
                BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), 0);
                break;
            case "truncated":
                Array.Resize(ref bytes, bytes.Length - 1);
                break;
            case "trailing":
                Array.Resize(ref bytes, bytes.Length + 1);
                break;
        }
        File.WriteAllBytes(path, bytes);

        Assert.Throws<InvalidDataException>(() => WftFontReader.Open(path));
    }

    [Fact]
    public void Reader_RejectsOutOfRangeGlyphsAndReadsAfterSourceRename()
    {
        string path = CreateFont(WftCoverageMode.Monochrome, 1, 0);
        using WftFont font = WftFontReader.Open(path);
        string renamed = path + ".renamed";
        File.Move(path, renamed);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            font.ReadGlyph((ushort)(WftFont.FirstCodePoint - 1)));
        Assert.Equal((ushort)0, font.ReadGlyph(WftFont.FirstCodePoint).AdvanceWidth);
    }

    private string CreateFont(WftCoverageMode mode, int cellSize, uint reserved,
        params (ushort CodePoint, byte[] Bitmap, ushort Advance)[] glyphs)
        => CreateFont(mode, cellSize, reserved, WftFont.MaximumGlyphCount, glyphs);

    private string CreateFont(WftCoverageMode mode, int cellSize, uint reserved, int glyphCount,
        params (ushort CodePoint, byte[] Bitmap, ushort Advance)[] glyphs)
    {
        int bitsPerPixel = mode == WftCoverageMode.Antialiased ? 4 : 1;
        int rowStride = (cellSize * bitsPerPixel + 7) / 8;
        int bitmapBytes = rowStride * cellSize;
        int recordSize = bitmapBytes + sizeof(ushort);
        long length = 16L + (long)recordSize * glyphCount;
        string path = System.IO.Path.Combine(_directory.Path, Guid.NewGuid() + ".wft");
        using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write,
            FileShare.None);
        stream.SetLength(length);
        Span<byte> header = stackalloc byte[16];
        BinaryPrimitives.WriteUInt32LittleEndian(header, 0x544E4657);
        BinaryPrimitives.WriteUInt32LittleEndian(header[4..], (uint)cellSize);
        BinaryPrimitives.WriteUInt32LittleEndian(header[8..], (uint)mode);
        BinaryPrimitives.WriteUInt32LittleEndian(header[12..], reserved);
        stream.Write(header);
        Span<byte> width = stackalloc byte[2];
        foreach ((ushort codePoint, byte[] bitmap, ushort advance) in glyphs)
        {
            Assert.Equal(bitmapBytes, bitmap.Length);
            stream.Position = 16L + (long)(codePoint - WftFont.FirstCodePoint) * recordSize;
            stream.Write(bitmap);
            BinaryPrimitives.WriteUInt16LittleEndian(width, advance);
            stream.Write(width);
        }
        return path;
    }

    public void Dispose() => _directory.Dispose();
}
