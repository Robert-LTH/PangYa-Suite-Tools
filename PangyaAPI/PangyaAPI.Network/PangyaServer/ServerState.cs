using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using PangyaAPI.Network.Repository;
using PangyaAPI.Network.Configuration;
using PangyaAPI.Network.Models;
using PangyaAPI.Network.PangyaPacket;
using PangyaAPI.Network.PangyaSession;
using PangyaAPI.Network.PangyaUnit;
using PangyaAPI.Network.PangyaUtil;
using PangyaAPI.Network.Hosting;
using PangyaAPI.SQL;
using PangyaAPI.Utilities;
using PangyaAPI.Utilities.BinaryModels;
using PangyaAPI.Utilities.Log;
using Microsoft.Extensions.Logging;
namespace PangyaAPI.Network.PangyaServer
{
    public enum ServerState
    {
        Uninitialized,
        Good,
        GoodWithWarning,
        Initialized,
        Failure
    }

    // Server Static
    //namespace ssv
    //{
    //    public abstract partial class sv : Singleton<Server>
    //    {
    //    }
    //}
}
