using System.Text;
using PangyaAPI.IFF;

namespace PangyaAPI.Tests;

public sealed class IffPatchAnalyzerTests
{
    [Fact]
    public void Analyze_MapsIdsAndFieldsAcrossLayoutsWithoutMutatingTarget()
    {
        IffSchema targetSchema = Schema("Target", 16,
            new("ItemId", 0, 4, IffFieldType.UInt32),
            new("Name", 4, 6, IffFieldType.FixedString),
            new("TargetOnly", 10, 2, IffFieldType.UInt16));
        IffSchema sourceSchema = Schema("Source", 20,
            new("TypeID", 4, 4, IffFieldType.UInt32),
            new("Name", 8, 10, IffFieldType.LongString),
            new("SourceOnly", 18, 2, IffFieldType.UInt16));
        IffRecord target = Record(targetSchema, 16, ("ItemId", 7u), ("Name", "Old"), ("TargetOnly", (ushort)99));
        IffRecord source = Record(sourceSchema, 20, ("TypeID", 7u), ("Name", "New"), ("SourceOnly", (ushort)12));
        target.AcceptChanges();

        IffPatchAnalysis analysis = IffPatchAnalyzer.Analyze(
            Document("Test.iff", targetSchema, 16), [target], Encoding.UTF8,
            Document("Test.iff", sourceSchema, 20), [source], Encoding.UTF8);

        IffPatchCandidate candidate = Assert.Single(analysis.Candidates);
        Assert.Equal(7u, candidate.ItemId);
        Assert.Equal(1, candidate.ChangedFieldCount);
        Assert.Equal(["Name"], analysis.SelectableFields);
        Assert.Equal("Old", target.GetValue("Name", Encoding.UTF8));
        Assert.False(target.IsDirty);

        Assert.Equal(1, analysis.Apply([target], [7u], ["Name"]));
        Assert.Equal("New", target.GetValue("Name", Encoding.UTF8));
        Assert.Equal((ushort)99, target.GetValue("TargetOnly"));
        Assert.True(target.IsDirty);
    }

    [Fact]
    public void Analyze_TruncatesAtEncodingSafeTextElementBoundary()
    {
        IffSchema schema = Schema("Desc", 10,
            new("ItemId", 0, 4, IffFieldType.UInt32),
            new("Description", 4, 6, IffFieldType.FixedString));
        IffSchema sourceSchema = Schema("Desc", 20,
            new("ItemId", 0, 4, IffFieldType.UInt32),
            new("Description", 4, 16, IffFieldType.LongString));
        IffRecord target = Record(schema, 10, ("ItemId", 1u), ("Description", "old"));
        IffRecord source = Record(sourceSchema, 20,
            [("ItemId", 1u), ("Description", "ééé")], Encoding.UTF8);

        IffPatchAnalysis analysis = IffPatchAnalyzer.Analyze(
            Document("Desc.iff", schema, 10), [target], Encoding.UTF8,
            Document("Desc.iff", sourceSchema, 20), [source], Encoding.UTF8);

        IffPatchCandidate candidate = Assert.Single(analysis.Candidates);
        Assert.Equal(["Description"], candidate.TruncatedFields);
        analysis.Apply([target], [1u], ["Description"]);
        Assert.Equal("éé", target.GetValue("Description", Encoding.UTF8));
    }

    [Fact]
    public void Analyze_DecodesPatchWithSourceEncodingAndWritesWithTargetEncoding()
    {
        IffSchema schema = Schema("Text", 12,
            new("ItemId", 0, 4, IffFieldType.UInt32),
            new("Name", 4, 8, IffFieldType.FixedString));
        IffRecord target = Record(schema, 12,
            [("ItemId", 1u), ("Name", "old")], Encoding.Latin1);
        IffRecord source = Record(schema, 12,
            [("ItemId", 1u), ("Name", "é")], Encoding.UTF8);

        IffPatchAnalysis analysis = IffPatchAnalyzer.Analyze(
            Document("Text.iff", schema, 12), [target], Encoding.Latin1,
            Document("Text.iff", schema, 12), [source], Encoding.UTF8);

        analysis.Apply([target], [1u], ["Name"]);

        Assert.Equal("é", target.GetValue("Name", Encoding.Latin1));
        Assert.Equal(0xE9, target.Bytes.Span[4]);
        Assert.Equal(0, target.Bytes.Span[5]);
    }

    [Fact]
    public void Analyze_ReportsUnmatchedRecordsAndAppliesOnlySelectedIds()
    {
        IffSchema schema = Schema("Test", 12,
            new("ID", 0, 4, IffFieldType.UInt32),
            new("Value", 4, 4, IffFieldType.UInt32));
        IffRecord target1 = Record(schema, 12, ("ID", 1u), ("Value", 10u));
        IffRecord target2 = Record(schema, 12, ("ID", 2u), ("Value", 20u));
        IffRecord source2 = Record(schema, 12, ("ID", 2u), ("Value", 200u));
        IffRecord source3 = Record(schema, 12, ("ID", 3u), ("Value", 300u));

        IffPatchAnalysis analysis = IffPatchAnalyzer.Analyze(
            Document("Test.iff", schema, 12), [target1, target2], Encoding.ASCII,
            Document("Test.iff", schema, 12), [source2, source3], Encoding.ASCII);

        Assert.Equal(1, analysis.SourceOnlyRecordCount);
        Assert.Equal(1, analysis.TargetOnlyRecordCount);
        Assert.Single(analysis.Candidates);
        Assert.Equal(0, analysis.Apply([target1, target2], [], ["Value"]));
        Assert.Equal(20u, target2.GetValue("Value"));
    }

