using System;
using System.Collections.Generic;
using System.Linq;
using PangyaAPI.Network.Models;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Repository
{
    public class CmdServerList : Pangya_DB
    {
        TYPE_SERVER m_type;
        List<ServerInfo> v_server_list;

        public CmdServerList(TYPE_SERVER _type)
        {
            v_server_list = new List<ServerInfo>();
            m_type = _type;
        }

        public CmdServerList()
        {
            v_server_list = new List<ServerInfo>();
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(13);//melhorar depois
            try
            {
                ServerInfo si = new ServerInfo();

                if (!string.IsNullOrEmpty(_result.data[0].ToString()))
                    si.Name = (_result.data[0].ToString());
                si.Id = int.Parse(_result.data[1].ToString());
                if (!string.IsNullOrEmpty(_result.data[2].ToString()))
                    si.IpAddress = _result.data[2].ToString();
                si.Port = int.Parse(_result.data[3].ToString());
                si.MaxUsers = int.Parse(_result.data[4].ToString());
                si.ConnectedUsers = int.Parse(_result.data[5].ToString());
                si.propriedade = (PropertyType)uint.Parse(_result.data[6].ToString());
                si.Angelic_wings_num = int.Parse(_result.data[7].ToString());
                si.flagEvent = (EventType)ushort.Parse(_result.data[8].ToString());
                si.EventMap = short.Parse(_result.data[9].ToString());
                si.ImageNumber = short.Parse(_result.data[10].ToString());
                si.AppRate = short.Parse(_result.data[11].ToString());
                si.ScratchRate = short.Parse(_result.data[12].ToString());    // Estava o rate_scratchy mas realoquei ele para o ServerInfoEx::Rate
                if (!v_server_list.Any(c => c.Id == si.Id))
                    v_server_list.Add(si);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {
            v_server_list.Clear();

            var @tipo = Convert.ToByte(m_type);
            var r = procedure("pangya.ProcGetServerList", tipo);

            checkResponse(r, "nao conseguiu pegar o server list");
            return r;
        }

        public List<ServerInfo> getServerList()
        {
            return this.v_server_list;
        }


        public TYPE_SERVER getType()
        {
            return m_type;
        }

        public void setType(TYPE_SERVER _type)
        {
            m_type = _type;
        }
    }
}
