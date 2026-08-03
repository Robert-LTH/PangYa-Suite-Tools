using System;
using System.Threading;
using response = PangyaAPI.SQL.Response;
using PangyaAPI.SQL.Resources;

namespace PangyaAPI.SQL.Manager
{
    public class exec_query : IDisposable
    {
        public enum QUERY_TYPE : byte
        {
            _QUERY,
            _PROCEDURE,
            _INSERT,
            _UPDATE,
            _DELETE
        }

        public exec_query()
        {
            m_event = new ManualResetEventSlim(false);
        }

        public exec_query(string query, byte type = 0) : this()
        {
            setQuery(query);
            m_type = type;
        }

        public exec_query(string procedureName, string[] parameters,
            byte type = (byte)QUERY_TYPE._PROCEDURE) : this(procedureName, type)
        {
            setParam(parameters);
        }

        public string getQuery() { return m_query; }
        public string[] getParams() { return m_params == null ? null : (string[])m_params.Clone(); }
        public response getRes() { return m_res; }
        public byte getType() { return m_type; }

        public void setQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException(SqlMessages.Get("QueryRequired"), nameof(query));
            m_query = query;
        }

        public void setParam(string[] parameters)
        {
            m_params = parameters == null ? new string[0] : (string[])parameters.Clone();
        }

        public void setRes(response result) { m_res = result; }

        public void waitEvent(int milliseconds = -1)
        {
            if (!m_event.Wait(milliseconds))
                throw new TimeoutException(SqlMessages.Get("QueryTimeout"));
        }

        public void setEvent() { m_event.Set(); }
        public void resetEvent() { m_event.Reset(); }
        public void pulseEvent() { m_event.Set(); }

        public static void enter() { m_gate.Wait(); }
        public static void release() { m_gate.Release(); }
        public static int getSpinCount() { return m_spin_count; }

        public void Dispose() { m_event.Dispose(); }

        protected string m_query;
        protected string[] m_params;
        protected response m_res;
        protected byte m_type;
        protected static int m_spin_count;

        private readonly ManualResetEventSlim m_event;
        private static readonly SemaphoreSlim m_gate = new SemaphoreSlim(1, 1);
    }
}
