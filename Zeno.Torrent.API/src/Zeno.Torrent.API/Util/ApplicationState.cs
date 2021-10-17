namespace Zeno.Torrent.API.Util {
    public interface IApplicationState {
        bool? Started { get; set; }
        bool? Ready { get; set; }
        bool? Healthy { get; set; }
    }

    public class ApplicationState : IApplicationState {
        public bool? Started { get; set; } = null;
        public bool? Ready { get; set; } = null;
        public bool? Healthy { get; set; } = true;
    }
}
