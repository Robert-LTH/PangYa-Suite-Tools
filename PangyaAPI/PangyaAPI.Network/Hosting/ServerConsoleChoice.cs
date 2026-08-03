using System;
using System.Collections.Generic;
using PangyaAPI.Network.Resources;

namespace PangyaAPI.Network.Hosting
{

    public sealed class ServerConsoleChoice
    {
        public ServerConsoleChoice(string name, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(NetworkMessages.Get("ChoiceNameRequired"), nameof(name));
            if (tokens == null || tokens.Length == 0)
                throw new ArgumentException(NetworkMessages.Get("ChoiceTokenRequired"), nameof(tokens));

            Name = name;
            Tokens = tokens;
        }

        public string Name { get; }
        public IReadOnlyList<string> Tokens { get; }
    }
}
