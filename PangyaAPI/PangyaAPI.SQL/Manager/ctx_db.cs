using MySql.Data.MySqlClient;
using Npgsql;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
namespace PangyaAPI.SQL
{
    public class ctx_db
    {
        public string engine;
        public string ip;
        public string db_name;
        public string user;
        public string pass;
        public bool integrated_security;
        public string connection_string;
        public uint port;
        public bool cmd_log;
        public SQL _mssql;
        public _MYSQL _mysql;
        public _POSTGRESQL _postgresql;
        public class SQL
        {
            public SqlCommand hEnv = new SqlCommand();
            public SqlConnection hDbc = new SqlConnection();
            public DataSet hStmt = new DataSet();

            public void clear()
            {
                hDbc.Dispose();
                hStmt.Dispose();
                hEnv.Dispose();
            }
        }

        public class _MYSQL
        {
            public MySqlCommand hEnv = new MySqlCommand();
            public MySqlConnection hDbc = new MySqlConnection();
            public DataSet hStmt = new DataSet();
        }
        public class _POSTGRESQL
        {
            public NpgsqlCommand hEnv = new NpgsqlCommand();
            public NpgsqlConnection hDbc = new NpgsqlConnection();
            public DataSet hStmt = new DataSet();
        }
        // Método para criar a string de conexão com base no tipo de banco de dados
        public string CreateStrConnection()
        {
            if (!string.IsNullOrWhiteSpace(connection_string))
                return connection_string;

            switch (engine.ToUpper())
            {
                case "MSSQL":
                    if (integrated_security)
                        return $"Server={ip},{port};Database={db_name};" +
                               "Min Pool Size=27;Max Pool Size=950;TrustServerCertificate=True;" +
                               "MultipleActiveResultSets=True;Integrated Security=True;";

                    return $"Server={ip},{port};Database={db_name};User Id={user};Password={pass};" +
                           "Min Pool Size=27;Max Pool Size=950;TrustServerCertificate=True;" +
                           "MultipleActiveResultSets=True;Integrated Security=False;";

                case "MYSQL":
                    if (integrated_security)
                        return $"Server={ip};Port={port};Database={db_name};" +
                               "Integrated Security=yes;SslMode=Disabled;Pooling=true;" +
                               "MinimumPoolSize=27;MaximumPoolSize=950;";

                    return $"Server={ip};Port={port};Database={db_name};Uid={user};Pwd={pass};" +
                           "SslMode=Disabled;Pooling=true;MinimumPoolSize=27;MaximumPoolSize=950;";

                case "POSTGRESQL":
                    if (integrated_security)
                        return $"Host={ip};Port={port};Database={db_name};" +
                               "Pooling=true;Minimum Pool Size=27;Maximum Pool Size=950;";

                    return $"Host={ip};Port={port};Database={db_name};Username={user};Password={pass};" +
                           "Pooling=true;Minimum Pool Size=27;Maximum Pool Size=950;";

                default:
                    throw new Exception($"Engine '{engine}' não é suportado.");
            }
        }

    }
}
