using System.Text;

namespace PangyaAPI.PAK.Models;

[Flags]
public enum PakEntryNameIssueKind
{
    None = 0,
    Empty = 1,
    Rooted = 2,
    Traversal = 4,
    EmptySegment = 8,
    ControlCharacter = 16,
    InvalidCharacter = 32,
    TrailingDotOrSpace = 64,
    ReservedDeviceName = 128,
    EncodingMismatch = 256
}

public readonly record struct PakEntryNameIssue(string Name, PakEntryNameIssueKind Kind);

public static class PakEntryNameDiagnostics
{
    private static readonly HashSet<string> ReservedDeviceNames =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "CON", "PRN", "AUX", "NUL",
            "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
            "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

    public static IReadOnlyList<PakEntryNameIssue> FindIssues(PakReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        return reader.Entries
            .Select(entry => Inspect(entry.NameRaw, reader.FileNameEncoding))
            .Where(issue => issue.Kind != PakEntryNameIssueKind.None)
            .ToArray();
    }

    internal static PakEntryNameIssue Inspect(byte[] rawName, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(rawName);
        ArgumentNullException.ThrowIfNull(encoding);

        string name = encoding.GetString(rawName).Replace('\\', '/');
        PakEntryNameIssueKind kind = PakEntryNameIssueKind.None;
        if (string.IsNullOrWhiteSpace(name)) kind |= PakEntryNameIssueKind.Empty;
        if (name.StartsWith('/') || Path.IsPathRooted(name.Replace('/', '\\')))
            kind |= PakEntryNameIssueKind.Rooted;
        if (name.Any(char.IsControl)) kind |= PakEntryNameIssueKind.ControlCharacter;
        if (!rawName.AsSpan().SequenceEqual(encoding.GetBytes(name.Replace('/', '\\'))))
        {
            byte[] slashNormalized = encoding.GetBytes(name);
            if (!rawName.AsSpan().SequenceEqual(slashNormalized))
                kind |= PakEntryNameIssueKind.EncodingMismatch;
        }

        string[] segments = name.Split('/');
        if (segments.Any(segment => segment.Length == 0))
            kind |= PakEntryNameIssueKind.EmptySegment;
        foreach (string segment in segments)
        {
            if (segment is "." or "..") kind |= PakEntryNameIssueKind.Traversal;
            if (segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                kind |= PakEntryNameIssueKind.InvalidCharacter;
            if (segment.EndsWith('.') || segment.EndsWith(' '))
                kind |= PakEntryNameIssueKind.TrailingDotOrSpace;
            string stem = segment.Split('.')[0].TrimEnd(' ', '.');
            if (ReservedDeviceNames.Contains(stem))
                kind |= PakEntryNameIssueKind.ReservedDeviceName;
        }

        return new PakEntryNameIssue(name, kind);
    }
}
