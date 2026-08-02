using System.Runtime.InteropServices;
namespace PangyaAPI.Network.Models
{

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
    public class uFlag
    {
        public uFlag(ulong _ull = 0)
        {
            ullFlag = _ull;
        }

        public ulong ullFlag { get; set; }
        /// <summary>
        /// Flag desconhecida. Representa o valor do bit 0.
        /// </summary>
        public bool Unknown0
        {
            get => (ullFlag & (1UL << 0)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 0)) : (ullFlag & ~(1UL << 0));
        }

        /// <summary>
        /// Desabilita a capacidade de jogar qualquer modo de jogo. Representa o valor do bit 1.
        /// </summary>
        public bool all_game
        {
            get => (ullFlag & (1UL << 1)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 1)) : (ullFlag & ~(1UL << 1));
        }

        /// <summary>
        /// Impede compras na loja e envio de presentes. Representa o valor do bit 2.
        /// </summary>
        public bool buy_and_gift_shop
        {
            get => (ullFlag & (1UL << 2)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 2)) : (ullFlag & ~(1UL << 2));
        }

        /// <summary>
        /// Impede o envio de presentes. Representa o valor do bit 3.
        /// </summary>
        public bool gift_shop
        {
            get => (ullFlag & (1UL << 3)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 3)) : (ullFlag & ~(1UL << 3));
        }

        /// <summary>
        /// Impede o acesso ao Papel Shop. Representa o valor do bit 4.
        /// </summary>
        public bool papel_shop
        {
            get => (ullFlag & (1UL << 4)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 4)) : (ullFlag & ~(1UL << 4));
        }

        /// <summary>
        /// Impede o acesso ao Personal Shop. Representa o valor do bit 5.
        /// </summary>
        public bool personal_shop
        {
            get => (ullFlag & (1UL << 5)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 5)) : (ullFlag & ~(1UL << 5));
        }

        /// <summary>
        /// Impede a participação no modo Stroke. Representa o valor do bit 6.
        /// </summary>
        public bool stroke
        {
            get => (ullFlag & (1UL << 6)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 6)) : (ullFlag & ~(1UL << 6));
        }

        /// <summary>
        /// Impede a participação em Match. Representa o valor do bit 7.
        /// </summary>
        public bool match
        {
            get => (ullFlag & (1UL << 7)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 7)) : (ullFlag & ~(1UL << 7));
        }

        /// <summary>
        /// Impede a participação em Tourney. Representa o valor do bit 8.
        /// </summary>
        public bool tourney
        {
            get => (ullFlag & (1UL << 8)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 8)) : (ullFlag & ~(1UL << 8));
        }

        /// <summary>
        /// Impede a participação em Team Tourney (agora é Short Game). Representa o valor do bit 9.
        /// </summary>
        public bool team_tourney
        {
            get => (ullFlag & (1UL << 9)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 9)) : (ullFlag & ~(1UL << 9));
        }

        /// <summary>
        /// Impede a participação em Guild Battle. Representa o valor do bit 10.
        /// </summary>
        public bool guild_battle
        {
            get => (ullFlag & (1UL << 10)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 10)) : (ullFlag & ~(1UL << 10));
        }

        /// <summary>
        /// Impede a participação em Pang Battle. Representa o valor do bit 11.
        /// </summary>
        public bool pang_battle
        {
            get => (ullFlag & (1UL << 11)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 11)) : (ullFlag & ~(1UL << 11));
        }

        /// <summary>
        /// Impede a participação no modo Approach. Representa o valor do bit 12.
        /// </summary>
        public bool approach
        {
            get => (ullFlag & (1UL << 12)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 12)) : (ullFlag & ~(1UL << 12));
        }

        /// <summary>
        /// Impede criar ou entrar em salas de lounge. Representa o valor do bit 13.
        /// </summary>
        public bool lounge
        {
            get => (ullFlag & (1UL << 13)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 13)) : (ullFlag & ~(1UL << 13));
        }

        /// <summary>
        /// Impede a participação no Scratchy System. Representa o valor do bit 14.
        /// </summary>
        public bool scratchy
        {
            get => (ullFlag & (1UL << 14)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 14)) : (ullFlag & ~(1UL << 14));
        }

        /// <summary>
        /// Flag desconhecida. Representa o valor do bit 15.
        /// </summary>
        public bool Unknown1
        {
            get => (ullFlag & (1UL << 15)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 15)) : (ullFlag & ~(1UL << 15));
        }

        /// <summary>
        /// Impede a visualização do rank server. Representa o valor do bit 16.
        /// </summary>
        public bool rank_server
        {
            get => (ullFlag & (1UL << 16)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 16)) : (ullFlag & ~(1UL << 16));
        }

        /// <summary>
        /// Impede o envio de ticker. Representa o valor do bit 17.
        /// </summary>
        public bool ticker
        {
            get => (ullFlag & (1UL << 17)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 17)) : (ullFlag & ~(1UL << 17));
        }

        /// <summary>
        /// Desabilita a funcionalidade de Mail Box. Representa o valor do bit 18.
        /// </summary>
        public bool mail_box
        {
            get => (ullFlag & (1UL << 18)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 18)) : (ullFlag & ~(1UL << 18));
        }

        /// <summary>
        /// Impede o acesso ao Grand Zodiac (provável). Representa o valor do bit 19.
        /// </summary>
        public bool grand_zodiac
        {
            get => (ullFlag & (1UL << 19)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 19)) : (ullFlag & ~(1UL << 19));
        }

        /// <summary>
        /// Impede o modo Single Play. Representa o valor do bit 20.
        /// </summary>
        public bool single_play
        {
            get => (ullFlag & (1UL << 20)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 20)) : (ullFlag & ~(1UL << 20));
        }

        /// <summary>
        /// Impede o acesso ao Grand Prix. Representa o valor do bit 21.
        /// </summary>
        public bool grand_prix
        {
            get => (ullFlag & (1UL << 21)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 21)) : (ullFlag & ~(1UL << 21));
        }

        /// <summary>
        /// Flag desconhecida. Representa os bits 22-23.
        /// </summary>
        public bool Unknown2
        {
            get => (ullFlag & (3UL << 22)) != 0;
            set => ullFlag = value ? (ullFlag | (3UL << 22)) : (ullFlag & ~(3UL << 22));
        }

        /// <summary>
        /// Impede o acesso a Guild. Representa o valor do bit 24.
        /// </summary>
        public bool guild
        {
            get => (ullFlag & (1UL << 24)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 24)) : (ullFlag & ~(1UL << 24));
        }

        /// <summary>
        /// Impede a participação no Special Shuffle Course. Representa o valor do bit 25.
        /// </summary>
        public bool ssc
        {
            get => (ullFlag & (1UL << 25)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 25)) : (ullFlag & ~(1UL << 25));
        }

        /// <summary>
        /// Flag desconhecida. Representa os bits 26-27.
        /// </summary>
        public bool Unknown3
        {
            get => (ullFlag & (3UL << 26)) != 0;
            set => ullFlag = value ? (ullFlag | (3UL << 26)) : (ullFlag & ~(3UL << 26));
        }

        /// <summary>
        /// Impede o uso do Memorial Shop. Representa o valor do bit 28.
        /// </summary>
        public bool memorial_shop
        {
            get => (ullFlag & (1UL << 28)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 28)) : (ullFlag & ~(1UL << 28));
        }

        /// <summary>
        /// Impede a participação no Short Game. Representa o valor do bit 29.
        /// </summary>
        public bool short_game
        {
            get => (ullFlag & (1UL << 29)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 29)) : (ullFlag & ~(1UL << 29));
        }

        /// <summary>
        /// Impede o acesso ao Character Mastery System. Representa o valor do bit 30.
        /// </summary>
        public bool char_mastery
        {
            get => (ullFlag & (1UL << 30)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 30)) : (ullFlag & ~(1UL << 30));
        }

        /// <summary>
        /// Flag desconhecida. Representa o valor do bit 31.
        /// </summary>
        public bool Unknown4
        {
            get => (ullFlag & (1UL << 31)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 31)) : (ullFlag & ~(1UL << 31));
        }

        /// <summary>
        /// Impede o uso do Lolo Compound Card System. Representa o valor do bit 32.
        /// </summary>
        public bool lolo_copound_card
        {
            get => (ullFlag & (1UL << 32)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 32)) : (ullFlag & ~(1UL << 32));
        }

        /// <summary>
        /// Impede o uso do Caddie Recycle Item System. Representa o valor do bit 33.
        /// </summary>
        public bool cadie_recycle
        {
            get => (ullFlag & (1UL << 33)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 33)) : (ullFlag & ~(1UL << 33));
        }

        /// <summary>
        /// Impede o uso do Legacy Tiki Shop System. Representa o valor do bit 34.
        /// </summary>
        public bool legacy_tiki_shop
        {
            get => (ullFlag & (1UL << 34)) != 0;
            set => ullFlag = value ? (ullFlag | (1UL << 34)) : (ullFlag & ~(1UL << 34));
        }

    }

}
