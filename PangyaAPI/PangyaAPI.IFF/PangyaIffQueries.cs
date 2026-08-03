namespace PangyaAPI.IFF;

public enum IffItemGroup : uint
{
    Character = 1, Part = 2, Club = 3, ClubSet = 4, Ball = 5, Item = 6,
    Caddie = 7, CaddieItem = 8, SetItem = 9, Course = 10, Match = 11,
    Title = 12, Enchant = 13, Skin = 14, HairStyle = 15, Mascot = 16,
    ChildItem = 17, Furniture = 18, OfflineShop = 19, Achievement = 20,
    CounterItem = 27, AuxPart = 28, QuestStuff = 29, QuestItem = 30, Card = 31
}

public enum IffAbilityEffect : uint
{
    None = 0,
    OneInAllStats = 25
}

public static class PangyaIffQueries
{
    private static readonly IReadOnlyDictionary<IffItemGroup, string> CommonTables =
        new Dictionary<IffItemGroup, string>
        {
            [IffItemGroup.Character] = "Character", [IffItemGroup.Part] = "Part",
            [IffItemGroup.Club] = "Club", [IffItemGroup.ClubSet] = "ClubSet",
            [IffItemGroup.Ball] = "Ball", [IffItemGroup.Item] = "Item",
            [IffItemGroup.Caddie] = "Caddie", [IffItemGroup.CaddieItem] = "CaddieItem",
            [IffItemGroup.SetItem] = "SetItem", [IffItemGroup.Course] = "Course",
            [IffItemGroup.Skin] = "Skin", [IffItemGroup.HairStyle] = "HairStyle",
            [IffItemGroup.Mascot] = "Mascot", [IffItemGroup.Furniture] = "Furniture",
            [IffItemGroup.Achievement] = "Achievement", [IffItemGroup.AuxPart] = "AuxPart",
            [IffItemGroup.QuestStuff] = "QuestStuff", [IffItemGroup.QuestItem] = "QuestItem",
            [IffItemGroup.Card] = "Card"
        };

    public static IffItemGroup GetItemGroup(uint typeId) => (IffItemGroup)((typeId & 0xFC00_0000u) >> 26);
    public static uint GetItemSubGroup24(uint typeId) => (typeId & ~0xFC00_0000u) >> 24;
    public static uint GetItemSubGroup22(uint typeId) => (typeId & ~0xFC00_0000u) >> 22;
    public static uint GetItemSubGroup21(uint typeId) => (typeId & ~0xFC00_0000u) >> 21;
    public static uint GetCharacter(uint typeId) => (typeId & 0x03FF_0000u) >> 18;
    public static uint GetCharacterPart(uint typeId) => (typeId & 0x0003_FF00u) >> 13;
    public static uint GetCharacterType(uint typeId) => (typeId & 0x0000_1FFFu) >> 8;
    public static uint GetItemNumber(uint typeId) => typeId & 0xFFu;
    public static bool IsEquipable(uint typeId) => ((typeId & 0xFE00_0000u) >> 25 & 3) == 0;

    public static IffRecord? FindCommonItem(this IffCatalog catalog, uint typeId)
    {
        if (!CommonTables.TryGetValue(GetItemGroup(typeId), out string? tableName) ||
            !catalog.TryGetTable(tableName, out IffTable? table))
            return null;
        return table!.Find(typeId);
    }

    public static string? GetItemName(this IffCatalog catalog, uint typeId) =>
        catalog.FindCommonItem(typeId)?.GetString("Name");

    public static bool ItemExists(this IffCatalog catalog, uint typeId) =>
        typeId != 0 && catalog.FindCommonItem(typeId) is not null;

    public static bool IsBuyable(this IffCatalog catalog, uint typeId)
    {
        IffRecord? record = catalog.FindCommonItem(typeId);
        return record is not null && record.GetBoolean("Enabled") &&
            (!record.TryGetValue("OnlyDisplay", out object? hidden) || hidden is not true);
    }

    public static bool IsGiftable(this IffCatalog catalog, uint typeId)
    {
        IffRecord? record = catalog.FindCommonItem(typeId);
        return record is not null && record.GetBoolean("Enabled") && record.GetBoolean("IsCash") &&
            record.GetBoolean("IsSaleable") != record.GetBoolean("IsGift");
    }

    public static bool HasIcon(this IffCatalog catalog, uint typeId)
    {
        IffRecord? record = catalog.FindCommonItem(typeId);
        return record is not null && !string.IsNullOrWhiteSpace(record.GetString("Icon"));
    }

    public static IReadOnlyList<IffRecord> FindSetEffects(this IffCatalog catalog, uint typeId)
    {
        if (!catalog.TryGetTable("SetEffectTable", out IffTable? table)) return [];
        return table!.Records.Where(record => Enumerable.Range(1, 5).Any(index =>
            record.TryGetValue($"Item{index}TypeId", out object? value) && value is uint item && item == typeId)).ToArray();
    }

    public static int GetStat(this IffRecord record, string statistic)
    {
        object? value = record.GetValue(statistic);
        return value switch
        {
            byte number => number, sbyte number => number, ushort number => number, short number => number,
            uint number => checked((int)number), int number => number,
            _ => throw new InvalidDataException($"IFF statistic '{statistic}' is not an integer.")
        };
    }
}
