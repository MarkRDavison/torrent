using System;

namespace Zeno.Torrent.API.Data.Models {

    public class Download : IEntity {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public string TorrentLocation { get; set; }
        public string OriginalUri { get; set; }
        public string State { get; set; }
        public string DownloadType { get; set; }
        public Guid DestinationTypeId { get; set; }
        public string Source { get; set; }
        public string CreatedByUserId { get; set; }
    }

}
