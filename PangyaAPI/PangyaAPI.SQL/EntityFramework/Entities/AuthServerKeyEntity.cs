namespace PangyaAPI.SQL.EntityFramework.Entities
{
    public sealed class AuthServerKeyEntity
    {
        public int ServerUid { get; set; }
        public string Key { get; set; } = string.Empty;
        public byte Valid { get; set; }
    }
}
