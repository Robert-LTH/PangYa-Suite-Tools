
using System;
using PangyaAPI.Network.Models;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Repository
{
    public class CmdUpdateAuthServerKey : Pangya_DB
    {
        AuthServerKey m_ask;

        public CmdUpdateAuthServerKey(AuthServerKey _ask)
        {
            m_ask = _ask;
        }

        public CmdUpdateAuthServerKey()
        {
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {

        }

        protected override Response prepareConsulta()
        {
            if (m_ask.server_uid == 0u)
                throw new Exception("[CmdUpdateAuthServerKey::prepareConsulta][Error] AuthServerKey m_ask.server_uid is invalid(zero).");

            var r = procedureWithParams("pangya.ProcUpdateAuthServerKey",
                new[] { "@SERVER_UID", "@KEY", "@VALID" },
                new[] { System.Data.SqlDbType.Int, System.Data.SqlDbType.NVarChar, System.Data.SqlDbType.TinyInt },
                new object[] { m_ask.server_uid, string.IsNullOrEmpty(m_ask.key) ? (object)DBNull.Value : m_ask.key, m_ask.valid });

            checkResponse(r, "nao conseguiu atualizar Auth Server Key[SERVER_UID=" + (m_ask.server_uid)
                        + ", VALID=" + m_ask.valid + "]");
            return r;
        }


        public AuthServerKey getInfo()
        {
            return m_ask;
        }
    }
}
