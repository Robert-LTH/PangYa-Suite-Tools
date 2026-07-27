using PangYa_Suite_Tools.Localization;
using PangyaAPI.IFF;
using System.ComponentModel;

namespace PangYa_Suite_Tools;

internal sealed class IffSchemaRecoveryDialog : Form
{
    private readonly Action<string> _save;
    private readonly TextBox _json = new()
    {
        AcceptsReturn = true,
        AcceptsTab = true,
        Dock = DockStyle.Fill,
        Font = new Font(FontFamily.GenericMonospace, 9F),
        Multiline = true,
        ScrollBars = ScrollBars.Both,
        WordWrap = false
    };
    private readonly Label _error = new()
    {
        AutoEllipsis = true,
        Dock = DockStyle.Fill,
        ForeColor = Color.Firebrick
    };

    [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    internal string JsonText
    {
        get => _json.Text;
        set => _json.Text = value;
    }

    [Browsable(false)]
    internal string ErrorText => _error.Text;

    internal IffSchemaRecoveryDialog(IffSavedSchemaSource source, Action<string> save)
    {
        ArgumentNullException.ThrowIfNull(source);
        _save = save ?? throw new ArgumentNullException(nameof(save));
        Text = Strings.IFFManager_SchemaRecoveryTitle;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(720, 480);
        ClientSize = new Size(900, 650);

        var description = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            Text = Strings.IFFManager_SchemaRecoveryDescription
        };
        var sourceLabel = new Label
        {
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Text = string.Format(LocalizationManager.CurrentCulture,
                Strings.IFFManager_SchemaRecoverySourceFormat, source.SourcePath)
        };
        _error.Text = source.Error ?? string.Empty;
        _json.Text = source.Json;

        var saveButton = new Button { AutoSize = true, Text = Strings.IFFManager_SaveSchema };
        var cancelButton = new Button
        {
            AutoSize = true,
            DialogResult = DialogResult.Cancel,
            Text = Strings.Options_Cancel
        };
        saveButton.Click += (_, _) => TrySave(showMessage: true);
        var buttons = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        buttons.Controls.Add(cancelButton);
        buttons.Controls.Add(saveButton);

        var layout = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.Controls.Add(description, 0, 0);
        layout.Controls.Add(sourceLabel, 0, 1);
        layout.Controls.Add(_error, 0, 2);
        layout.Controls.Add(_json, 0, 3);
        layout.Controls.Add(buttons, 0, 4);
        Controls.Add(layout);
        AcceptButton = saveButton;
        CancelButton = cancelButton;
    }

    internal bool TrySave(bool showMessage)
    {
        try
        {
            _save(_json.Text);
            _error.Text = string.Empty;
            DialogResult = DialogResult.OK;
            return true;
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException or
                                   ArgumentException or NotSupportedException or System.Text.Json.JsonException)
        {
            _error.Text = ex.Message;
            if (showMessage)
                MessageBox.Show(ex.Message, Strings.IFFManager_Error,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
    }
}
