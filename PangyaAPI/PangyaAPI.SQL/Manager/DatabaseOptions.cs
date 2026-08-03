namespace PangyaAPI.SQL
{
    public sealed class DatabaseOptions
    {
        public string Engine { get; set; } = "MSSQL";
        public string ConnectionString { get; set; } = "";
        public bool LogCommands { get; set; } = true;
    }
}
