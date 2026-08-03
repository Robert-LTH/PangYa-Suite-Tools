namespace PangyaAPI.Network.Configuration
{
    public sealed class IffOptions
    {
        public string Path { get; set; } = "data/pangya_jp.iff";
        public string Region { get; set; } = "JP";
        public string[] RequiredTables { get; set; } = ["Part", "AuxPart", "Card", "SetEffectTable"];
    }
}
