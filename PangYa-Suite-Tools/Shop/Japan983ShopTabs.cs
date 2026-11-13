using PangyaAPI.IFF;
using PangyaAPI.UI;

namespace PangYa_Suite_Tools.Shop;

internal static class Japan983ShopTabs
{
    private static readonly int[] MainImages = [1, 7, 2, 3, 4, 5, 6];

    // The clothing through My Box rows follow the client dispatch at 0x61E4E0.
    // Feature-gated tabs are exposed in the editor because it has no player session.
    private static readonly int[][] SubImages =
    [
        [], [22, 2, 3, 4, 5, 27, 23], [7, 8, 9, 10, 28], [12],
        [13], [14, 15, 16, 17, 34, 35, 32], [19, 20, 21, 25, 24, 38],
    ];

    public static IReadOnlyList<ShopTab> Main { get; } = MainImages
        .Select(id => new ShopTab($"stab_{id}_n", $"stab_{id}_d", $"stab_{id}_d"))
        .ToArray();

    public static IReadOnlyList<ShopTab>[] Sub { get; } = SubImages
        .Select(ids => (IReadOnlyList<ShopTab>)ids.Select(id =>
        {
            string image = $"stext_{id}_n";
            return new ShopTab(image, image, image);
        }).ToArray()).ToArray();

    public static bool Matches(ShopCatalogItem item, int main, int sub)
    {
        IffItemGroup group = PangyaIffQueries.GetItemGroup(item.ItemId);
        if (main == 0) return group == IffItemGroup.Character;
        int image = SubImages[main][sub];
        return image switch
        {
            2 => IsPartCategory(item, 0x01),
            3 => IsPartCategory(item, 0x02),
            4 => IsPartCategory(item, 0x10),
            5 => IsPartCategory(item, 0x2C) || group == IffItemGroup.AuxPart,
            7 => group == IffItemGroup.ClubSet,
            8 => group == IffItemGroup.Ball,
            9 => group == IffItemGroup.Item && (item.ItemId & 0x0200_0000) != 0,
            10 => group == IffItemGroup.Item && (item.ItemId & 0x0200_0000) == 0,
            12 => group is IffItemGroup.Caddie or IffItemGroup.CaddieItem,
            13 => group == IffItemGroup.Mascot,
            14 => IsSkinCategory(item, 0),
            15 => IsSkinCategory(item, 1),
            16 => IsSkinCategory(item, 2),
            17 => IsSkinCategory(item, 3),
            19 => group == IffItemGroup.Achievement,
            20 => group == IffItemGroup.Item,
            21 => group is IffItemGroup.QuestStuff or IffItemGroup.QuestItem,
            22 => group is IffItemGroup.Part or IffItemGroup.AuxPart or IffItemGroup.HairStyle or IffItemGroup.SetItem,
            23 => group is IffItemGroup.HairStyle or IffItemGroup.SetItem ||
                group == IffItemGroup.Part && !IsPartCategory(item, 0x3BF),
            24 or 25 => group == IffItemGroup.Item,
            27 => IsPartCategory(item, 0x380),
            28 => group == IffItemGroup.Card,
            32 => group == IffItemGroup.Furniture,
            34 => group == IffItemGroup.Skin && (item.ItemId & 0x03FF_FFFF) is >= 100 and < (1u << 22),
            35 => group == IffItemGroup.Title || IsSkinCategory(item, 6),
            38 => group == IffItemGroup.Item,
            _ => false,
        };
    }

    private static bool IsPartCategory(ShopCatalogItem item, int mask) =>
        PangyaIffQueries.GetItemGroup(item.ItemId) == IffItemGroup.Part &&
        item.PartCategory is < 32 and byte category && (mask & (1 << category)) != 0;

    private static bool IsSkinCategory(ShopCatalogItem item, uint category) =>
        PangyaIffQueries.GetItemGroup(item.ItemId) == IffItemGroup.Skin &&
        PangyaIffQueries.GetItemSubGroup22(item.ItemId) == category;
}
