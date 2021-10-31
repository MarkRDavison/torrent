using System.Collections.Generic;

namespace Zeno.Torrent.API.Data.Models {
    public class CompletedMedia {
        public Download Download { get; set; }
        public string DownloadType { get; set; }
        public Show Show { get; set; }
        public List<TVFilenameInfo> TvInfo { get; } = new List<TVFilenameInfo>();
    }

}
