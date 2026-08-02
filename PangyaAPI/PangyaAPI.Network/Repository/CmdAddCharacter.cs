using System;
using PangyaAPI.Network.Models;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Repository
{
    public class CmdAddCharacter : CmdAddItemBase
    {
        CharacterInfo m_ci;
        public CmdAddCharacter(uint _uid, CharacterInfo _ci, byte _purchase, byte _gift_flag) : base(_uid, _purchase, _gift_flag)
        {
            m_uid = _uid;
            m_ci = _ci;
        }

        protected override void lineResult(ctx_res _result, uint _index_result)
        {
            checkColumnNumber(1);
            try
            {
                m_ci.id = (int)(_result.data[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

        protected override Response prepareConsulta()
        {

            var values = new object[31];
            values[0] = m_uid;
            values[1] = m_ci.id;
            values[2] = m_ci._typeid;
            values[3] = m_ci.default_hair;
            values[4] = m_ci.default_shirts;
            values[5] = m_purchase;
            values[6] = m_gift_flag;
            for (var index = 0; index < 24; index++)
                values[index + 7] = m_ci.parts_typeid[index];

            var r = procedure("pangya.ProcAddCharacter", values);
            checkResponse(r, "nao conseguiu adicionar o character[TYPEID=" + (m_ci._typeid) + "] para o player: " + (m_uid));
            return r;
        }

        public CharacterInfo getInfo()
        {
            return m_ci;
        }

        public void setInfo(CharacterInfo _ci)
        {
            m_ci = _ci;
        }
    }
}
