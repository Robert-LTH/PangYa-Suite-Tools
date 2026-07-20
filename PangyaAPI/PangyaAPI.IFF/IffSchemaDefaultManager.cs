using System.Globalization;

namespace PangyaAPI.IFF;

public enum IffSchemaUpdateAction
{
    KeepForNow,
    ReplaceWithBundledDefault,
    UseLocalDefinition
}

public sealed record IffSchemaUpdateCandidate(
    string FileName,
    string Region,
    string LocalPath,
    IffSchemaDefinition LocalDefinition,
    IffSchemaDefinition BundledDefinition,
    string LocalJson,
    string BundledJson)
{
    public int LocalRevision => LocalDefinition.DefaultRevision;
    public int BundledRevision => BundledDefinition.DefaultRevision;
}

public sealed record IffSchemaUpdateSelection(
    IffSchemaUpdateCandidate Candidate,
    IffSchemaUpdateAction Action);

public sealed record IffSchemaUpdateResult(
    int ReplacedCount,
    int PreferredLocalCount,
    int DeferredCount,
    string? BackupDirectory);

public sealed class IffSchemaDefaultManager
{
    private readonly DirectoryIffSchemaProvider _localProvider;
    private readonly IReadOnlyDictionary<string, IffSchemaDefinition> _bundledDefinitions;

    public IffSchemaDefaultManager(DirectoryIffSchemaProvider localProvider,
        EmbeddedIffSchemaProvider? embeddedProvider = null)
        : this(localProvider, (embeddedProvider ?? new EmbeddedIffSchemaProvider()).LoadDefinitions())
    {
    }

