using System.Runtime.InteropServices;
namespace PangyaAPI.Network.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 2)]
    public class uEventFlag
    {
        public uEventFlag(ushort ul = 0)
        {
            usEventFlag = ul;
        }

        [field: MarshalAs(UnmanagedType.U2, SizeConst = 2)]
        public ushort usEventFlag { get; set; }
        public bool pang_x_plus
        {
            get => (usEventFlag & (1 << 1)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 1)) : (usEventFlag & ~(1 << 1)));
        }

        public bool exp_x2
        {
            get => (usEventFlag & (1 << 2)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 2)) : (usEventFlag & ~(1 << 2)));
        }

        public bool angel_wing
        {
            get => (usEventFlag & (1 << 3)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 3)) : (usEventFlag & ~(1 << 3)));
        }

        /// <summary>
        /// 3x
        /// </summary>
        public bool exp_x_plus
        {
            get => (usEventFlag & (1 << 4)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 4)) : (usEventFlag & ~(1 << 4)));
        }

        public bool unknown_0
        {
            get => (usEventFlag & (1 << 5)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 5)) : (usEventFlag & ~(1 << 5)));
        }

        public bool unknown_1
        {
            get => (usEventFlag & (1 << 6)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 6)) : (usEventFlag & ~(1 << 6)));
        }

        public bool unknown_2
        {
            get => (usEventFlag & (1 << 8)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 8)) : (usEventFlag & ~(1 << 8)));
        }

        public bool club_mastery_x_plus
        {
            get => (usEventFlag & (1 << 7)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 7)) : (usEventFlag & ~(1 << 7)));
        }


        public bool unknown_3
        {
            get => (usEventFlag & (1 << 9)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 9)) : (usEventFlag & ~(1 << 9)));
        }

        public bool unknown_4
        {
            get => (usEventFlag & (1 << 10)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 10)) : (usEventFlag & ~(1 << 10)));
        }

        public bool unknown_5
        {
            get => (usEventFlag & (1 << 11)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 11)) : (usEventFlag & ~(1 << 11)));
        }

        public bool unknown_6
        {
            get => (usEventFlag & (1 << 12)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 12)) : (usEventFlag & ~(1 << 12)));
        }

        public bool unknown_7
        {
            get => (usEventFlag & (1 << 13)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 13)) : (usEventFlag & ~(1 << 13)));
        }

        public bool unknown_8
        {
            get => (usEventFlag & (1 << 14)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 14)) : (usEventFlag & ~(1 << 14)));
        }

        public bool unknown_9
        {
            get => (usEventFlag & (1 << 15)) != 0;
            set => usEventFlag = (ushort)(value ? (usEventFlag | (1 << 15)) : (usEventFlag & ~(1 << 15)));
        }
    }

}
