using System;
namespace PangyaAPI.Network.Models
{

    [Flags]
    public enum PropertyType : uint
    {
        NORMAL = 0,
        SPECIAL = 1,
        SMALL_PLAY = 2,
        LADDER = 4,
        ADULT = 8,
        MANTLE = 16,
        SKINS = 32,
        ONLY_ROOKIE = 64,
        NATURAL = 128,
        CHAMPIONSHIP = 256,
        AZUL = 512,
        VERDE = 1024,
        GRAND_PRIX = 2048,
        RELAY = 4096,
        ROOKIE_BEGINNER_ONLY = 2147483648
    }

}
