using System;
using System.Collections.Generic;
using System.Linq;
using PangyaAPI.Network.Models;
using PangyaAPI.SQL;

namespace PangyaAPI.Network.Repository
{
    public enum TYPE_SERVER : byte
    {
        GAME,
        MSN,
        LOGIN,
        RANK,
        AUTH,
    }
}
