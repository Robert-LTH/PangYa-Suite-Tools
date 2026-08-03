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

    public static class PangyaLoggerExtensions
    {
        public static void Write(this ILogger logger, string message, LogDestination destinations = LogDestination.Default, Exception exception = null)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            var level = PangyaLog.GetLevel(message);
            var text = message ?? string.Empty;
            switch (destinations)
            {
                case LogDestination.Console:
                    PangyaLogMessages.Console(logger, level, text, exception);
                    break;
                case LogDestination.TimedFile | LogDestination.Console:
                    PangyaLogMessages.TimedAndConsole(logger, level, text, exception);
                    break;
                case LogDestination.GeneralFile | LogDestination.Console:
                    PangyaLogMessages.GeneralAndConsole(logger, level, text, exception);
                    break;
                case LogDestination.GeneralFile:
                    PangyaLogMessages.General(logger, level, text, exception);
                    break;
                case LogDestination.TimedFile:
                    PangyaLogMessages.Timed(logger, level, text, exception);
                    break;
                case LogDestination.IoDataFile:
                    PangyaLogMessages.IoData(logger, level, text, exception);
                    break;
                case LogDestination.IoDataFile | LogDestination.Console:
                    PangyaLogMessages.IoDataAndConsole(logger, level, text, exception);
                    break;
                case LogDestination.TestFile:
                    PangyaLogMessages.Test(logger, level, text, exception);
                    break;
                case LogDestination.TestFile | LogDestination.Console:
                    PangyaLogMessages.TestAndConsole(logger, level, text, exception);
                    break;
                case LogDestination.Debugger:
                    PangyaLogMessages.Debugger(logger, level, text, exception);
                    break;
                default:
                    PangyaLogMessages.Default(logger, level, text, exception);
                    break;
            }
        }

    }
}
