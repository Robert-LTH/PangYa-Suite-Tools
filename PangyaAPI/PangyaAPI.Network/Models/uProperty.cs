using System.Runtime.InteropServices;
namespace PangyaAPI.Network.Models
{

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 4)]
    public class uProperty
    {
        public uProperty(uint _ul = 0u)
        {
            ulProperty = _ul;
        }

        [field: MarshalAs(UnmanagedType.U4, SizeConst = 4)]
        public uint ulProperty { get; set; }

        // NORMAL (0) = nenhum bit ativado
        public bool normal => ulProperty == 0;

        public bool special // 1
        {
            get => (ulProperty & (1 << 0)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 0)) : (ulProperty & ~(1u << 0));
        }

        public bool small_play // 2
        {
            get => (ulProperty & (1 << 1)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 1)) : (ulProperty & ~(1u << 1));
        }

        public bool ladder // 4
        {
            get => (ulProperty & (1 << 2)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 2)) : (ulProperty & ~(1u << 2));
        }

        public bool adult // 8
        {
            get => (ulProperty & (1 << 3)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 3)) : (ulProperty & ~(1u << 3));
        }

        public bool mantle // 16 (também foi chamado de mantle antes)
        {
            get => (ulProperty & (1 << 4)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 4)) : (ulProperty & ~(1u << 4));
        }

        public bool skins // 32
        {
            get => (ulProperty & (1 << 5)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 5)) : (ulProperty & ~(1u << 5));
        }

        public bool only_rookie // 64
        {
            get => (ulProperty & (1 << 6)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 6)) : (ulProperty & ~(1u << 6));
        }

        public bool natural // 128
        {
            get => (ulProperty & (1 << 7)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 7)) : (ulProperty & ~(1u << 7));
        }

        public bool championship // 256
        {
            get => (ulProperty & (1 << 8)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 8)) : (ulProperty & ~(1u << 8));
        }

        public bool azul // 512
        {
            get => (ulProperty & (1 << 9)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 9)) : (ulProperty & ~(1u << 9));
        }

        public bool verde // 1024
        {
            get => (ulProperty & (1 << 10)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 10)) : (ulProperty & ~(1u << 10));
        }

        public bool grand_prix // 2048
        {
            get => (ulProperty & (1 << 11)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 11)) : (ulProperty & ~(1u << 11));
        }

        public bool relay // 4096
        {
            get => (ulProperty & (1 << 12)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 12)) : (ulProperty & ~(1u << 12));
        }

        public bool rookie_beginner_only // 2147483648 (bit 31)
        {
            get => (ulProperty & (1u << 31)) != 0;
            set => ulProperty = value ? (ulProperty | (1u << 31)) : (ulProperty & ~(1u << 31));
        }
    }

}
