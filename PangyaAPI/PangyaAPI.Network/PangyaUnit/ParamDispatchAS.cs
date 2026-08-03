using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PangyaAPI.Network.Cryptor;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.Configuration;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;

namespace PangyaAPI.Network.PangyaUnit
{
    public class ParamDispatchAS
    {
        public UnitPlayer _session;
        public packet _packet;

        public ParamDispatchAS(ParamDispatch arg2)
        {
            _session = (UnitPlayer)arg2._session;
            _packet = arg2._packet;
        }
        public ParamDispatchAS()
        {
        }
    }
}
