using System.Text.Json;

namespace PangYa_Suite_Tools.Configuration;

internal sealed record UpdateListGeneratorSettings(
    string? KeyLabel,
    string? PatchVersion,
    string? UpdateListVersion,
    string? PatchNumber);

internal static class UpdateListGeneratorPreferences
{
    private static readonly object PreferenceLock = new();

    internal static string? PreferencePathOverride { get; set; }

    private static string PreferencePath => PreferencePathOverride ?? Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "PangYa-Suite-Tools", "updatelist-generator-settings.json");

    internal static UpdateListGeneratorSettings? Load()
    {
        lock (PreferenceLock)
        {
            try
            {
                if (!File.Exists(PreferencePath)) return null;
                return JsonSerializer.Deserialize<UpdateListGeneratorSettings>(
                    File.ReadAllText(PreferencePath));
            }
            catch
            {
                return null;
            }
        }
    }

    internal static void Save(UpdateListGeneratorSettings settings)
    {
        var normalized = new UpdateListGeneratorSettings(
            settings.KeyLabel?.Trim(),
            settings.PatchVersion?.Trim(),
            settings.UpdateListVersion?.Trim(),
            settings.PatchNumber?.Trim());

        lock (PreferenceLock)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PreferencePath)!);
                File.WriteAllText(PreferencePath, JsonSerializer.Serialize(normalized));
            }
            catch
            {
                // A preference must never prevent UpdateList operations.
            }
        }
    }
}
