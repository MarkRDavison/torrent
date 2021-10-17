using System;
using System.Threading;
using System.Threading.Tasks;

namespace Zeno.Torrent.API.Core.Services.Interfaces {

    public interface ITorrentClientEngine : IDisposable {
        Task Initialize(CancellationToken cancellationToken);
        Task Teardown(CancellationToken cancellationToken);

    }

}
