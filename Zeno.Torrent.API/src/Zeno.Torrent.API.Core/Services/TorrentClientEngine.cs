using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Framework.Instrumentation;
using Zeno.Torrent.API.Framework.Utility;

namespace Zeno.Torrent.API.Core.Services {

    public class TorrentClientEngine : ITorrentClientEngine {

        private readonly ILogger logger;
        private readonly IFileOperations fileOperations;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IWrappedTorrent wrappedTorrent;

        private bool disposedValue;

        /// <summary>
        /// Creates a new instance of the <see cref="TorrentClientEngine"/> class.
        /// </summary>
        public TorrentClientEngine(
            ILogger<TorrentClientEngine> logger,
            IFileOperations fileOperations,
            IServiceScopeFactory serviceScopeFactory,
            IWrappedTorrent wrappedTorrent
        ) {
            this.logger = logger;
            this.fileOperations = fileOperations;
            this.serviceScopeFactory = serviceScopeFactory;
            this.wrappedTorrent = wrappedTorrent;
        }


        /// <inheritdoc/>
        public async Task Initialize(CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                await wrappedTorrent.Initialize(cancellationToken);
                wrappedTorrent.OnDownloadStateChanged += HandleDownloadStateChanged;
            }
        }

        private async void HandleDownloadStateChanged(object sender, DownloadStateChangedEventArgs e) {
            using (logger.ProfileOperation()) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var interactionService = scope.ServiceProvider.GetRequiredService<IDownloadInteractionService>();
                    switch (e.NewState) {
                        case Constants.DownloadState.Added:
                            await interactionService.HandleDownloadAdded(e, CancellationToken.None);
                            break;
                        case Constants.DownloadState.Downloading:
                            await interactionService.HandleDownloadDownloading(e, CancellationToken.None);
                            break;
                        case Constants.DownloadState.Deleted:
                            await interactionService.HandleDownloadDeleted(e, CancellationToken.None);
                            break;
                        case Constants.DownloadState.Complete:
                            await interactionService.HandleDownloadComplete(e, CancellationToken.None);
                            break;
                        case Constants.DownloadState.Error:
                            await interactionService.HandleDownloadError(e, CancellationToken.None);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async Task Teardown(CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                await wrappedTorrent.Teardown(cancellationToken);
                wrappedTorrent.OnDownloadStateChanged -= HandleDownloadStateChanged;
            }
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    wrappedTorrent?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

    }

}