    [Fact]
    public void Apply_ChangesOnlySelectedFieldsAcrossSelectedItems()
    {
        IffSchema schema = Schema("Test", 12,
            new("ItemId", 0, 4, IffFieldType.UInt32),
            new("First", 4, 4, IffFieldType.UInt32),
            new("Second", 8, 4, IffFieldType.UInt32));
        IffRecord target1 = Record(schema, 12, ("ItemId", 1u), ("First", 10u), ("Second", 20u));
        IffRecord target2 = Record(schema, 12, ("ItemId", 2u), ("First", 30u), ("Second", 40u));
        IffRecord source1 = Record(schema, 12, ("ItemId", 1u), ("First", 100u), ("Second", 200u));
        IffRecord source2 = Record(schema, 12, ("ItemId", 2u), ("First", 300u), ("Second", 400u));

        IffPatchAnalysis analysis = IffPatchAnalyzer.Analyze(
            Document("Test.iff", schema, 12), [target1, target2], Encoding.ASCII,
            Document("Test.iff", schema, 12), [source1, source2], Encoding.ASCII);

        Assert.Equal(["First", "Second"], analysis.SelectableFields);
        IffPatchSelectionSummary summary = analysis.Summarize([1u, 2u], ["Second"]);
        Assert.Equal(2, summary.ChangedRecordCount);
        Assert.Equal(2, summary.ChangedFieldCount);

        Assert.Equal(2, analysis.Apply([target1, target2], [1u, 2u], ["Second"]));
        Assert.Equal(10u, target1.GetValue("First"));
        Assert.Equal(200u, target1.GetValue("Second"));
        Assert.Equal(30u, target2.GetValue("First"));
        Assert.Equal(400u, target2.GetValue("Second"));
    }

    [Fact]
    public void Summarize_CompatibleUnchangedFieldReportsNoChanges()
    {
        IffSchema schema = Schema("Test", 8,
            new("ItemId", 0, 4, IffFieldType.UInt32),
            new("Value", 4, 4, IffFieldType.UInt32));
        IffRecord target = Record(schema, 8, ("ItemId", 1u), ("Value", 10u));
        IffRecord source = Record(schema, 8, ("ItemId", 1u), ("Value", 10u));

        IffPatchAnalysis analysis = IffPatchAnalyzer.Analyze(
            Document("Test.iff", schema, 8), [target], Encoding.ASCII,
            Document("Test.iff", schema, 8), [source], Encoding.ASCII);

        Assert.Equal(["Value"], analysis.SelectableFields);
        IffPatchSelectionSummary summary = analysis.Summarize([1u], ["Value"]);
        Assert.Equal(1, summary.SelectedRecordCount);
        Assert.Equal(0, summary.ChangedRecordCount);
        Assert.Equal(0, summary.ChangedFieldCount);
    }

    [Fact]
    public void Apply_SelectedOverlappingBitFieldsPreserveUnselectedBits()
    {
        IffSchema schema = Schema("Bits", 5,
            new("ItemId", 0, 4, IffFieldType.UInt32),
            new("Low", 4, 1, IffFieldType.BitField, BitMask: 0x0F, BitShift: 0),
            new("High", 4, 1, IffFieldType.BitField, BitMask: 0xF0, BitShift: 4));
        IffRecord target = Record(schema, 5, ("ItemId", 1u), ("Low", 1u), ("High", 2u));
        IffRecord source = Record(schema, 5, ("ItemId", 1u), ("Low", 3u), ("High", 4u));

        IffPatchAnalysis analysis = IffPatchAnalyzer.Analyze(
            Document("Bits.iff", schema, 5), [target], Encoding.ASCII,
            Document("Bits.iff", schema, 5), [source], Encoding.ASCII);

        Assert.Equal(1, analysis.Apply([target], [1u], ["Low"]));
        Assert.Equal(3u, target.GetValue("Low"));
        Assert.Equal(2u, target.GetValue("High"));
    }

    [Fact]
    public void Analyze_RejectsDuplicateIdsBeforeAnyMutation()
    {
        IffSchema schema = Schema("Test", 8,
            new("ItemId", 0, 4, IffFieldType.UInt32),
            new("Value", 4, 4, IffFieldType.UInt32));
        IffRecord target = Record(schema, 8, ("ItemId", 1u), ("Value", 10u));
        IffRecord source1 = Record(schema, 8, ("ItemId", 1u), ("Value", 20u));
        IffRecord source2 = Record(schema, 8, ("ItemId", 1u), ("Value", 30u));
        target.AcceptChanges();

        InvalidDataException exception = Assert.Throws<InvalidDataException>(() => IffPatchAnalyzer.Analyze(
            Document("Test.iff", schema, 8), [target], Encoding.ASCII,
            Document("Test.iff", schema, 8), [source1, source2], Encoding.ASCII));

        Assert.Contains("duplicate item ID 1", exception.Message, StringComparison.Ordinal);
        Assert.Equal(10u, target.GetValue("Value"));
        Assert.False(target.IsDirty);
    }

    private static IffSchema Schema(string name, int size, params IffField[] fields) =>
        new(name, size, fields, true);

    private static IffDocumentInfo Document(string fileName, IffSchema schema, int size) =>
        new(fileName, "TH", size, schema, new IffHeader(0, 0, 11, [0, 0, 0]));

    private static IffRecord Record(
        IffSchema schema,
        int size,
        params (string Field, object Value)[] values) =>
        Record(schema, size, values, Encoding.ASCII);

    private static IffRecord Record(
        IffSchema schema,
        int size,
        (string Field, object Value)[] values,
        Encoding encoding)
    {
        IffRecord record = IffRecord.CreateBlank(0, size, schema);
        foreach ((string field, object value) in values) record.SetValue(field, value, encoding);
        return record;
    }
}
