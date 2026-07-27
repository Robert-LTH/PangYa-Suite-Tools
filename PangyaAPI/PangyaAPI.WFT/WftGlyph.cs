namespace PangyaAPI.WFT;

public sealed class WftGlyph
{
    internal WftGlyph(ushort codePoint, int cellSize, ushort advanceWidth, byte[] coverage)
    {
        CodePoint = codePoint;
        CellWidth = cellSize;
        CellHeight = cellSize;
        AdvanceWidth = advanceWidth;
        Coverage = coverage;
    }

    public ushort CodePoint { get; }
    public int CellWidth { get; }
    public int CellHeight { get; }
    public ushort AdvanceWidth { get; }
    public ReadOnlyMemory<byte> Coverage { get; }
}
