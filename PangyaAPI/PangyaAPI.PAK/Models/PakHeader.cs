//criado por LUISMK -> github.com/luismk
using System;
using System.Collections.Generic;
using System.Text;

namespace PangyaAPI.PAK.Models
{
    // ─────────────────────────────────────────────────────────────────────────
    // Estruturas de cabeçalho
    // ─────────────────────────────────────────────────────────────────────────

    public class PakHeader
    {
        public uint OffsetFileEntry { get; set; }
        public uint NumFileEntry { get; set; }
        public byte Version { get; set; }
        public string Author { get; set; } = "Desconhecido";

        public const int BinarySize = 4 + 4 + 1; // 9 bytes
    }
}