    public IffSchemaDefaultManager(DirectoryIffSchemaProvider localProvider,
        IEnumerable<IffSchemaDefinition> bundledDefinitions)
    {
        ArgumentNullException.ThrowIfNull(localProvider);
        ArgumentNullException.ThrowIfNull(bundledDefinitions);
        _localProvider = localProvider;
        _bundledDefinitions = bundledDefinitions.ToDictionary(Key, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<IffSchemaUpdateCandidate> FindUpdates()
    {
        var candidates = new List<IffSchemaUpdateCandidate>();
        foreach (IffSchemaDefinition bundled in _bundledDefinitions.Values
                     .Where(definition => definition.DefaultRevision > 0)
                     .OrderBy(definition => definition.FileName, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(definition => definition.Region, StringComparer.OrdinalIgnoreCase))
        {
            string localPath = _localProvider.GetSchemaPath(bundled.FileName, bundled.Region);
            if (!File.Exists(localPath)) continue;
            string localJson;
            IffSchemaDefinition local;
            try
            {
                localJson = File.ReadAllText(localPath);
                local = IffSchemaJson.Deserialize(localJson);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException
                or InvalidDataException or ArgumentException or NotSupportedException)
            {
                continue;
            }

            if (!Key(local).Equals(Key(bundled), StringComparison.OrdinalIgnoreCase) ||
                local.DefaultRevision >= bundled.DefaultRevision)
                continue;

            IffSchemaDefinition stamped = local with { DefaultRevision = bundled.DefaultRevision };
            string bundledJson = IffSchemaJson.Serialize(bundled);
            if (local.DefaultRevision == 0 && DefinitionsEqual(stamped, bundled))
            {
                _localProvider.Save(stamped);
                continue;
            }

            candidates.Add(new IffSchemaUpdateCandidate(bundled.FileName, bundled.Region, localPath,
                local, bundled, localJson, bundledJson));
        }
        return candidates;
    }

    public IffSchemaUpdateResult ApplyUpdates(IEnumerable<IffSchemaUpdateSelection> selections)
    {
        ArgumentNullException.ThrowIfNull(selections);
        IffSchemaUpdateSelection[] selected = selections.ToArray();
        if (selected.Select(item => Key(item.Candidate.LocalDefinition))
            .Distinct(StringComparer.OrdinalIgnoreCase).Count() != selected.Length)
            throw new ArgumentException("A schema update batch cannot contain duplicate schemas.", nameof(selections));
        foreach (IffSchemaUpdateSelection selection in selected)
            ValidateCandidate(selection.Candidate);

        IffSchemaUpdateSelection[] writes = selected
            .Where(item => item.Action != IffSchemaUpdateAction.KeepForNow)
            .ToArray();
        var replacements = writes.ToDictionary(
            item => Key(item.Candidate.LocalDefinition),
            item => item.Action switch
            {
                IffSchemaUpdateAction.ReplaceWithBundledDefault => item.Candidate.BundledDefinition,
                IffSchemaUpdateAction.UseLocalDefinition => item.Candidate.LocalDefinition with
                {
                    DefaultRevision = item.Candidate.BundledRevision
                },
                _ => throw new InvalidOperationException("Unsupported schema update action.")
            }, StringComparer.OrdinalIgnoreCase);

        ValidateStagedCatalog(replacements);
        if (writes.Length == 0)
            return new(0, 0, selected.Length, null);

        string backupDirectory = CreateBackupDirectory();
        var backups = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (IffSchemaUpdateSelection selection in writes)
            {
                string source = selection.Candidate.LocalPath;
                string backup = Path.Combine(backupDirectory, Path.GetFileName(source));
                File.Copy(source, backup, overwrite: false);
                backups[source] = backup;
            }
            foreach ((string key, IffSchemaDefinition definition) in replacements)
                _localProvider.Save(definition);
        }
        catch (Exception writeError)
        {
            var rollbackErrors = new List<Exception>();
            foreach ((string destination, string backup) in backups)
            {
                try { WriteAtomically(destination, File.ReadAllText(backup)); }
                catch (Exception rollbackError) when (rollbackError is IOException or UnauthorizedAccessException)
                {
                    rollbackErrors.Add(rollbackError);
                }
            }
            if (rollbackErrors.Count > 0)
                throw new AggregateException("The schema update failed and one or more files could not be restored.",
                    [writeError, .. rollbackErrors]);
            throw new IOException(
                $"The schema update failed; all changed files were restored from '{backupDirectory}'.", writeError);
        }

        return new(
            writes.Count(item => item.Action == IffSchemaUpdateAction.ReplaceWithBundledDefault),
            writes.Count(item => item.Action == IffSchemaUpdateAction.UseLocalDefinition),
            selected.Length - writes.Length,
            backupDirectory);
    }

    private void ValidateCandidate(IffSchemaUpdateCandidate candidate)
    {
        string key = Key(candidate.FileName, candidate.Region);
        string expectedPath = _localProvider.GetSchemaPath(candidate.FileName, candidate.Region);
        if (!Path.GetFullPath(candidate.LocalPath).Equals(expectedPath, StringComparison.OrdinalIgnoreCase) ||
            !Key(candidate.LocalDefinition).Equals(key, StringComparison.OrdinalIgnoreCase) ||
            !Key(candidate.BundledDefinition).Equals(key, StringComparison.OrdinalIgnoreCase) ||
            !_bundledDefinitions.TryGetValue(key, out IffSchemaDefinition? bundled) ||
            !DefinitionsEqual(bundled, candidate.BundledDefinition))
            throw new InvalidDataException("The schema update candidate does not match the configured schema catalog.");
        if (!File.Exists(expectedPath) || File.ReadAllText(expectedPath) != candidate.LocalJson)
            throw new InvalidDataException(
                $"The local schema '{expectedPath}' changed after the update list was created. Scan for updates again.");
    }

    private void ValidateStagedCatalog(IReadOnlyDictionary<string, IffSchemaDefinition> replacements)
    {
        Dictionary<string, IffSchemaDefinition> staged = _localProvider.LoadDefinitions()
            .ToDictionary(Key, StringComparer.OrdinalIgnoreCase);
        foreach ((string key, IffSchemaDefinition definition) in replacements)
            staged[key] = definition;

        foreach (IffSchemaDefinition definition in staged.Values)
        {
            IffSchemaJson.ValidateDefinition(definition, definition.MinimumRecordSize);
            _ = Materialize(definition, definition.MinimumRecordSize, staged,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }
    }

    private static IffSchema Materialize(IffSchemaDefinition definition, int recordSize,
        IReadOnlyDictionary<string, IffSchemaDefinition> catalog, HashSet<string> resolving)
    {
        string key = Key(definition);
        if (!resolving.Add(key))
            throw new InvalidDataException($"Circular IFF base schema reference detected at '{key}'.");
        try
        {
            if (definition.Base is null)
                return IffSchemaJson.ToSchema(definition, recordSize);

            IffSchemaDefinition? baseDefinition = FindBaseDefinition(definition, catalog);
            if (baseDefinition is null)
                throw new InvalidDataException(
                    $"Could not resolve base schema '{definition.Base.Name}' for '{definition.FileName}'.");
            IffSchema baseSchema = Materialize(baseDefinition, recordSize, catalog, resolving);
            return IffSchemaJson.ToSchema(definition, recordSize, baseSchema);
        }
        finally
        {
            resolving.Remove(key);
        }
    }

    private static IffSchemaDefinition? FindBaseDefinition(IffSchemaDefinition definition,
        IReadOnlyDictionary<string, IffSchemaDefinition> catalog)
    {
        IffSchemaBaseReference reference = definition.Base!;
        string baseFile = reference.Name + ".iff";
        foreach (string region in BaseRegionCandidates(reference.Region ?? definition.Region))
        {
            if (catalog.TryGetValue(Key(baseFile, region), out IffSchemaDefinition? resolved)) return resolved;
        }
        return catalog.TryGetValue(Key(baseFile, "*"), out IffSchemaDefinition? fallback) ? fallback : null;
    }

    private static IEnumerable<string> BaseRegionCandidates(string region)
    {
        yield return region;
        if (region.StartsWith("Global_", StringComparison.OrdinalIgnoreCase)) yield return "Global";
        else if (region.StartsWith("Japan", StringComparison.OrdinalIgnoreCase)) yield return "JP";
        else if (region.StartsWith("Korea", StringComparison.OrdinalIgnoreCase)) yield return "KR";
    }

    private string CreateBackupDirectory()
    {
        string root = Path.Combine(_localProvider.DirectoryPath, "backups");
        Directory.CreateDirectory(root);
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmssfff", CultureInfo.InvariantCulture);
        string candidate = Path.Combine(root, timestamp);
        for (int suffix = 1; Directory.Exists(candidate); suffix++)
            candidate = Path.Combine(root, $"{timestamp}-{suffix}");
        Directory.CreateDirectory(candidate);
        return candidate;
    }

    private static void WriteAtomically(string destination, string contents)
    {
        string temporary = destination + ".tmp." + Guid.NewGuid().ToString("N");
        try
        {
            File.WriteAllText(temporary, contents);
            File.Move(temporary, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static bool DefinitionsEqual(IffSchemaDefinition left, IffSchemaDefinition right) =>
        IffSchemaJson.Serialize(left) == IffSchemaJson.Serialize(right);

    private static string Key(IffSchemaDefinition definition) => Key(definition.FileName, definition.Region);

    private static string Key(string fileName, string region) =>
        $"{Path.GetFileName(fileName)}|{region}";
}
