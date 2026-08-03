using System.Collections.Generic;

namespace PangyaAPI.SQL.EntityFramework
{
    public interface IRelationalCommandExecutor
    {
        Response ExecuteText(string commandText, IReadOnlyList<RelationalParameter> parameters);

        Response ExecuteStoredProcedure(string procedureName, IReadOnlyList<RelationalParameter> parameters);
    }
}
