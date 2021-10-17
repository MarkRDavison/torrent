using System.Collections.Generic;

namespace Zeno.Torrent.API.Data {

    public static class Constants {

        public static class Quality {
            public const string HD_1080p = "1080p";
            public const string HD_720p = "720p";

            public static List<string> All => new List<string> { HD_1080p, HD_720p };
        }

        public static class DownloadType {
            public const string Movie = "MOV";
            public const string Episode = "EP";
            public const string Season = "SEA";

            public static List<string> All => new List<string> { Movie, Episode, Season };
        }
        
        public static class DownloadState {
            public const string Added = "ADD";
            public const string Initializing = "INIT";
            public const string Downloading = "DOWN";
            public const string Complete = "COMP";
            public const string Deleted = "DEL";
            public const string Processed = "PROC";
            public const string Error = "ERR";

            public static List<string> All => new List<string> { Added, Initializing, Downloading, Complete, Deleted, Processed, Error };
        }

        public static class DownloadSource {
            public const string Manual = "M";
            public const string Automated = "A";

            public static List<string> All => new List<string> { Manual, Automated };
        }

    }

}
