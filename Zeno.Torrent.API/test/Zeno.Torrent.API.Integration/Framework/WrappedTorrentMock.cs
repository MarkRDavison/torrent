using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Data.DTOs;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Integration.Framework {

    [ExcludeFromCodeCoverage]
    public class WrappedTorrentMock : IWrappedTorrent {

        public async Task Initialize(CancellationToken cancellationToken) {
            await Task.CompletedTask;
        }

        public async Task Teardown(CancellationToken cancellationToken) {
            await Task.CompletedTask;
        }

        public void Dispose() {

        }

        public async Task<Download> RegisterDownload(Download download, CancellationToken cancellationToken) {
            await Task.CompletedTask;
            return null;
        }

        public async Task RemoveDownload(Download download, CancellationToken cancellationToken) {
            await Task.CompletedTask;
        }

        public void NotifyDownloadStateChanged(DownloadStateChangedEventArgs eventArgs) {
            throw new NotImplementedException();
        }

        public string[] GetDownloadFiles(Download download) {
            throw new NotImplementedException();
        }

        public Task<DownloadStatus> GetDownloadStatus(Download download, CancellationToken cancellationToken) {
            return Task.FromResult(new DownloadStatus {
                Id = download.Id,
                State = download.State
            });
        }

        public EventHandler<DownloadStateChangedEventArgs> OnDownloadStateChanged { get; set; }
    }

}
