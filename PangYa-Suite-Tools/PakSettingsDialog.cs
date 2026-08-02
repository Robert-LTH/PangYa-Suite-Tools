using PangYa_Suite_Tools.Localization;
using PangyaAPI.PAK.Flags;

namespace PangYa_Suite_Tools;

internal sealed record PakSettingsOptions(
    byte PakVersion,
    string Author,
    bool RecompressEntries,
    PakFileEntryType CompressionType,
    byte CompressionLevel);

internal sealed class PakSettingsDialog : Form
{
    internal NumericUpDown HeaderVersionControl { get; } = new()
    {
        Dock = DockStyle.Fill,
        Hexadecimal = true,
        Maximum = byte.MaxValue,
        Name = "numPakHeaderVersion"
    };

    internal TextBox AuthorTextBox { get; } = new()
    {
        Dock = DockStyle.Fill,
        MaxLength = ushort.MaxValue,
        Name = "txtPakAuthor"
    };

    internal CheckBox RecompressCheckBox { get; } = new()
    {
        AutoSize = true,
        Dock = DockStyle.Fill,
        Name = "chkRecompressEntries"
    };

    internal ComboBox CompressionComboBox { get; } = new()
    {
        Dock = DockStyle.Fill,
        DropDownStyle = ComboBoxStyle.DropDownList,
        Name = "cboPakCompression"
    };

    internal NumericUpDown CompressionLevelControl { get; } = new()
    {
        Dock = DockStyle.Fill,
        Maximum = 9,
        Name = "numPakCompressionLevel"
    };

    internal Button ApplyButton { get; } = new()
    {
        AutoSize = true,
        DialogResult = DialogResult.None,
        Name = "btnApplyPakSettings"
    };

    internal Button CancelActionButton { get; } = new()
    {
        AutoSize = true,
        DialogResult = DialogResult.Cancel,
        Name = "btnCancelPakSettings"
    };

    internal PakSettingsDialog(PakSettingsOptions initialOptions)
    {
        ArgumentNullException.ThrowIfNull(initialOptions);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(520, 330);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = Strings.Pak_SettingsTitle;

        HeaderVersionControl.Value = initialOptions.PakVersion;
        AuthorTextBox.Text = initialOptions.Author;
        RecompressCheckBox.Text = Strings.Pak_RecompressEntries;
        RecompressCheckBox.Checked = initialOptions.RecompressEntries;
        CompressionComboBox.Items.AddRange(
            new object[] { PakFileEntryType.Raw, PakFileEntryType.LZ77, PakFileEntryType.LZ772 });
        CompressionComboBox.SelectedItem = initialOptions.CompressionType is PakFileEntryType.Directory
            ? PakFileEntryType.LZ772
            : initialOptions.CompressionType;
        CompressionLevelControl.Value = Math.Clamp(initialOptions.CompressionLevel, (byte)0, (byte)9);

        var fields = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            RowCount = 7
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
        for (int row = 0; row < 5; row++)
            fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        fields.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));

        AddField(fields, 0, Strings.Pak_HeaderVersion, HeaderVersionControl);
        AddField(fields, 1, Strings.Pak_Author, AuthorTextBox);
        fields.Controls.Add(RecompressCheckBox, 0, 2);
        fields.SetColumnSpan(RecompressCheckBox, 2);
        AddField(fields, 3, Strings.Pak_Compression, CompressionComboBox);
        AddField(fields, 4, Strings.Pak_CompressionLevel, CompressionLevelControl);

        var compressionHint = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            ForeColor = SystemColors.GrayText,
            Text = Strings.Pak_CompressionLevelUnknownHint
        };
        fields.Controls.Add(compressionHint, 0, 5);
        fields.SetColumnSpan(compressionHint, 2);

        ApplyButton.Text = Strings.Common_Apply;
        CancelActionButton.Text = Strings.Common_Cancel;
        ApplyButton.Click += ApplyButton_Click;
        RecompressCheckBox.CheckedChanged += (_, _) => UpdateCompressionEnabledState();
        CompressionComboBox.SelectedIndexChanged += (_, _) => UpdateCompressionEnabledState();

        var actions = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 8, 0, 0)
        };
        actions.Controls.Add(CancelActionButton);
        actions.Controls.Add(ApplyButton);
        fields.Controls.Add(actions, 0, 6);
        fields.SetColumnSpan(actions, 2);

        AcceptButton = ApplyButton;
        CancelButton = CancelActionButton;
        Controls.Add(fields);
        UpdateCompressionEnabledState();
    }

    internal bool TryGetSelectedOptions(out PakSettingsOptions options, out string error)
    {
        string author = AuthorTextBox.Text.Trim();
        if (author.Any(character => character > 0x7F))
        {
            options = null!;
            error = Strings.Pak_AuthorAsciiOnly;
            return false;
        }

        options = new PakSettingsOptions(
            (byte)HeaderVersionControl.Value,
            author,
            RecompressCheckBox.Checked,
            CompressionComboBox.SelectedItem is PakFileEntryType type
                ? type
                : PakFileEntryType.LZ772,
            (byte)CompressionLevelControl.Value);
        error = string.Empty;
        return true;
    }

    private void ApplyButton_Click(object? sender, EventArgs e)
    {
        if (!TryGetSelectedOptions(out _, out string error))
        {
            MessageBox.Show(this, error, Strings.Common_Error,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void UpdateCompressionEnabledState()
    {
        CompressionComboBox.Enabled = RecompressCheckBox.Checked;
        CompressionLevelControl.Enabled = RecompressCheckBox.Checked &&
            CompressionComboBox.SelectedItem is not PakFileEntryType.Raw;
    }

    private static void AddField(TableLayoutPanel fields, int row, string labelText, Control control)
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
