namespace PangyaAPI.SQL.EntityFramework.Entities
{
    public sealed class IpBanEntity
    {
        public string Ip { get; set; } = string.Empty;
        public string Mask { get; set; } = string.Empty;
    }
}
