using PangyaAPI.SQL.Manager;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using response = PangyaAPI.SQL.Response;

namespace PangyaAPI.SQL
{
    public class call_db_cmd_st : IDisposable
    {
        private Mutex m_hMutex;
        private readonly string url_log = @"call_db_cmd.log";

        public call_db_cmd_st()
        {
            try
            {
                // Mutex nomeado, equivalente ao CreateMutexA
                m_hMutex = new Mutex(false, "xg_CALL_DB_CMD_LOG");
            }
            catch (Exception ex)
            {
                Console.WriteLine("[pangya_db::call_db_cmd_st::call_db_cmd_st][Error] fail to create Mutex. Error: " + ex.Message);
            }
        }

        ~call_db_cmd_st()
        {
            Dispose();
        }

        public Dictionary<string, string> loadCmds()
        {
            var v_cmds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!isValid() || !lock_())
                return v_cmds;

            try
            {
                if (File.Exists(url_log))
                {
                    using (var inFile = new StreamReader(url_log))
                    {
                        string line;
                        while ((line = inFile.ReadLine()) != null)
                        {
                            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (parts.Length >= 2)
                            {
                                // junta o resto se por acaso tiver mais de 2 pedaços
                                var name = parts[0];
                                var value = string.Join(" ", parts, 1, parts.Length - 1);

                                v_cmds[name] = value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[pangya_db::call_db_cmd_st::loadCmds][Error] " + ex.Message);
            }
            finally
            {
                if (!unlock())
                    Console.WriteLine("[pangya_db::call_db_cmd_st::loadCmds][Error] fail to release Mutex.");
            }

            return v_cmds;
        }

        public void saveCmds(Dictionary<string, string> _cmds)
        {
            if (_cmds == null || _cmds.Count == 0 || !isValid() || !lock_())
                return;

            try
            {
                using (var outFile = new StreamWriter(url_log, false))
                {
                    foreach (var el in _cmds)
                    {
                        outFile.WriteLine($"{el.Key} {el.Value}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[pangya_db::call_db_cmd_st::saveCmds][Error] " + ex.Message);
            }
            finally
            {
                if (!unlock())
                    Console.WriteLine("[pangya_db::call_db_cmd_st::saveCmds][Error] fail to release Mutex.");
            }
        }

        private bool isValid()
        {
            return m_hMutex != null;
        }

        private bool lock_()
        {
            if (!isValid())
                return false;

            try
            {
                return m_hMutex.WaitOne(20); // espera até 20ms, depois retorna false
            }
            catch
            {
                return false;
            }
        }

        private bool unlock()
        {
            if (!isValid())
                return false;

            try
            {
                m_hMutex.ReleaseMutex();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            m_hMutex?.Dispose();
            m_hMutex = null;
        }
    }
}
