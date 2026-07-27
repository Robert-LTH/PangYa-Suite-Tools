using PangYa_Suite_Tools.Localization;
using PangyaAPI.PAK.Flags;
using PangyaAPI.Utilities.Cryptography;

namespace PangYa_Suite_Tools;

internal sealed record PakCreationOptions(
    PakFileEntryVersion EntryVersion,
    PakFileEntryType EntryType,
    byte CompressLevel,
    string RegionLabel,
    uint[] LocationKeys)
{
    internal static PakCreationOptions Default { get; } = new(
        PakFileEntryVersion.V3,
        PakFileEntryType.LZ772,
        5,
        "Global",
        [.. PakKeys.GB]);
}

internal sealed class PakCreationOptionsDialog : Form
{
    private sealed record RegionChoice(string Label, uint[] Keys);

    internal ComboBox VersionComboBox { get; } = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Name = "cboVersion"
    };

    internal ComboBox CompressionComboBox { get; } = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Name = "cboCompressType"
    };

    internal NumericUpDown CompressionLevelControl { get; } = new()
    {
        Dock = DockStyle.Fill,
        Maximum = 9,
        Name = "numCompressLevel"
    };

    internal ComboBox RegionComboBox { get; } = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Name = "cboRegion"
    };

    internal Button CreateButton { get; } = new()
    {
        AutoSize = true,
        DialogResult = DialogResult.OK,
        Name = "btnCreate"
    };

    internal Button CancelActionButton { get; } = new()
    {
        AutoSize = true,
        DialogResult = DialogResult.Cancel,
        Name = "btnCancel"
    };

    internal PakCreationOptions SelectedOptions
    {
        get
        {
            var region = RegionComboBox.SelectedItem as RegionChoice
                ?? throw new InvalidOperationException("A PAK region must be selected.");
            return new PakCreationOptions(
                VersionComboBox.SelectedItem is PakFileEntryVersion version
                    ? version
                    : PakFileEntryVersion.V3,
                CompressionComboBox.SelectedItem is PakFileEntryType type
                    ? type
                    : PakFileEntryType.LZ772,
                (byte)CompressionLevelControl.Value,
                region.Label,
                [.. region.Keys]);
        }
    }

    internal PakCreationOptionsDialog(PakCreationOptions initialOptions)
    {
        ArgumentNullException.ThrowIfNull(initialOptions);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(460, 242);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = Strings.Pak_NewDialogTitle;

        VersionComboBox.Items.AddRange(Enum.GetValues<PakFileEntryVersion>().Cast<object>().ToArray());
        VersionComboBox.SelectedItem = initialOptions.EntryVersion;
        CompressionComboBox.Items.AddRange(Enum.GetValues<PakFileEntryType>().Cast<object>().ToArray());
        CompressionComboBox.SelectedItem = initialOptions.EntryType;
        CompressionLevelControl.Value = Math.Clamp(initialOptions.CompressLevel, (byte)0, (byte)9);

        List<RegionChoice> regions = PakKeys.All
            .Select(region => new RegionChoice(region.Label, region.Keys))
            .ToList();
        RegionComboBox.DisplayMember = nameof(RegionChoice.Label);
        RegionComboBox.Items.AddRange(regions.Cast<object>().ToArray());
        RegionComboBox.SelectedItem = regions.FirstOrDefault(region =>
            region.Keys.SequenceEqual(initialOptions.LocationKeys)) ?? regions[0];

        var fields = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 5
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        for (int row = 0; row < 4; row++)
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        AddField(fields, 0, Strings.Pak_EntryVersion, VersionComboBox);
        AddField(fields, 1, Strings.Pak_Compression, CompressionComboBox);
        AddField(fields, 2, Strings.Pak_CompressionLevel, CompressionLevelControl);
        AddField(fields, 3, Strings.Pak_Region, RegionComboBox);

        CreateButton.Text = Strings.Pak_CreateEmpty;
        CancelActionButton.Text = Strings.Common_Cancel;
        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        actions.Controls.Add(CancelActionButton);
        actions.Controls.Add(CreateButton);
        fields.Controls.Add(actions, 0, 4);
        fields.SetColumnSpan(actions, 2);

        AcceptButton = CreateButton;
        CancelButton = CancelActionButton;
        Controls.Add(fields);
    }

    private static void AddField(
        TableLayoutPanel fields,
        int row,
        string labelText,
        Control control)
    {
        fields.Controls.Add(new Label
        {
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Text = labelText
        }, 0, row);
        fields.Controls.Add(control, 1, row);
    }
}
