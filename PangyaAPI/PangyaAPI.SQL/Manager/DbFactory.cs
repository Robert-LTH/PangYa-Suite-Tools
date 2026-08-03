using System;
using Microsoft.Data.SqlClient;
using static PangyaAPI.SQL.ctx_db;
using PangyaAPI.SQL.Resources;

namespace PangyaAPI.SQL.Manager
{
    public static class DbFactory
    {
        public static mssql _mssql;
        public static database Create(ctx_db ctx)
        {
            if (ctx == null || string.IsNullOrEmpty(ctx.engine))
                throw new ArgumentNullException(nameof(ctx), SqlMessages.Get("ContextRequired"));

            switch (ctx.engine.ToUpper())
            {
                case "MSSQL":
                case "SQLSERVER":
                    {
                        return new mssql(ctx);
                    } 
                case "MYSQL":
                    return new mysql(ctx);

                case "POSTGRESQL":
                case "PGSQL":
                    return new postgresql(ctx);

                default:
                    throw new NotSupportedException(SqlMessages.Format("EngineUnsupported", ctx.engine));
            }
        }
    }
}
