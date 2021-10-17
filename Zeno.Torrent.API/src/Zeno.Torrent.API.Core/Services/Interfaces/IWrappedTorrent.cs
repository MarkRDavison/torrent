using System;
using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.DTOs;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Core.Services.Interfaces {

    public struct DownloadStateChangedEventArgs {
        public string Hash { get; set; }
        public Guid Id { get; set; }
        public string OldState { get; set; }
        public string NewState { get; set; }
    }

    public interface IWrappedTorrent : IDisposable {
        Task Initialize(CancellationToken cancellationToken);
        Task Teardown(CancellationToken cancellationToken);

        Task<Download> RegisterDownload(Download download, CancellationToken cancellationToken);
        Task RemoveDownload(Download download, CancellationToken cancellationToken);

        Task<DownloadStatus> GetDownloadStatus(Download download, CancellationToken cancellationToken);

        string[] GetDownloadFiles(Download download);

        EventHandler<DownloadStateChangedEventArgs> OnDownloadStateChanged { get; set; }
        void NotifyDownloadStateChanged(DownloadStateChangedEventArgs eventArgs);
    }

}
