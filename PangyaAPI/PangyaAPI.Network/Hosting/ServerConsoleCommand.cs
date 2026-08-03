using System;
using System.Collections.Generic;
using PangyaAPI.Network.Resources;

namespace PangyaAPI.Network.Hosting
{
    public sealed class ServerConsoleCommand
    {
        public ServerConsoleCommand(
            string name,
            string description,
            IReadOnlyList<string> commandTokens,
            IReadOnlyList<ServerConsoleArgument> arguments = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(NetworkMessages.Get("CommandNameRequired"), nameof(name));
            if (commandTokens == null || commandTokens.Count == 0)
                throw new ArgumentException(NetworkMessages.Get("CommandTokenRequired"), nameof(commandTokens));

            Name = name;
            Description = description ?? string.Empty;
            CommandTokens = commandTokens;
            Arguments = arguments ?? Array.Empty<ServerConsoleArgument>();
        }

        public string Name { get; }
        public string Description { get; }
        public IReadOnlyList<string> CommandTokens { get; }
        public IReadOnlyList<ServerConsoleArgument> Arguments { get; }
    }
}
