using System;
using System.Collections.Generic;
using PangyaAPI.Network.Resources;

namespace PangyaAPI.Network.Hosting
{

    public sealed class ServerConsoleArgument
    {
        public ServerConsoleArgument(
            string prompt,
            IReadOnlyList<ServerConsoleChoice> choices)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException(NetworkMessages.Get("ArgumentPromptRequired"), nameof(prompt));
            if (choices == null || choices.Count == 0)
                throw new ArgumentException(NetworkMessages.Get("ArgumentChoiceRequired"), nameof(choices));

            Prompt = prompt;
            Choices = choices;
            Kind = ServerConsoleArgumentKind.Choice;
        }

        public ServerConsoleArgument(string prompt, uint minimum, uint maximum)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException(NetworkMessages.Get("ArgumentPromptRequired"), nameof(prompt));
            if (minimum > maximum)
                throw new ArgumentOutOfRangeException(nameof(minimum));

            Prompt = prompt;
            Choices = Array.Empty<ServerConsoleChoice>();
            Kind = ServerConsoleArgumentKind.UnsignedInteger;
            Minimum = minimum;
            Maximum = maximum;
        }

        public string Prompt { get; }
        public ServerConsoleArgumentKind Kind { get; }
        public IReadOnlyList<ServerConsoleChoice> Choices { get; }
        public uint Minimum { get; }
        public uint Maximum { get; }
    }
}
