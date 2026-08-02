using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows.Forms;
using PangYa_Suite_Tools.Localization;
using PangyaAPI.PAK.Flags;
using PangyaAPI.PAK.Models;
using PangyaAPI.Utilities.Cryptography;
using Xunit;

namespace PangYa_Suite_Tools.Tests;

[Collection("Localization")]
public sealed class PakSettingsTests
{
    [Fact]
    public void PakMaker_IsResizableAndKeepsEntryScrollbarVisible()
    {
        RunSta(() =>
        {
            using var form = new FrmPakMaker();
            form.Show();
            Application.DoEvents();

            Assert.Equal(FormBorderStyle.Sizable, form.FormBorderStyle);
            Assert.True(form.MaximizeBox);
            Assert.True(form.MinimumSize.Width > 0);
            Assert.True(form.MinimumSize.Height > 0);

            TreeView folders = PrivateField<TreeView>(form, "tvFolders");
            Assert.False(folders.CheckBoxes);

            ListView entries = PrivateField<ListView>(form, "lstEntries");
            Panel readerPanel = PrivateField<Panel>(form, "readerPanel");
            int originalWidth = entries.Width;
            int originalHeight = entries.Height;
            Assert.True(entries.Right <= readerPanel.ClientSize.Width - readerPanel.Padding.Right);
            form.Size = new Size(form.Width + 240, form.Height + 180);
            form.PerformLayout();
            Application.DoEvents();

            Assert.True(entries.Width > originalWidth);
            Assert.True(entries.Height > originalHeight);
            Assert.True(entries.Right <= readerPanel.ClientSize.Width - readerPanel.Padding.Right);

            form.Size = form.MinimumSize;
            form.PerformLayout();
            Application.DoEvents();
            Assert.True(entries.Right <= readerPanel.ClientSize.Width - readerPanel.Padding.Right);
            Assert.True(entries.Bottom <= readerPanel.ClientSize.Height - readerPanel.Padding.Bottom);
            Assert.True((bool)Invoke(form, "IsEntryListScrollBarVisible")!);
        });
    }

    [Fact]
    public void SettingsDialog_ReturnsLocalizedValidatedOptions()
    {
        RunSta(() =>
        {
            using var dialog = new PakSettingsDialog(new PakSettingsOptions(
                0x34, "Original", false, PakFileEntryType.LZ77, 6));

            Assert.Equal(Strings.Pak_SettingsTitle, dialog.Text);
            Assert.Equal(0x34, dialog.HeaderVersionControl.Value);
            Assert.Equal("Original", dialog.AuthorTextBox.Text);
            Assert.False(dialog.CompressionComboBox.Enabled);
            Assert.False(dialog.CompressionLevelControl.Enabled);

            dialog.RecompressCheckBox.Checked = true;
            dialog.CompressionComboBox.SelectedItem = PakFileEntryType.LZ772;
            dialog.CompressionLevelControl.Value = 8;
            dialog.AuthorTextBox.Text = "Updated";
            Assert.True(dialog.TryGetSelectedOptions(out PakSettingsOptions selected, out _));
            Assert.Equal(0x34, selected.PakVersion);
            Assert.Equal("Updated", selected.Author);
            Assert.True(selected.RecompressEntries);
            Assert.Equal(PakFileEntryType.LZ772, selected.CompressionType);
            Assert.Equal(8, selected.CompressionLevel);

            dialog.AuthorTextBox.Text = "作者";
            Assert.False(dialog.TryGetSelectedOptions(out _, out string error));
            Assert.Equal(Strings.Pak_AuthorAsciiOnly, error);
        });
    }

    [Fact]
    public void PakMaker_LoadedArchiveEnablesSettingsAndShowsRawKey()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string source = Path.Combine(directory, "source");
            string pak = Path.Combine(directory, "raw.pak");
            Directory.CreateDirectory(source);
            File.WriteAllText(Path.Combine(source, "file.txt"), "data");
            new PakWriter
            {
                EntryVersion = PakFileEntryVersion.Raw,
                EntryType = PakFileEntryType.Raw,
                LocationKeys = [],
                Author = "Raw Test"
            }.CreateFromDirectoryContents(source, pak);

