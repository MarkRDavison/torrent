using System.Collections.Generic;
using System.Linq;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services {

    public class EpisodeValidator : EntityValidator<Episode> {

        public override IEnumerable<string> Validate(Episode entity) {
            return Enumerable.Empty<string>();
        }

    }

}
