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

    public static class PangyaLog
    {
        private static readonly ConcurrentDictionary<string, ILogger> Loggers = new ConcurrentDictionary<string, ILogger>(StringComparer.Ordinal);
        private static ILoggerFactory _factory;

        public static void Configure(ILoggerFactory factory)
        {
            Interlocked.Exchange(ref _factory, factory ?? throw new ArgumentNullException(nameof(factory)));
            Loggers.Clear();
        }

        public static void Reset()
        {
            Interlocked.Exchange(ref _factory, null);
            Loggers.Clear();
        }

        public static ILogger CreateLogger(string categoryName)
        {
            var factory = Volatile.Read(ref _factory);
            if (factory == null)
                return Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;

            return Loggers.GetOrAdd(categoryName ?? "Pangya", factory.CreateLogger);
        }

        public static void Write(
            string message,
            LogDestination destinations = LogDestination.Default,
            Exception exception = null,
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFile = "")
        {
            var category = string.IsNullOrEmpty(sourceFile) ? "Pangya" : Path.GetFileNameWithoutExtension(sourceFile);
            var logger = CreateLogger(category);
            logger.Write(message, destinations, exception);
        }

        internal static LogLevel GetLevel(string message)
        {
            if (string.IsNullOrEmpty(message))
                return LogLevel.Information;
            if (message.IndexOf("[Error", StringComparison.OrdinalIgnoreCase) >= 0)
                return LogLevel.Error;
            if (message.IndexOf("[Warning]", StringComparison.OrdinalIgnoreCase) >= 0
                || message.IndexOf("[WARNING]", StringComparison.OrdinalIgnoreCase) >= 0)
                return LogLevel.Warning;
            if (message.IndexOf("[Debug]", StringComparison.OrdinalIgnoreCase) >= 0)
                return LogLevel.Debug;
            return LogLevel.Information;
        }
    }
}
