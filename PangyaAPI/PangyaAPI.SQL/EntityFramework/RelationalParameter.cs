using System.Data;

namespace PangyaAPI.SQL.EntityFramework
{
    public sealed record RelationalParameter(
        string Name,
        object Value,
        SqlDbType? SqlDbType = null,
        ParameterDirection Direction = ParameterDirection.Input);
}
