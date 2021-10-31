using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services.Interfaces {

    public interface IMediaNotifier {

        public Task NotifyCompletedMedia(CompletedMedia media, CancellationToken cancellationToken);

    }

}
