namespace Zeno.Torrent.API.Core.Configuration {
    public class AppSettings {
        public static string ZENO_TORRENT_DAEMON = "ZENO_TORRENT_DAEMON";
        public string TORRENT_ENGINE_SAVE_PATH { get; set; }
        public string TORRENT_ENGINE_MOVIE_PATH { get; set; }
        public string TORRENT_ENGINE_TV_PATH { get; set; }
        public string MEDIA_EXTENSIONS { get; set; }
        public string[] SplitMediaExtensions => MEDIA_EXTENSIONS.Split(",");
        public string DATABASE_TYPE { get; set; }
        public string CONNECTION_STRING { get; set; }
        public string DATABASE_HOST { get; set; }
        public string DATABASE_PORT { get; set; }
        public string DATABASE_NAME { get; set; }
        public string DATABASE_USER { get; set; }
        public string DATABASE_PASSWORD { get; set; }
        public string URL { get; set; }
        public string FEED_URL { get; set; }
        public string CRON_JOB_PARAM { get; set; }
        public bool AUTO_COMPLETE { get; set; }
        public string AUTHORITY { get; set; }
        public int NUM_DOWNLOADS { get; set; }
        public string BFF_ORIGIN { get; set; }
    }
}
