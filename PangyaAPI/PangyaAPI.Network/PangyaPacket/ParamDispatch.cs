using System;
using System.Collections.Generic;
using System.Diagnostics;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Utilities;

namespace PangyaAPI.Network.PangyaPacket
{
    /// <summary>
    /// get packet and Session
    /// </summary>
    public class ParamDispatch
    {
        public ParamDispatch(Session session, packet packet)
        {
            _session = session;
            _packet = packet;
        }
        public ParamDispatch()
        {

        }

        public Session _session { get; set; }
        public packet _packet { get; set; }
    }
}
