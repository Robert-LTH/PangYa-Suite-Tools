using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PangyaAPI.SQL;
namespace PangyaAPI.Network.Repository
{
    public class CmdLogonCheck : Pangya_DB
    {
        int m_uid = -1;
        private bool m_check;
        private int m_server_uid = -1;
        public CmdLogonCheck(int _uid)
        {
            m_uid = _uid;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(2);
            try
            {
                m_check = (_result.data[0].ToString() == "1") ? true : false;
                if ((_result.data[1]) != null)
                {
                    m_server_uid = Convert.ToInt32(_result.data[1]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {
            m_check = false;
            m_server_uid = 0;
            using var context = createContext();
            var rows = context.Accounts
                .AsNoTracking()
                .Where(account => account.Uid == m_uid)
                .Select(account => new object[] { account.Logon, account.GameServerId })
                .ToArray();
            var r = responseFromRows(rows);
            checkResponse(r, "nao conseguiu verificar o logon do player: " + (m_uid));
            return r;
        }


        public int getUID()
        {
            return m_uid;
        }

        public void setUID(int _uid)
        {
            m_uid = _uid;
        }

        public bool getLastCheck()
        {
            return m_check;
        }

        public int getServerUID()
        {
            return m_server_uid;
        }
    }
}
