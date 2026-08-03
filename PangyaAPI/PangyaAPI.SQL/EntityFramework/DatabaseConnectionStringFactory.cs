using System;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using Npgsql;
using PangyaAPI.SQL.Resources;

namespace PangyaAPI.SQL.EntityFramework
{
    public static class DatabaseConnectionStringFactory
    {
        public static string Create(DatabaseOptions options)
        {
            Validate(options);
            return options.ConnectionString;
        }

        public static void Validate(DatabaseOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (string.IsNullOrWhiteSpace(options.Engine))
                throw new InvalidOperationException(SqlMessages.Get("EngineRequired"));
            if (string.IsNullOrWhiteSpace(options.ConnectionString))
                throw new InvalidOperationException(SqlMessages.Get("ConnectionRequired"));

            try
            {
                if (string.IsNullOrWhiteSpace(GetDataSource(options)) || string.IsNullOrWhiteSpace(GetDatabaseName(options)))
                    throw new InvalidOperationException(SqlMessages.Get("ServerAndDatabaseRequired"));

                if (NormalizeEngine(options) == "MSSQL")
                {
                    var sql = new SqlConnectionStringBuilder(options.ConnectionString);
                    if (!sql.IntegratedSecurity
                        && (string.IsNullOrWhiteSpace(sql.UserID) || string.IsNullOrWhiteSpace(sql.Password)))
                    {
                        throw new InvalidOperationException(
                            SqlMessages.Get("CredentialsRequired"));
                    }
                }
            }
            catch (ArgumentException exception)
            {
                throw new InvalidOperationException(SqlMessages.Get("ConnectionInvalid"), exception);
            }
        }

        public static string GetDataSource(DatabaseOptions options)
        {
            switch (NormalizeEngine(options))
            {
                case "MSSQL":
                    return new SqlConnectionStringBuilder(options.ConnectionString).DataSource;
                case "MYSQL":
                    return new MySqlConnectionStringBuilder(options.ConnectionString).Server;
                case "POSTGRESQL":
                    return new NpgsqlConnectionStringBuilder(options.ConnectionString).Host;
                default:
                    throw new NotSupportedException(SqlMessages.Format("EngineUnsupported", options.Engine));
            }
        }

        public static string GetDatabaseName(DatabaseOptions options)
        {
            switch (NormalizeEngine(options))
            {
                case "MSSQL":
                    return new SqlConnectionStringBuilder(options.ConnectionString).InitialCatalog;
                case "MYSQL":
                    return new MySqlConnectionStringBuilder(options.ConnectionString).Database;
                case "POSTGRESQL":
                    return new NpgsqlConnectionStringBuilder(options.ConnectionString).Database;
                default:
                    throw new NotSupportedException(SqlMessages.Format("EngineUnsupported", options.Engine));
            }
        }

        private static string NormalizeEngine(DatabaseOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            switch (options.Engine?.ToUpperInvariant())
            {
                case "MSSQL":
                case "SQLSERVER":
                    return "MSSQL";
                case "MYSQL":
                    return "MYSQL";
                case "POSTGRESQL":
                case "PGSQL":
                    return "POSTGRESQL";
                default:
                    throw new NotSupportedException(SqlMessages.Format("EngineUnsupported", options.Engine));
            }
        }
    }
}
