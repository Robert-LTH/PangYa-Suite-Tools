using System.Text;
using PangyaAPI.PAK.Models;

namespace PangyaAPI.Tests;

public sealed class PakEntryNameDiagnosticsTests
{
    [Theory]
    [InlineData("", PakEntryNameIssueKind.Empty)]
    [InlineData("/root/file.txt", PakEntryNameIssueKind.Rooted)]
    [InlineData("folder/../file.txt", PakEntryNameIssueKind.Traversal)]
    [InlineData("folder//file.txt", PakEntryNameIssueKind.EmptySegment)]
    [InlineData("folder/bad?.txt", PakEntryNameIssueKind.InvalidCharacter)]
    [InlineData("folder/name. ", PakEntryNameIssueKind.TrailingDotOrSpace)]
    [InlineData("folder/CON.txt", PakEntryNameIssueKind.ReservedDeviceName)]
    public void Inspect_FlagsUnusualNames(string name, PakEntryNameIssueKind expected)
    {
        PakEntryNameIssue issue = PakEntryNameDiagnostics.Inspect(
            Encoding.UTF8.GetBytes(name), Encoding.UTF8);

        Assert.True(issue.Kind.HasFlag(expected), $"Actual flags: {issue.Kind}");
    }

    [Fact]
    public void Inspect_FlagsBytesThatDoNotRoundTripThroughEncoding()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding shiftJis = Encoding.GetEncoding(932);

        PakEntryNameIssue issue = PakEntryNameDiagnostics.Inspect([0x82], shiftJis);

        Assert.True(issue.Kind.HasFlag(PakEntryNameIssueKind.EncodingMismatch));
    }

    [Theory]
    [InlineData("한국/파일.txt", 51949)]
    [InlineData("日本/ファイル.txt", 932)]
    [InlineData("café/élément.txt", 65001)]
    public void Inspect_AcceptsValidMultilingualNames(string name, int codePage)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding encoding = Encoding.GetEncoding(codePage);

        PakEntryNameIssue issue = PakEntryNameDiagnostics.Inspect(
            encoding.GetBytes(name), encoding);

        Assert.Equal(PakEntryNameIssueKind.None, issue.Kind);
    }
}
