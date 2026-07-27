using PangYa_Suite_Tools.Configuration;
using PangYa_Suite_Tools.Localization;
using PangyaAPI.IFF;

namespace PangYa_Suite_Tools;

internal sealed class IffPatchSourceOptionsDialog : Form
{
    private readonly ComboBox _encoding = new();

    public int SelectedCodePage => ((PakEncodingOption)_encoding.SelectedItem!).CodePage;

    public IffPatchSourceOptionsDialog(int defaultCodePage, string patchFileName)
    {
        Text = Strings.IFFManager_PatchSourceOptions;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(470, 190);

        var description = new Label
        {
            Text = string.Format(LocalizationManager.CurrentCulture,
                Strings.IFFManager_PatchEncodingDescriptionFormat, patchFileName),
            AutoSize = false,
            Location = new Point(16, 16),
            Size = new Size(438, 42)
        };

        var label = new Label
        {
            Text = Strings.IFFManager_PatchSourceEncoding,
            AutoSize = true,
            Location = new Point(16, 72)
        };
        _encoding.Name = "cboPatchSourceEncoding";
        _encoding.DropDownStyle = ComboBoxStyle.DropDownList;
        _encoding.Location = new Point(190, 68);
        _encoding.Size = new Size(264, 24);
        _encoding.DisplayMember = nameof(PakEncodingOption.DisplayName);
        IReadOnlyList<PakEncodingOption> options = IffStringEncodingPreferences.GetAvailableEncodings();
        foreach (PakEncodingOption option in options) _encoding.Items.Add(option);
        _encoding.SelectedItem = options.FirstOrDefault(option => option.CodePage == defaultCodePage) ?? options[0];

        var cancel = new Button
        {
            Text = Strings.Options_Cancel,
            DialogResult = DialogResult.Cancel,
            Location = new Point(360, 140),
            Size = new Size(94, 30)
        };
        var next = new Button
        {
            Text = Strings.IFFManager_PatchNext,
            DialogResult = DialogResult.OK,
            Location = new Point(260, 140),
            Size = new Size(94, 30)
        };
        Controls.AddRange([description, label, _encoding, next, cancel]);
        AcceptButton = next;
        CancelButton = cancel;
    }
}

internal sealed class IffPatchSelectionDialog : Form
{
    private readonly CheckedListBox _items = new();
    private readonly IReadOnlyList<IffPatchCandidate> _candidates;

    public IReadOnlyList<uint> SelectedItemIds => _items.CheckedIndices.Cast<int>()
        .Select(index => _candidates[index].ItemId)
        .ToArray();

    public IffPatchSelectionDialog(IffPatchAnalysis analysis)
    {
        _candidates = analysis.Candidates;
        Text = Strings.IFFManager_PatchSelectItems;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(650, 420);
        Size = new Size(780, 560);

        var description = new Label
        {
            Text = Strings.IFFManager_PatchSelectionDescription,
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(10, 10, 10, 0)
        };
        _items.Name = "lstPatchItems";
        _items.Dock = DockStyle.Fill;
        _items.CheckOnClick = true;
        foreach (IffPatchCandidate candidate in _candidates)
        {
            string labels = string.IsNullOrWhiteSpace(candidate.TargetLabel) &&
                string.IsNullOrWhiteSpace(candidate.SourceLabel)
                ? string.Empty
                : $" — {candidate.TargetLabel} → {candidate.SourceLabel}";
            _items.Items.Add(string.Format(LocalizationManager.CurrentCulture,
                Strings.IFFManager_PatchItemFormat, candidate.ItemId, candidate.ChangedFieldCount, labels), true);
        }

        var selectAll = new Button { Text = Strings.IFFManager_PatchSelectAll, AutoSize = true };
        selectAll.Click += (_, _) => SetAll(true);
        var clear = new Button { Text = Strings.IFFManager_PatchClearAll, AutoSize = true };
        clear.Click += (_, _) => SetAll(false);
        var next = new Button
        {
            Text = Strings.IFFManager_PatchNext,
            AutoSize = true
        };
        next.Click += (_, _) =>
        {
            if (SelectedItemIds.Count == 0)
            {
                MessageBox.Show(this, Strings.IFFManager_PatchNoItemsSelected, Strings.IFFManager_Patch,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        };
        var cancel = new Button
        {
            Text = Strings.Options_Cancel,
            DialogResult = DialogResult.Cancel,
            AutoSize = true
        };
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            WrapContents = false
        };
        buttons.Controls.AddRange([cancel, next, clear, selectAll]);
        Controls.Add(_items);
        Controls.Add(description);
        Controls.Add(buttons);
        AcceptButton = next;
        CancelButton = cancel;
    }

    private void SetAll(bool value)
    {
        for (int index = 0; index < _items.Items.Count; index++) _items.SetItemChecked(index, value);
    }
}

internal sealed class IffPatchFieldSelectionDialog : Form
{
    private readonly CheckedListBox _fields = new();
    private readonly IffPatchAnalysis _analysis;
    private readonly IReadOnlyList<uint> _selectedItemIds;

    public IReadOnlyList<string> SelectedFieldNames => _fields.CheckedIndices.Cast<int>()
        .Select(index => _analysis.SelectableFields[index])
        .ToArray();

    public IffPatchFieldSelectionDialog(IffPatchAnalysis analysis, IReadOnlyList<uint> selectedItemIds)
    {
        _analysis = analysis;
        _selectedItemIds = selectedItemIds;
        Text = Strings.IFFManager_PatchSelectFields;
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(520, 380);
        Size = new Size(650, 500);

        var description = new Label
        {
            Text = Strings.IFFManager_PatchFieldSelectionDescription,
            Dock = DockStyle.Top,
            Height = 42,
            Padding = new Padding(10, 10, 10, 0)
        };
        _fields.Name = "lstPatchFields";
        _fields.Dock = DockStyle.Fill;
        _fields.CheckOnClick = true;
        foreach (string fieldName in analysis.SelectableFields)
            _fields.Items.Add(fieldName, true);

        var selectAll = new Button { Text = Strings.IFFManager_PatchSelectAll, AutoSize = true };
        selectAll.Click += (_, _) => SetAll(true);
        var clear = new Button { Text = Strings.IFFManager_PatchClearAll, AutoSize = true };
        clear.Click += (_, _) => SetAll(false);
        var review = new Button { Text = Strings.IFFManager_PatchReview, AutoSize = true };
        review.Click += (_, _) => ReviewSelection();
        var cancel = new Button
        {
            Text = Strings.Options_Cancel,
            DialogResult = DialogResult.Cancel,
            AutoSize = true
        };
        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(8),
            WrapContents = false
        };
        buttons.Controls.AddRange([cancel, review, clear, selectAll]);
        Controls.Add(_fields);
        Controls.Add(description);
        Controls.Add(buttons);
        AcceptButton = review;
        CancelButton = cancel;
    }

    private void ReviewSelection()
    {
        IReadOnlyList<string> selectedFields = SelectedFieldNames;
        if (selectedFields.Count == 0)
        {
            MessageBox.Show(this, Strings.IFFManager_PatchNoFieldsSelected, Strings.IFFManager_Patch,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        IffPatchSelectionSummary summary = _analysis.Summarize(_selectedItemIds, selectedFields);
        if (summary.ChangedFieldCount == 0)
        {
            MessageBox.Show(this, Strings.IFFManager_PatchNoChangesSelected, Strings.IFFManager_Patch,
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void SetAll(bool value)
    {
        for (int index = 0; index < _fields.Items.Count; index++) _fields.SetItemChecked(index, value);
    }
}