            RunSta(() =>
            {
                using var form = new FrmPakMaker();
                ToolStripButton settings = PrivateField<ToolStripButton>(form, "_toolbarEditPakSettings");
                Assert.False(settings.Enabled);

                Invoke(form, "LoadPakWithKeys", pak, Encoding.UTF8, Array.Empty<uint>());

                Assert.True(settings.Enabled);
                Assert.Equal(Strings.Pak_Settings, settings.Text);
                Label key = PrivateField<Label>(form, "lblPakKeySummary");
                Assert.Contains(Strings.Pak_NoKey, key.Text);
                Label compression = PrivateField<Label>(form, "lblCompressionLevelSummary");
                Assert.Contains(Strings.Pak_CompressionLevelUnknown, compression.Text);

                typeof(FrmPakMaker).GetField("_knownCompressionLevel",
                    BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, (byte?)7);
                Invoke(form, "UpdateDisplayedCompressionLevel");
                Assert.Contains("7", compression.Text);

                var options = (PakRebuildOptions)Invoke(form, "BuildRebuildOptionsForCurrentPak")!;
                Assert.Equal(PakFileEntryVersion.Raw, options.EntryVersion);
                Assert.Equal(PakFileEntryType.Raw, options.EntryType);
                Assert.Equal("Raw Test", options.Author);
                Assert.Equal((byte?)0x12, options.PakVersion);
            });
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void PakMaker_ShowsCustomKeyValuesAndMarksUnusualEntries()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            uint[] customKeys = [1, 2, 3, 4];
            string source = Path.Combine(directory, "source");
            string pak = Path.Combine(directory, "custom.pak");
            Directory.CreateDirectory(source);
            File.WriteAllText(Path.Combine(source, "file.txt"), "data");
            new PakWriter { LocationKeys = customKeys }.CreateFromDirectoryContents(source, pak);

            RunSta(() =>
            {
                using var form = new FrmPakMaker();
                Invoke(form, "LoadPakWithKeys", pak, Encoding.UTF8, customKeys);

                Label key = PrivateField<Label>(form, "lblPakKeySummary");
                ToolTip toolTip = PrivateField<ToolTip>(form, "toolTip1");
                Assert.Contains(Strings.Pak_CustomKey, key.Text);
                Assert.Contains("0x00000001", toolTip.GetToolTip(key));

                var entry = new PakFileEntry
                {
                    Type = PakFileEntryType.Raw,
                    NameRaw = Encoding.ASCII.GetBytes("folder/CON.txt")
                };
                var warnings = PrivateField<HashSet<PakFileEntry>>(form, "_unusualNameEntries");
                warnings.Add(entry);
                Invoke(form, "PopulateList", (object)new[] { entry });

                ListView list = PrivateField<ListView>(form, "lstEntries");
                ListViewItem item = Assert.Single(list.Items.Cast<ListViewItem>());
                Assert.Equal(Color.LemonChiffon, item.BackColor);
                Assert.Equal(Strings.Pak_UnusualNamesTitle, item.ToolTipText);
            });
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void PakIssueTenResourcesExistInEveryCulture()
    {
        string[] keys =
        [
            "Common_Apply", "Pak_NoKey", "Pak_Settings", "Pak_SettingsTitle",
            "Pak_HeaderVersion", "Pak_RecompressEntries", "Pak_CompressionLevelUnknownHint",
            "Pak_CompressionLevelUnknown", "Pak_CompressionLevelSummaryFormat",
            "Pak_AuthorAsciiOnly", "Pak_ConfirmSettingsUpdate", "Pak_ConfirmRecompression",
            "Pak_UpdatingSettings", "Pak_SettingsUpdated", "Pak_SettingsUpdateFailed",
            "Pak_NoSettingsChanges", "Pak_UnusualNamesTitle", "Pak_UnusualNamesSummary"
        ];
        CultureInfo[] cultures =
        [
            CultureInfo.InvariantCulture,
            CultureInfo.GetCultureInfo(LocalizationManager.PortugueseBrazil),
            CultureInfo.GetCultureInfo(LocalizationManager.Swedish),
            CultureInfo.GetCultureInfo(LocalizationManager.Japonese),
            CultureInfo.GetCultureInfo(LocalizationManager.French)
        ];

        foreach (CultureInfo culture in cultures)
        {
            ResourceSet resources = Strings.ResourceManager.GetResourceSet(culture, true, false)!;
            var resourceKeys = resources.Cast<DictionaryEntry>()
                .Select(entry => (string)entry.Key)
                .ToHashSet(StringComparer.Ordinal);
            foreach (string key in keys) Assert.Contains(key, resourceKeys);
        }
    }

    private static void RunSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { failure = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        Assert.Null(failure);
    }

    private static object? Invoke(object target, string methodName, params object?[] arguments) =>
        target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(target, arguments);

    private static T PrivateField<T>(object target, string fieldName) where T : class =>
        Assert.IsType<T>(target.GetType().GetField(fieldName,
            BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(target));

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "PangYaPakSettingsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
