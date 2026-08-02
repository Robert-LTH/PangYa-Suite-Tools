using System.Buffers.Binary;
using System.IO.Compression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PangyaAPI.IFF;
using PangyaAPI.Network.Configuration;
using PangyaAPI.Network.Hosting;
using Xunit;

namespace PangyaAPI.Network.Tests;

public sealed class IffCatalogHostingTests
{
    [Fact]
    public async Task HostedService_LoadsCatalogBeforeReturningFromStartup()
    {
        string directory = CreateTemporaryDirectory();
        try
        {
            string path = Path.Combine(directory, "pangya_jp.iff");
            CreateArchive(path, "Part", "AuxPart", "Card", "SetEffectTable");
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddPangyaIffCatalog(new IffOptions { Path = path, Region = "JP" });
            await using ServiceProvider provider = services.BuildServiceProvider();

            foreach (IHostedService service in provider.GetServices<IHostedService>())
                await service.StartAsync(CancellationToken.None);

            IIffCatalogProvider catalog = provider.GetRequiredService<IIffCatalogProvider>();
            Assert.True(catalog.IsLoaded);
            Assert.Equal(4, catalog.Catalog.Tables.Count);
            var character = new PangyaAPI.Network.Models.CharacterInfo();
            character.parts_typeid[0] = 1;
            character.auxparts[0] = 1;
            character.Card_Character[0] = 1;
            Assert.Equal(9, provider.GetRequiredService<CharacterEquipmentService>()
                .GetEquippedStat(character, PangyaAPI.Network.Models.CharacterInfo.Stats.S_POWER));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task HostedService_PropagatesInvalidArchiveFailure()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPangyaIffCatalog(new IffOptions { Path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".iff") });
        await using ServiceProvider provider = services.BuildServiceProvider();
        IHostedService hostedService = Assert.Single(provider.GetServices<IHostedService>());

        await Assert.ThrowsAsync<FileNotFoundException>(() => hostedService.StartAsync(CancellationToken.None));
        Assert.False(provider.GetRequiredService<IIffCatalogProvider>().IsLoaded);
    }

    private static void CreateArchive(string path, params string[] tables)
    {
        var schemas = new EmbeddedIffSchemaProvider();
        using ZipArchive archive = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (string table in tables)
        {
            IffSchemaDefinition definition = schemas.LoadDefinitions().Single(item =>
                item.Region == "JP" && item.FileName.Equals(table + ".iff", StringComparison.OrdinalIgnoreCase));
            byte[] bytes = new byte[8 + definition.MinimumRecordSize];
            BinaryPrimitives.WriteUInt16LittleEndian(bytes, 1);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(2), 548);
            bytes[4] = 13;
            IffSchema schema = schemas.Resolve(table + ".iff", "JP", definition.MinimumRecordSize).Schema!;
            if (schema.KeyField is { } key)
            {
                IffField field = schema.Fields.Single(candidate => candidate.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
                field.SetValue(bytes.AsSpan(8), 1u);
            }
            SetIfPresent(schema, bytes.AsSpan(8), "PowerUp", 2);
            SetIfPresent(schema, bytes.AsSpan(8), "Power", table == "Card" ? 4 : 3);
            ZipArchiveEntry entry = archive.CreateEntry(table + ".iff");
            using Stream output = entry.Open();
            output.Write(bytes);
        }
    }

    private static void SetIfPresent(IffSchema schema, Span<byte> record, string name, object value)
    {
        IffField? field = schema.Fields.FirstOrDefault(candidate => candidate.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        field?.SetValue(record, value);
    }

    private static string CreateTemporaryDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), "pangya-network-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
