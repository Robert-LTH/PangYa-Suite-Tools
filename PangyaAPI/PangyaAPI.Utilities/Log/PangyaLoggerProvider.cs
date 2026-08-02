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

    public sealed class PangyaLoggerProvider : ILoggerProvider
    {
        private readonly object _sync = new object();
        private readonly PangyaLoggerOptions _options;
        private readonly Dictionary<LogDestination, StreamWriter> _writers = new Dictionary<LogDestination, StreamWriter>();
        private DateTime _writerDay;
        private string _fileStamp;
        private volatile bool _disposed;

        public PangyaLoggerProvider(PangyaLoggerOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _options.TimeProvider ??= TimeProvider.System;
            _options.ConsoleWriter ??= Console.Out;
        }

        public ILogger CreateLogger(string categoryName) => new PangyaLogger(this, categoryName);

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;
                _disposed = true;
                CloseWriters();
            }
            PangyaLog.Reset();
        }

        internal void Write<TState>(
            string category,
            LogLevel level,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (_disposed)
                return;

            var destinations = GetDestinations(eventId);
            var message = formatter(state, exception);
            var now = _options.TimeProvider.GetLocalNow();
            var line = FormatLine(now, level, category, eventId, message, exception);

            lock (_sync)
            {
                if (_disposed)
                    return;

                if ((destinations & LogDestination.Debugger) != 0)
                    Debug.WriteLine(line);

                if ((destinations & LogDestination.Console) != 0)
                    WriteConsole(line, level);

                WriteFile(LogDestination.GeneralFile, destinations, now.DateTime, line);
                WriteFile(LogDestination.TimedFile, destinations, now.DateTime, line);
                WriteFile(LogDestination.IoDataFile, destinations, now.DateTime, line);
#if DEBUG
                WriteFile(LogDestination.TestFile, destinations, now.DateTime, line);
#endif
            }
        }

        private static LogDestination GetDestinations(EventId eventId) => eventId.Id switch
        {
            1001 => LogDestination.Console,
            1002 => LogDestination.TimedFile | LogDestination.Console,
            1003 => LogDestination.GeneralFile | LogDestination.Console,
            1004 => LogDestination.GeneralFile,
            1005 => LogDestination.TimedFile,
            1006 => LogDestination.IoDataFile,
            1007 => LogDestination.IoDataFile | LogDestination.Console,
            1008 => LogDestination.TestFile,
            1009 => LogDestination.TestFile | LogDestination.Console,
            1010 => LogDestination.Debugger,
            _ => LogDestination.Default
        };

        private static string FormatLine(DateTimeOffset now, LogLevel level, string category, EventId eventId, string message, Exception exception)
        {
            var text = $"{now:yyyy-MM-dd HH:mm:ss.fff zzz} [{level}] {category} ({eventId.Id}) {message}";
            if (exception != null)
                text += Environment.NewLine + exception;
            return LogRedactor.Redact(text);
        }

        private void WriteConsole(string line, LogLevel level)
        {
            var useColor = ReferenceEquals(_options.ConsoleWriter, Console.Out) && !Console.IsOutputRedirected;
            if (useColor)
            {
                try
                {
                    Console.ForegroundColor = GetColor(level);
                }
                catch
                {
                    useColor = false;
                }
            }

            _options.ConsoleWriter.WriteLine(line);
            _options.ConsoleWriter.Flush();

            if (useColor)
                Console.ResetColor();
        }

        private void WriteFile(LogDestination destination, LogDestination requested, DateTime now, string line)
        {
            if ((requested & destination) == 0)
                return;

            EnsureCurrentDay(now);
            var writer = GetWriter(destination);
            writer.WriteLine(line);
            writer.Flush();
        }

        private void EnsureCurrentDay(DateTime now)
        {
            if (_writers.Count > 0 && _writerDay.Date != now.Date)
                CloseWriters();

            if (_writers.Count == 0)
            {
                _writerDay = now.Date;
                _fileStamp = now.ToString("ddMMyyyyHHmmss");
            }
        }

        private StreamWriter GetWriter(LogDestination destination)
        {
            if (_writers.TryGetValue(destination, out var writer))
                return writer;

            var directory = string.IsNullOrWhiteSpace(_options.Directory) ? "Log" : _options.Directory;
            System.IO.Directory.CreateDirectory(directory);
            var prefix = destination switch
            {
                LogDestination.TimedFile => "log_time",
                LogDestination.IoDataFile => "log_io_data",
                LogDestination.TestFile => "log_test",
                _ => "log"
            };
            var path = Path.Combine(directory, $"{prefix} {_fileStamp}.log");
            writer = new StreamWriter(new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
            _writers.Add(destination, writer);
            return writer;
        }

        private void CloseWriters()
        {
            foreach (var writer in _writers.Values)
                writer.Dispose();
            _writers.Clear();
        }

        private static ConsoleColor GetColor(LogLevel level) => level switch
        {
            LogLevel.Critical => ConsoleColor.Red,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Trace => ConsoleColor.DarkGray,
            _ => ConsoleColor.Gray
        };

        private sealed class PangyaLogger : ILogger
        {
            private readonly PangyaLoggerProvider _provider;
            private readonly string _category;

            public PangyaLogger(PangyaLoggerProvider provider, string category)
            {
                _provider = provider;
                _category = category;
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (formatter != null && IsEnabled(logLevel))
                    _provider.Write(_category, logLevel, eventId, state, exception, formatter);
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new NullScope();
            public void Dispose() { }
        }
    }
}
