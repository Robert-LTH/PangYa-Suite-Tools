//criado por LUISMK -> github.com/luismk
namespace PangyaAPI.PAK.Flags
{
    // ─────────────────────────────────────────────────────────────────────────
    // Enums
    // ─────────────────────────────────────────────────────────────────────────

    public enum PakFileEntryVersion : byte
    {
        //versoes da compression 
        V1 = 0,
        V2 = 1,
        V3 = 2,
        //pak sem chaves
        Raw = 0xF,
    }

    public enum PakFileEntryType : byte
    {
        //tipos de entrada de paks
        Raw = 0, //->> sem dados compress, so base
        LZ77 = 1,
        Directory = 2,
        LZ772 = 3,
    }
}
