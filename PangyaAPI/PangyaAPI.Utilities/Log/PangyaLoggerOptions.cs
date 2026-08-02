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

    public sealed class PangyaLoggerOptions
    {
        public string Directory { get; set; } = "Log";
        public TimeProvider TimeProvider { get; set; } = TimeProvider.System;
        public TextWriter ConsoleWriter { get; set; } = Console.Out;
    }
}
