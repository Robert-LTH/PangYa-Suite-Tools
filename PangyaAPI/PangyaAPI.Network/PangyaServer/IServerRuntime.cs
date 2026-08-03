using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PangyaAPI.Network.Hosting;

namespace PangyaAPI.Network.PangyaServer
{
    public interface IServerRuntime
    {
        IReadOnlyList<ServerConsoleCommand> ConsoleCommands { get; }
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
        bool CheckCommand(Queue<string> command);
    }
}
