using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using response = PangyaAPI.SQL.Response;
using result_set = PangyaAPI.SQL.Result_Set;

namespace PangyaAPI.SQL.Manager
{
    public class mssql : database
    {
        private static readonly object s_connectionRetryLock = new object();
        private static readonly TimeSpan s_connectionRetryDelay = TimeSpan.FromSeconds(30);
        private static DateTime s_nextConnectionAttemptUtc = DateTime.MinValue;
        private static bool s_connectionAttemptInProgress;
        private static bool s_connectionOutage;

        public virtual void destroy()
        {

            if (is_connected())
                disconnect();

            if (m_ctx_db._mssql.hDbc != null)
                m_ctx_db._mssql.hDbc = null;

            if (m_ctx_db._mssql.hEnv != null)
                m_ctx_db._mssql.hEnv = null;

            m_state = false;
        }


        public override bool hasGoneAway()
        {
            return false;
        }


        public override void connect()
        {
            if (!TryBeginConnectionAttempt())
            {
                m_connected = false;
                return;
            }

            try
            {
                init();

                if (m_error)
                    throw new exception(m_error_string);

                if (m_ctx_db._mssql.hDbc != null && m_ctx_db._mssql.hDbc.State == ConnectionState.Closed)
                {
                    m_ctx_db._mssql.hDbc = new SqlConnection(m_ctx_db.CreateStrConnection());

                    m_ctx_db._mssql.hDbc.Open();
                }

                m_connected = true;

                if (CompleteConnectionAttempt())
                {
                    PangyaLog.Write("[mssql::Connect][Log] SQL Server connection restored.", LogDestination.GeneralFile | LogDestination.Console);
                }
            }
            catch (Exception ex)
            {
                m_connected = false;
                if (RecordConnectionFailure())
                {
                    PangyaLog.Write("[mssql::Connect][Error] " + ex.Message
                        + "] Further connection attempts will be paused for 30 seconds.",
                        LogDestination.GeneralFile | LogDestination.Console);
                }
            }
        }

        public override void reconnect()
        {
            disconnect();
            connect();
        }

        public override void disconnect()
        {
            if (is_connected())
            {
                if (m_ctx_db._mssql.hDbc != null)
                    m_ctx_db._mssql.hDbc.Close();
            }

            m_connected = false;
        }


        public override response ExecQuery(string _query)
        {
            response res = new response();
            if (!is_connected())
                return res;

            uint numResults = 0;
            int numRows;
            try
            {
                HandleDiagnosticRecord(_query);
                if (m_ctx_db._mssql.hStmt != null)
                {
                    var _data = m_ctx_db._mssql.hStmt.Tables[m_ctx_db.db_name];
                    if (_data == null)
                    {
                        res = new response();
                        res.addResultSet(new result_set((uint)result_set.STATE_TYPE._NO_DATA, 0, 0, null));
                        return res;
                    }
                    if (_data.Rows.Count == 1)
                    {
                        numResults = 1;
                    }
                    if (_data.Rows.Count > 1)
                    {
                        numResults = (uint)_data.Rows.Count - 1;
                    }
                    numRows = _data.Columns.Count;
                    res.setRowsAffected(numRows);
                    if (numResults > 0)
                    {
                        foreach (DataRow item in _data.Rows)
                            res.addResultSet(new result_set((uint)result_set.STATE_TYPE.HAVE_DATA, numResults, (uint)numRows, item));
                    }

                    m_ctx_db._mssql.hStmt.Clear();
                }
                return res;
            }
            catch (Exception ex)
            {
                if (HandleConnectionFailure(ex))
                    return res;

                // Montar a string de comando para execução do procedimento
                var commandText = $"{_query}";

                // A mensagem completa da exceção
                string mensagemErro = string.Format(
                    "[mssql::ExecQuery][Error]: {0}, [Query]: {1}",
                    ex.Message, commandText
                );

                // Enviar a mensagem para o legacy logging queue
                PangyaLog.Write(mensagemErro, LogDestination.Console); return res;
            }
        }
        public override response ExecProc(string _proc_name, string valor = null)
        {
            response res = new response();
            if (!is_connected())
                return res;

            uint numResults = 0;
            int numRows = 0;
            try
            {
                HandleDiagnosticRecord(_proc_name, valor);
                if (m_ctx_db._mssql.hStmt != null && m_ctx_db._mssql.hStmt.Tables[m_ctx_db.db_name] != null)
                {
                    var _data = m_ctx_db._mssql.hStmt.Tables[m_ctx_db.db_name];
                    if (_data != null && _data.Rows.Count == 1)
                    {
                        numResults = 1;
                    }
                    if (_data.Rows.Count > 1)
                    {
                        numResults = (uint)_data.Rows.Count - 1;
                    }
                    numRows = _data.Columns.Count;
                    res.setRowsAffected(numRows);
                    if (numResults > 0)
                    {
                        foreach (DataRow item in _data.Rows)
                            res.addResultSet(new result_set((uint)result_set.STATE_TYPE.HAVE_DATA, numResults, (uint)numRows, item));
                    }
                    m_ctx_db._mssql.hStmt.Clear();
                }
                return res;
            }
            catch (Exception ex)
            {
                if (HandleConnectionFailure(ex))
                    return res;

                // Montar a string de comando para execução do procedimento
                var commandText = $"EXEC {m_ctx_db.db_name}.{_proc_name} ";

                if (!string.IsNullOrEmpty(valor))
                {
                    // Divide os valores com base na vírgula
                    var valorArray = valor.Split(',')
                                            .Select(v => v.Trim()) // Remove espaços em branco
                                            .Select(v => $"'{v}'") // Adiciona aspas simples
                                            .ToArray();

                    // Junta os valores formatados de volta em uma string
                    commandText += string.Join(", ", valorArray);
                }

                // A mensagem completa da exceção
                string mensagemErro = string.Format(
                    "[mssql::ExecProc][Error]: {0}, [Query]: {1}",
                    ex.Message, commandText
                );

                // Enviar a mensagem para o legacy logging queue
                PangyaLog.Write(mensagemErro, LogDestination.GeneralFile | LogDestination.Console);
                return res;
            }
        }

