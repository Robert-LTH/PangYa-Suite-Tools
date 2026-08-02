using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
namespace PangyaAPI.Network.Models
{


    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 513)]
    public class CharacterInfo
    {
        public CharacterInfo()
        {
            clear();
        }

        public enum Stats : int
        {
            S_POWER,
            S_CONTROL,
            S_ACCURACY,
            S_SPIN,
            S_CURVE,
        }
        public uint _typeid { get; set; }
        public int id { get; set; }
        public byte default_hair { get; set; }
        public byte default_shirts { get; set; }
        public byte gift_flag { get; set; }
        public byte purchase { get; set; }
        /// <summary>
        /// Parts typeid, do 1 ao 24
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public uint[] parts_typeid { get; set; }
        /// <summary>
        /// Parts id, do 1 ao 24
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public uint[] parts_id { get; set; }
        /// <summary>
        ///Não sei bem direito o que é aqui
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 216)]
        public byte[] UccIndexList { get; set; }
        /// <summary>
        ///Auxiliar Parts 5, aqui fica anel
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public uint[] auxparts { get; set; }
        /// <summary>
        ///Cut-in, no primeiro mas acho que pode ser cut-in no resto
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] cut_in { get; set; }
        /// <summary>
        ///Aqui é o character stats, como controle, força, spin e etc
        /// </summary>
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public byte[] pcl { get; set; }
        /// <summary>
        /// Mastery, que aumenta os slot do stats do character
        /// </summary>
        public uint mastery { get; set; }
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_Character { get; set; }				// 4 Slot de card Character
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_Caddie { get; set; }             // 4 Slot de card Caddie
        [field: MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public uint[] Card_NPC { get; set; }

        public void clear()
        {
            Card_NPC = new uint[4];
            Card_Character = new uint[4];
            Card_Caddie = new uint[4];
            parts_id = new uint[24];
            parts_typeid = new uint[24];
            auxparts = new uint[5];
            UccIndexList = new byte[216];
            cut_in = new uint[4];
            pcl = new byte[5];
        }

        /// <summary>
        /// size = 513 bytes
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            using (var p = new PangyaBinaryWriter())
            {
                p.Write(_typeid);
                p.Write(id);
                p.Write(default_hair);
                p.Write(default_shirts);
                p.Write(gift_flag);
                p.Write(purchase);
                p.WriteUInt32(parts_typeid);
                p.WriteUInt32(parts_id);
                for (int i = 0; i < 216; i++)
                    p.WriteByte(0);
                p.WriteUInt32(auxparts);
                p.WriteUInt32(cut_in);
                p.WriteBytes(pcl);

                p.WriteUInt32(mastery);
                p.WriteUInt32(Card_Character);
                p.WriteUInt32(Card_Caddie);
                p.WriteUInt32(Card_NPC);

                return p.GetBytes;
            }
        }

        public CharacterInfo ToRead(packet r)
        {
            _typeid = r.ReadUInt32();
            id = r.ReadInt32();
            default_hair = r.ReadByte();
            default_shirts = r.ReadByte();
            gift_flag = r.ReadByte();
            purchase = r.ReadByte();

            parts_typeid = new uint[24];
            for (int i = 0; i < 24; i++)
                parts_typeid[i] = r.ReadUInt32();

            parts_id = new uint[24];
            for (int i = 0; i < 24; i++)
                parts_id[i] = r.ReadUInt32();

            UccIndexList = r.ReadBytes(216);

            auxparts = new uint[5];
            for (int i = 0; i < 5; i++)
                auxparts[i] = r.ReadUInt32();

            cut_in = new uint[4];
            for (int i = 0; i < 4; i++)
                cut_in[i] = r.ReadUInt32();

            pcl = r.ReadBytes(5);

            mastery = r.ReadUInt32();

            Card_Character = new uint[4];
            for (int i = 0; i < 4; i++)
                Card_Character[i] = r.ReadUInt32();

            Card_Caddie = new uint[4];
            for (int i = 0; i < 4; i++)
                Card_Caddie[i] = r.ReadUInt32();

            Card_NPC = new uint[4];
            for (int i = 0; i < 4; i++)
                Card_NPC[i] = r.ReadUInt32();

            return this;
        }

    }

}

