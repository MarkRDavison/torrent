using System;

namespace Zeno.Torrent.API.Service.EventArgs {

    public class DownloadStateChangedEventArgs : System.EventArgs {

        /// <summary>
        /// Creates a new instance of the <see cref="DownloadStateChangedEventArgs"/> class.
        /// </summary>
        public DownloadStateChangedEventArgs(
            Guid downloadId,
            string oldState,
            string newState,
            string hash
        ) {
            DownloadId = downloadId;
            OldState = oldState;
            NewState = newState;
            Hash = hash;
        }

        public Guid DownloadId { get; }
        public string OldState { get; }
        public string NewState { get; }
        public string Hash { get; }

    }

}
