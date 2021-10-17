using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Service.Services {

    public class EpisodeDefaulter : IEntityDefaulter<Episode> {

        public async Task DefaultAsync(Episode entity, User user) {
            await Task.CompletedTask;
        }

    }

}