        public override response ExecQueryWithParams(string _proc_name, string[] parameter = null, SqlDbType[] tipo = null, object[] valor = null, ParameterDirection Direcao = ParameterDirection.Input)
        {
            if (!is_connected())
                return new response();

            try
            {
                return ExecuteParameterized(_proc_name, parameter, tipo, valor, Direcao, CommandType.Text);
            }
            catch (Exception ex)
            {
                HandleConnectionFailure(ex);
                throw new exception("[mssql::ExecQueryWithParams][Error] " + ex.Message,
                    ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB, 0, 0));
            }
        }
        public override response ExecProcWithParams(string _proc_name, string[] parameter = null, SqlDbType[] tipo = null, object[] valor = null, ParameterDirection Direcao = ParameterDirection.Input)
        {
            if (!is_connected())
                return new response();

            try
            {
                return ExecuteParameterized(_proc_name, parameter, tipo, valor, Direcao, CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                HandleConnectionFailure(ex);
                throw new exception("[mssql::ExecProcWithParams][Error] " + ex.Message,
                    ExceptionError.STDA_MAKE_ERROR_TYPE(STDA_ERROR_TYPE.PANGYA_DB, 0, 0));
            }
        }

        private response ExecuteParameterized(string commandText, string[] names, SqlDbType[] types, object[] values,
            ParameterDirection direction, CommandType commandType)
        {
            names = names ?? new string[0];
            types = types ?? new SqlDbType[0];
            values = values ?? new object[0];
            ValidateParameters(names, types, values);

            using (var connection = new SqlConnection(m_ctx_db.CreateStrConnection()))
            using (var command = new SqlCommand(commandType == CommandType.StoredProcedure
                ? (commandText.Contains(".") ? commandText : m_ctx_db.db_name + "." + commandText)
                : commandText, connection))
            using (var adapter = new SqlDataAdapter(command))
            using (var dataSet = new DataSet())
            {
                command.CommandType = commandType;
                command.CommandTimeout = 300;

                for (var i = 0; i < names.Length; i++)
                {
                    var parameter = command.Parameters.Add(names[i], types[i]);
                    parameter.Direction = direction;
                    parameter.Value = values[i] ?? DBNull.Value;
                    if (types[i] == SqlDbType.NVarChar || types[i] == SqlDbType.VarChar)
                        parameter.Size = Math.Max(1, Math.Min(4000, Convert.ToString(values[i] ?? string.Empty).Length));
                }

                connection.Open();
                adapter.Fill(dataSet, m_ctx_db.db_name);
                return ToResponse(dataSet);
            }
        }

        private static void ValidateParameters(string[] names, SqlDbType[] types, object[] values)
        {
            if (names.Length != types.Length || names.Length != values.Length)
                throw new ArgumentException("Parameter names, types, and values must have identical lengths.");
            if (names.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Parameter names cannot be empty.");
        }

        private static response ToResponse(DataSet dataSet)
        {
            var result = new response();
            if (dataSet.Tables.Count == 0)
            {
                result.addResultSet(new result_set((uint)result_set.STATE_TYPE._NO_DATA, 0, 0, null));
                return result;
            }

            foreach (DataTable table in dataSet.Tables)
            {
                result.setRowsAffected(table.Rows.Count);
                foreach (DataRow row in table.Rows)
                    result.addResultSet(new result_set((uint)result_set.STATE_TYPE.HAVE_DATA,
                        (uint)table.Rows.Count, (uint)table.Columns.Count, row));
            }
            return result;
        }

        private static bool TryBeginConnectionAttempt()
        {
            lock (s_connectionRetryLock)
            {
                if (s_connectionOutage)
                {
                    if (s_connectionAttemptInProgress || DateTime.UtcNow < s_nextConnectionAttemptUtc)
                        return false;

                    s_connectionAttemptInProgress = true;
                }
                return true;
            }
        }

        private static bool CompleteConnectionAttempt()
        {
            lock (s_connectionRetryLock)
            {
                s_connectionAttemptInProgress = false;
                bool recovered = s_connectionOutage;
                s_connectionOutage = false;
                s_nextConnectionAttemptUtc = DateTime.MinValue;
                return recovered;
            }
        }

        private bool HandleConnectionFailure(Exception exception)
        {
            if (!IsConnectionFailure(exception))
                return false;

            m_connected = false;
            if (RecordConnectionFailure())
            {
                PangyaLog.Write("[mssql::Connection][Error] " + exception.Message
                    + "] Further connection attempts will be paused for 30 seconds.",
                    LogDestination.GeneralFile | LogDestination.Console);
            }
            return true;
        }

        private static bool RecordConnectionFailure()
        {
            lock (s_connectionRetryLock)
            {
                DateTime now = DateTime.UtcNow;
                bool shouldLog = !s_connectionOutage || now >= s_nextConnectionAttemptUtc;

                s_connectionAttemptInProgress = false;
                s_connectionOutage = true;
                s_nextConnectionAttemptUtc = now.Add(s_connectionRetryDelay);
                return shouldLog;
            }
        }

        private static bool IsConnectionFailure(Exception exception)
        {
            if (exception is not SqlException sqlException)
                return false;

            if (sqlException.Class >= 20)
                return true;

            return sqlException.Number == 2
                || sqlException.Number == 20
                || sqlException.Number == 53
                || sqlException.Number == 64
                || sqlException.Number == 233
                || sqlException.Number == 258
                || sqlException.Number == 10053
                || sqlException.Number == 10054
                || sqlException.Number == 10060
                || sqlException.Number == 11001;
        }

        public override string makeEscapeKeyword(string _value)
        {
            return "[" + _value + "]";
        }

        public mssql(ctx_db _m_ctx_db) : base(_m_ctx_db)
        {
            connect();
        }

        protected void HandleDiagnosticRecord(string query)
        {

            Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                if (m_ctx_db._mssql.hDbc != null)
                {
                    if (string.IsNullOrEmpty(m_ctx_db._mssql.hDbc.ConnectionString))
                        m_ctx_db._mssql.hDbc.ConnectionString = m_ctx_db.CreateStrConnection();

                    // RECRIAR o DataSet antes de popular
                    m_ctx_db._mssql.hStmt = new DataSet();

                    var da = new SqlDataAdapter(query, m_ctx_db._mssql.hDbc);
                    da.Fill(m_ctx_db._mssql.hStmt, m_ctx_db.db_name);
                }
            }
            catch (exception ex)
            {
                PangyaLog.Write("[mssql::HandleDiagnosticQuery][Error] " + ex.getFullMessageError() + "]", LogDestination.GeneralFile | LogDestination.Console);
            }
            finally
            {
                stopwatch.Stop();
                //Debug.WriteLine($"[HandleDiagnosticRecord][Log] Tempo de execução: {stopwatch.ElapsedMilliseconds}ms");

            }

        }

        protected void HandleDiagnosticRecord(string _proc_name, string valores = null)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {

                if (string.IsNullOrEmpty(m_ctx_db._mssql.hDbc.ConnectionString))
                {
                    m_ctx_db._mssql.hDbc.Close();
                    m_ctx_db._mssql.hDbc = new SqlConnection(m_ctx_db.CreateStrConnection());
                    m_ctx_db._mssql.hDbc.Open();
                }

                // RECRIAR o DataSet antes de popular
                m_ctx_db._mssql.hStmt = new DataSet();

                // Montar a string de comando para execução do procedimento
                var commandText = $"EXEC {m_ctx_db.db_name}.{_proc_name} ";

                if (!string.IsNullOrEmpty(valores))
                {
                    // Verifica se os valores estão no formato de uma sequência separada por '|'
                    if (valores.Contains("|"))
                    {
                        // Dividindo corretamente pelos pipes '|'
                        var valorArray = valores.Split('|')
                                                .Select(v => v.Trim()) // Remover espaços em branco
                                                .Select(v => v.ToUpper() == "NULL" ? "NULL" : $"N'{v.Replace("'", "''")}'")
                                                .ToArray();

                        commandText += string.Join(", ", valorArray);
                    }
                    else
                    {
                        // Dividindo por vírgula ',' caso não contenha pipe
                        var valorArray = valores.Split(',')
                                                .Select(v => v.Trim())
                                                .Select(v => v.ToUpper() == "NULL" ? "NULL" : $"N'{v.Replace("'", "''")}'")
                                                .ToArray();

                        commandText += string.Join(", ", valorArray);
                    }

                }
                m_ctx_db._mssql.hEnv = new SqlCommand(commandText, m_ctx_db._mssql.hDbc);
                m_ctx_db._mssql.hEnv.CommandTimeout = 300;
                var da = new SqlDataAdapter(m_ctx_db._mssql.hEnv);
                da.Fill(m_ctx_db._mssql.hStmt, m_ctx_db.db_name);
            }
            catch (exception ex)
            {
                PangyaLog.Write("[mssql::HandleDiagnosticQuery][Error] " + ex.getFullMessageError() + "]", LogDestination.GeneralFile | LogDestination.Console);
            }

            finally
            {
                stopwatch.Stop();
                //Debug.WriteLine($"[HandleDiagnosticRecord1][Log] Tempo de execução: {stopwatch.ElapsedMilliseconds}ms");

            }
        }
        public void HandleDiagnosticRecord(string _proc_name, string[] parameter = null, SqlDbType[] tipo = null, object[] valor = null, ParameterDirection Direcao = ParameterDirection.Input, CommandType command = CommandType.StoredProcedure)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {

                m_ctx_db._mssql.hDbc = new SqlConnection(m_ctx_db.CreateStrConnection());
                m_ctx_db._mssql.hDbc.Open();

                m_ctx_db._mssql.hStmt = new DataSet();

                m_ctx_db._mssql.hEnv = new SqlCommand($"{m_ctx_db.db_name}.{_proc_name}", m_ctx_db._mssql.hDbc)
                {
                    CommandType = command,
                    CommandTimeout = 300
                };

                if (parameter != null && parameter.Length > 0)
                {
                    for (int i = 0; i < parameter.Length; i++)
                    {
                        var param = new SqlParameter
                        {
                            ParameterName = parameter[i],
                            SqlDbType = tipo[i],
                            Direction = Direcao,
                            Value = (valor[i] is Guid g && g == Guid.Empty) ? DBNull.Value : valor[i]
                        };


                        if (tipo[i] == SqlDbType.NVarChar || tipo[i] == SqlDbType.VarChar)
                            param.Size = 1024;

                        m_ctx_db._mssql.hEnv.Parameters.Add(param);
                    }
                }

                var da = new SqlDataAdapter(m_ctx_db._mssql.hEnv);
                da.Fill(m_ctx_db._mssql.hStmt, m_ctx_db.db_name);
            }
            catch (SqlException ex)
            {
                throw new Exception("[HandleDiagnosticRecord][SqlException] " + ex.Message, ex);
            }

            finally
            {
                stopwatch.Stop();
                Debug.WriteLine($"[HandleDiagnosticRecord2][Log] Tempo de execução: {stopwatch.ElapsedMilliseconds}ms");
            }
        }
    }
}
