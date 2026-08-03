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

    public static partial class PangyaLogMessages
    {
        [LoggerMessage(EventId = 1000, Message = "{Message}")]
        public static partial void Default(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1001, Message = "{Message}")]
        public static partial void Console(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1002, Message = "{Message}")]
        public static partial void TimedAndConsole(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1003, Message = "{Message}")]
        public static partial void GeneralAndConsole(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1004, Message = "{Message}")]
        public static partial void General(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1005, Message = "{Message}")]
        public static partial void Timed(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1006, Message = "{Message}")]
        public static partial void IoData(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1007, Message = "{Message}")]
        public static partial void IoDataAndConsole(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1008, Message = "{Message}")]
        public static partial void Test(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1009, Message = "{Message}")]
        public static partial void TestAndConsole(ILogger logger, LogLevel logLevel, string message, Exception exception);
        [LoggerMessage(EventId = 1010, Message = "{Message}")]
        public static partial void Debugger(ILogger logger, LogLevel logLevel, string message, Exception exception);
    }
}
