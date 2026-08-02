namespace PangyaAPI.Network.Models
{

    // ------------------ Player Account Basic ---------------- //
    // Struct ID state Block Flag
    public class IDStateBlockFlag
    {
        public IDStateBlockFlag(ulong _ul)
        {
            _ull_IDState = _ul;
        }
        private ulong _ull_IDState;

        /// <summary>
        /// Todo:  0 player normal status, 1 block por tempo, 2 block permanente, 4 block lounge, 8 block shop lounge,  10 nao lembro,  16 block gift shop, 20 paran end,  32 block papel shop, FLAGBLOCK_WRONG_CARD_BLOCK  = 40h,  64 block scratchy, 128 Ticker, 256 block memorial shop
        /// </summary>
        public ulong ull_IDState
        {
            get { return _ull_IDState; }
            set
            {
                _ull_IDState = value;
            }
        }

        public int block_time;

        public bool L_BLOCK_TEMPORARY //bloqueio temporario
        {
            get => (_ull_IDState & 1) == 1;
            set => _ull_IDState = value ? (_ull_IDState | 1) : (_ull_IDState & ~(1ul));
        }

        public bool L_BLOCK_FOREVER // bloqueio infinito
        {
            get => (_ull_IDState & 2) == 2;
            set => _ull_IDState = value ? (_ull_IDState | 2) : (_ull_IDState & ~(2ul));
        }

        public bool L_BLOCK_LOUNGE
        {
            get => (_ull_IDState & 4) == 4;
            set => _ull_IDState = value ? (_ull_IDState | 4) : (_ull_IDState & ~(4ul));
        }

        public bool L_BLOCK_SHOP_LOUNGE
        {
            get => (_ull_IDState & 8) == 8;
            set => _ull_IDState = value ? (_ull_IDState | 8) : (_ull_IDState & ~(8ul));
        }

        public bool L_BLOCK_GIFT_SHOP
        {
            get => (_ull_IDState & 16) == 16;
            set => _ull_IDState = value ? (_ull_IDState | 16) : (_ull_IDState & ~(16ul));
        }

        public bool L_BLOCK_PAPEL_SHOP
        {
            get => (_ull_IDState & 32) == 32;
            set => _ull_IDState = value ? (_ull_IDState | 32) : (_ull_IDState & ~(32ul));
        }

        public bool L_BLOCK_SCRATCHY
        {
            get => (_ull_IDState & 64) == 64;
            set => _ull_IDState = value ? (_ull_IDState | 64) : (_ull_IDState & ~(64ul));
        }

        public bool L_BLOCK_TICKER
        {
            get => (_ull_IDState & 128) == 128;
            set => _ull_IDState = value ? (_ull_IDState | 128) : (_ull_IDState & ~(128ul));
        }

        public bool L_BLOCK_MEMORIAL_SHOP
        {
            get => (_ull_IDState & 256) == 256;
            set => _ull_IDState = value ? (_ull_IDState | 256) : (_ull_IDState & ~(256ul));
        }

        public bool L_BLOCK_ALL_IP //nao sei qual bit flag
        {
            get => (_ull_IDState & 512) == 512;
            set => _ull_IDState = value ? (_ull_IDState | 512) : (_ull_IDState & ~(512ul));
        }

        public bool L_BLOCK_MAC_ADDRESS //nao sei qual bit flag
        {
            get => (_ull_IDState & 1024) == 1024;
            set => _ull_IDState = value ? (_ull_IDState | 1024) : (_ull_IDState & ~(1024ul));
        }
    }

}
