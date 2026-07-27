using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;

namespace PangyaAPI.WFT;

public static class WftFontReader
{
    private const uint Signature = 0x544E4657;
    private const int HeaderSize = 16;
    private const int MaximumCellSize = 512;

    public static WftFont Open(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        string fullPath = Path.GetFullPath(path);
        SafeFileHandle handle = File.OpenHandle(fullPath, FileMode.Open, FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete, FileOptions.RandomAccess);
        try
        {
            long fileLength = RandomAccess.GetLength(handle);
            if (fileLength < HeaderSize)
                throw new InvalidDataException("The WFT file is truncated before its header.");

            Span<byte> header = stackalloc byte[HeaderSize];
            ReadExactly(handle, header, 0);
            if (BinaryPrimitives.ReadUInt32LittleEndian(header) != Signature)
                throw new InvalidDataException("The file does not contain a WFNT signature.");

            uint rawCellSize = BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
            if (rawCellSize is 0 or > MaximumCellSize)
                throw new InvalidDataException(
                    $"The WFT cell size must be between 1 and {MaximumCellSize} pixels.");

            uint rawMode = BinaryPrimitives.ReadUInt32LittleEndian(header[8..]);
            if (rawMode > (uint)WftCoverageMode.Antialiased)
                throw new InvalidDataException($"Unsupported WFT coverage mode {rawMode}.");

            int cellSize = checked((int)rawCellSize);
            var mode = (WftCoverageMode)rawMode;
            int bitsPerPixel = mode == WftCoverageMode.Antialiased ? 4 : 1;
            int rowStride = checked((cellSize * bitsPerPixel + 7) / 8);
            int bitmapByteCount = checked(rowStride * cellSize);
            int recordSize = checked(bitmapByteCount + sizeof(ushort));
            long payloadLength = fileLength - HeaderSize;
            if (payloadLength % recordSize != 0)
                throw new InvalidDataException(
                    "The WFT file ends with a partial glyph record.");
            long rawGlyphCount = payloadLength / recordSize;
            if (rawGlyphCount is not WftFont.LegacyGlyphCount and not WftFont.MaximumGlyphCount)
            {
                long legacyLength = checked(HeaderSize +
                    (long)recordSize * WftFont.LegacyGlyphCount);
                long maximumLength = checked(HeaderSize +
                    (long)recordSize * WftFont.MaximumGlyphCount);
                throw new InvalidDataException(
                    $"The WFT file length is {fileLength:N0} bytes; expected either " +
                    $"{legacyLength:N0} or {maximumLength:N0} bytes.");
            }

            uint reserved = BinaryPrimitives.ReadUInt32LittleEndian(header[12..]);
            return new WftFont(fullPath, handle, cellSize, mode, reserved,
                checked((int)rawGlyphCount),
                rowStride, bitmapByteCount, recordSize);
        }
        catch
        {
            handle.Dispose();
            throw;
        }
    }

    internal static void ReadExactly(SafeFileHandle handle, Span<byte> destination, long offset)
    {
        int totalRead = 0;
        while (totalRead < destination.Length)
        {
            int read = RandomAccess.Read(handle, destination[totalRead..], offset + totalRead);
            if (read == 0) throw new EndOfStreamException("The WFT glyph data is truncated.");
            totalRead += read;
        }
    }
}
