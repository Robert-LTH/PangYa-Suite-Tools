using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PangyaAPI.Network.PangyaServer;
using PangyaAPI.Network.Resources;

namespace PangyaAPI.Network.Hosting
{
    public sealed class ServerHostedService<TServer> : BackgroundService where TServer : class, IServerRuntime
    {
        private readonly TServer _server;
        private readonly IServerConsole _console;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public ServerHostedService(
            TServer server,
            IServerConsole console,
            IHostApplicationLifetime applicationLifetime)
        {
            _server = server;
            _console = console;
            _applicationLifetime = applicationLifetime;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _server.StartAsync(stoppingToken).ConfigureAwait(false);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var input = await _console.ReadLineAsync(CreateMenuFrame(), stoppingToken).ConfigureAwait(false);
                    if (input == null)
                    {
                        _applicationLifetime.StopApplication();
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(input))
                        continue;

                    if (!int.TryParse(input.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var selection))
                    {
                        _console.WriteLine(NetworkMessages.Get("InvalidMenuSelection"));
                        continue;
                    }

                    var commandCount = _server.ConsoleCommands.Count;
                    if (selection >= 1 && selection <= commandCount)
                    {
                        await ExecuteCommandAsync(_server.ConsoleCommands[selection - 1], stoppingToken).ConfigureAwait(false);
                        continue;
                    }

                    if (selection == commandCount + 1)
                        continue;

                    if (selection == commandCount + 2)
                    {
                        TryClearConsole();
                        continue;
                    }

                    if (selection == commandCount + 3)
                    {
                        _applicationLifetime.StopApplication();
                        return;
                    }

                    _console.WriteLine(NetworkMessages.Get("InvalidMenuSelection"));
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal hosted-service shutdown.
            }
        }

        private IReadOnlyList<string> CreateMenuFrame()
        {
            var lines = new List<string>
            {
                string.Empty,
                NetworkMessages.Format("MenuTitle", typeof(TServer).Name)
            };

            for (var index = 0; index < _server.ConsoleCommands.Count; index++)
            {
                var command = _server.ConsoleCommands[index];
                var description = string.IsNullOrWhiteSpace(command.Description)
                    ? string.Empty
                    : NetworkMessages.Format("MenuDescription", command.Description);
                lines.Add(NetworkMessages.Format("MenuEntry", index + 1, command.Name + description));
            }

            var hostOption = _server.ConsoleCommands.Count + 1;
            lines.Add(NetworkMessages.Format("MenuEntry", hostOption, NetworkMessages.Get("RedisplayMenu")));
            lines.Add(NetworkMessages.Format("MenuEntry", hostOption + 1, NetworkMessages.Get("ClearConsole")));
            lines.Add(NetworkMessages.Format("MenuEntry", hostOption + 2, NetworkMessages.Get("StopServer")));
            lines.Add(NetworkMessages.Get("SelectOption"));
            return lines;
        }

        private async Task ExecuteCommandAsync(ServerConsoleCommand command, CancellationToken cancellationToken)
        {
            var tokens = new List<string>(command.CommandTokens);

            foreach (var argument in command.Arguments)
            {
                var argumentTokens = await ReadArgumentAsync(argument, cancellationToken).ConfigureAwait(false);
                if (argumentTokens == null)
                {
                    _console.WriteLine(NetworkMessages.Get("CommandCancelled"));
                    return;
                }

                tokens.AddRange(argumentTokens);
            }

            try
            {
                _server.CheckCommand(new Queue<string>(tokens));
            }
            catch (Exception exception)
            {
                _console.WriteLine(NetworkMessages.Format("CommandFailed", exception.Message));
            }
        }

        private async Task<IReadOnlyList<string>> ReadArgumentAsync(
            ServerConsoleArgument argument,
            CancellationToken cancellationToken)
        {
            if (argument.Kind == ServerConsoleArgumentKind.Choice)
            {
                while (true)
                {
                    var frame = new List<string> { argument.Prompt };
                    for (var index = 0; index < argument.Choices.Count; index++)
                        frame.Add(NetworkMessages.Format("MenuEntry", index + 1, argument.Choices[index].Name));
                    frame.Add(NetworkMessages.Get("ChoiceCancelPrompt"));

                    var input = await _console.ReadLineAsync(frame, cancellationToken).ConfigureAwait(false);
                    if (input == null)
                    {
                        _applicationLifetime.StopApplication();
                        return null;
                    }
                    if (string.IsNullOrWhiteSpace(input))
                        return null;

                    if (int.TryParse(input.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var selection)
                        && selection >= 1
                        && selection <= argument.Choices.Count)
                    {
                        return argument.Choices[selection - 1].Tokens;
                    }

                    _console.WriteLine(NetworkMessages.Get("InvalidChoice"));
                }
            }

            while (true)
            {
                var frame = new[]
                {
                    NetworkMessages.Format("RangePrompt", argument.Prompt, argument.Minimum, argument.Maximum)
                };
                var input = await _console.ReadLineAsync(frame, cancellationToken).ConfigureAwait(false);
                if (input == null)
                {
                    _applicationLifetime.StopApplication();
                    return null;
                }
                if (string.IsNullOrWhiteSpace(input))
                    return null;

                if (uint.TryParse(input.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var value)
                    && value >= argument.Minimum
                    && value <= argument.Maximum)
                {
                    return new[] { value.ToString(CultureInfo.InvariantCulture) };
                }

                _console.WriteLine(NetworkMessages.Get("InvalidRangeValue"));
            }
        }

        private void TryClearConsole()
        {
            try
            {
                _console.Clear();
            }
            catch (Exception exception) when (exception is IOException
                || exception is InvalidOperationException
                || exception is PlatformNotSupportedException)
            {
                _console.WriteLine(NetworkMessages.Format("ConsoleClearFailed", exception.Message));
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _server.StopAsync(cancellationToken).ConfigureAwait(false);
            await base.StopAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
