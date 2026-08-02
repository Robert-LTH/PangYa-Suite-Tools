#nullable disable
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace PangyaAPI.Utilities.Log
{
    [Flags]
    public enum LogDestination
    {
        None = 0,
        Console = 1,
        GeneralFile = 2,
        TimedFile = 4,
        IoDataFile = 8,
        TestFile = 16,
        Debugger = 32,
        Default = Console | GeneralFile
    }
}
