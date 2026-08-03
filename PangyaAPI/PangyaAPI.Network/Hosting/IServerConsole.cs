using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PangyaAPI.Network.Hosting
{
    public interface IServerConsole
    {
        Task<string> ReadLineAsync(IReadOnlyList<string> frameLines, CancellationToken cancellationToken);
        void WriteLine(string value = "");
        void Clear();
    }
}
