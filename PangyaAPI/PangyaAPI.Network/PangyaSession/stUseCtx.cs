using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PangyaAPI.Network.Cryptor;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaServer;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.Log;

namespace PangyaAPI.Network.PangyaSession
{    // Estrutura para sincronizar o uso de buff, para não limpar o socket(Session) antes dele ser liberado
    public class stUseCtx
    {

        private object m_cs = new object();
        protected int m_active = new int();
        protected bool m_quit;
        public stUseCtx()
        {
            clear();
        }
        public void Dispose()
        {

            clear();
            m_cs = new object();
        }
        public void clear()
        {
            Monitor.Enter(m_cs);
            m_active = 0;
            m_quit = false;
            Monitor.Exit(m_cs);
        }
        public bool isQuit()
        {

            var quit = false;
            Monitor.Enter(m_cs);
            quit = m_quit;
            Monitor.Exit(m_cs);

            return quit;
        }
        public int usa()
        {

            var spin = 0;
            Monitor.Enter(m_cs);
            spin = ++m_active;
            Monitor.Exit(m_cs);

            return spin;
        }
        public bool devolve()
        {
            Monitor.Enter(m_cs);
            --m_active;
            Monitor.Exit(m_cs);
            return m_active <= 0 && m_quit; // pode excluir(limpar) a Session
        }
        // Verifica se pode excluir a Session, se não seta a flag quit para o prox method que devolver excluir ela
        public bool checkCanQuit()
        {

            var can = false;
            Monitor.Enter(m_cs);

            if (m_active <= 0)
            {
                can = true;
            }
            else
            {
                m_quit = true;
            }

            Monitor.Exit(m_cs);
            return can;
        }
    }
}