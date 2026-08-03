using System;
namespace PangyaAPI.Network.Models
{

    [Flags]
    public enum EventType : ushort
    {
        NONE = 0,
        PANG_X_PLUS = 1 << 1,
        EXP_X2 = 1 << 2,
        ANGEL_WING = 1 << 3,
        EXP_X_PLUS = 1 << 4,
        UNKNOWN_0 = 1 << 5,
        UNKNOWN_1 = 1 << 6,
        CLUB_MASTERY_X_PLUS = 1 << 7,
        UNKNOWN_2 = 1 << 8,
        UNKNOWN_3 = 1 << 9,
        UNKNOWN_4 = 1 << 10,
        UNKNOWN_5 = 1 << 11,
        UNKNOWN_6 = 1 << 12,
        UNKNOWN_7 = 1 << 13,
        UNKNOWN_8 = 1 << 14,
        UNKNOWN_9 = 1 << 15
    }

}
