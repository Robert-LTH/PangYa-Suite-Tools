namespace PangyaAPI.SQL.EntityFramework.Entities
{
    public sealed class AccountEntity
    {
        public int Uid { get; set; }
        public int Logon { get; set; }
        public int? GameServerId { get; set; }
    }
}
