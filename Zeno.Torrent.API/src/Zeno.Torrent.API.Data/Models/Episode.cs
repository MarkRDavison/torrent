using System;

namespace Zeno.Torrent.API.Data.Models {

    public class Episode : IEntity {
        public Guid Id { get; set; }
        public Guid ShowId { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public bool Repack { get; set; }
        public Guid DownloadId { get; set; }
    }

}
