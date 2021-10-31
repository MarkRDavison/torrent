namespace Zeno.Torrent.API.Data.Models {

    public struct TVFilenameInfo {
        public bool Valid =>
            Season > 0 &&
            Episode >= 0;
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Quality { get; set; }
        public bool Repack { get; set; }
    }

}
