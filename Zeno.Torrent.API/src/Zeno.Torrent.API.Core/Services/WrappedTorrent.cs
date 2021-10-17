using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MonoTorrent.Client;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.DTOs;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Framework.Instrumentation;
using Zeno.Torrent.API.Framework.Utility;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Core.Services {

    public class WrappedTorrent : IWrappedTorrent {

        private readonly ILogger logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IFileOperations fileOperations;

        private ClientEngine clientEngine;
        private bool disposedValue;

        private IDictionary<string, TorrentManager> torrents;

        public WrappedTorrent(
            ILogger<WrappedTorrent> logger,
            IServiceScopeFactory serviceScopeFactory,
            IFileOperations fileOperations
        ) {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.fileOperations = fileOperations;
        }

        public async Task Initialize(CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var configuration = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
                    torrents = new Dictionary<string, TorrentManager>();

                    var settings = new EngineSettingsBuilder {
                        CacheDirectory = configuration.Value.TORRENT_ENGINE_SAVE_PATH
                    };
                    clientEngine = new ClientEngine(settings.ToSettings());
                    await clientEngine.StartAllAsync();
                    await clientEngine.DhtEngine.StartAsync();
                }
            }
        }
        public async Task Teardown(CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                await clientEngine?.StopAllAsync();
                await clientEngine?.DhtEngine.StopAsync();
            }
        }

        public async Task<Download> RegisterDownload(Download download, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var configuration = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
                    var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();
                    var magnet = MonoTorrent.MagnetLink.FromUri(new Uri(download.OriginalUri));
                    
                    if (string.IsNullOrEmpty(download.Hash)) {
                        download.Hash = magnet.InfoHash.ToHex();

                        download.Name = magnet.Name;
                        download.State = Constants.DownloadState.Initializing;

                        await downloadService.SaveEntityAsync(download, cancellationToken);
                    }

                    if (torrents.ContainsKey(download.Hash)) {
                        return null;
                    }

                    var settings = new TorrentSettingsBuilder {
                        UploadSlots = 8,
                        MaximumConnections = 60,
                        MaximumDownloadSpeed = 0,
                        MaximumUploadSpeed = 0,
                        AllowInitialSeeding = true                        
                    };

                    var torrentManager = await clientEngine.AddAsync(magnet, configuration.Value.TORRENT_ENGINE_SAVE_PATH, settings.ToSettings());
                    torrentManager.TorrentStateChanged += OnTorrentStateChanged;
                    //torrentManager.PieceHashed += (o, e) => {
                    //    if (e.TorrentManager.State != TorrentState.Downloading)
                    //        return;

                    //    logger.LogInformation($"Piece {e.PieceIndex} hashed progress: {e.TorrentManager?.Progress ?? e.Progress}.");
                    //};

                    await torrentManager.StartAsync();

                    torrents.Add(download.Hash, torrentManager);
                }

                NotifyDownloadStateChanged(new DownloadStateChangedEventArgs {
                    Id = download.Id,
                    Hash = download.Hash,
                    OldState = null,
                    NewState = download.State
                });

                return download;
            }
        }

        public async Task RemoveDownload(Download download, CancellationToken cancellationToken) {
            var tm = GetTorrentManager(download);
            if (tm == null) {
                return;
            }

            torrents.Remove(download.Hash);
            await tm.StopAsync();
            await clientEngine.RemoveAsync(tm, RemoveMode.CacheDataAndDownloadedData);
            fileOperations.DeleteDirectory(Path.Combine(tm.SavePath, tm.Torrent.Name));
            if (!string.IsNullOrEmpty(download.TorrentLocation)) {
                fileOperations.DeleteFile(download.TorrentLocation);
            }
        }

        private TorrentManager GetTorrentManager(Download download) {
            TorrentManager tm = null;
            if (!string.IsNullOrEmpty(download.Hash)) {
                torrents.TryGetValue(download.Hash, out tm);
            }
            return tm;
        }

        public string[] GetDownloadFiles(Download download) {
            var tm = GetTorrentManager(download);
            if (tm == null) {
                return Array.Empty<string>();
            }

            return tm.Torrent.Files.Select(f => Path.Combine(tm.SavePath, tm.Torrent.Name, f.Path)).ToArray();
        }

        public async Task<DownloadStatus> GetDownloadStatus(Download download, CancellationToken cancellationToken) {
            var tm = GetTorrentManager(download);

            double percentage = 0;
            if (download.State == Constants.DownloadState.Downloading) {
                percentage = tm?.PartialProgress ?? 0;
            }
            else if (download.State == Constants.DownloadState.Complete ||
                     download.State == Constants.DownloadState.Processed) {
                percentage = 100;
            }
            else if (download.State == Constants.DownloadState.Added ||
                     download.State == Constants.DownloadState.Initializing) {
                percentage = 0;
            }
            
            var status = new DownloadStatus { 
                Id = download.Id,
                State = download.State,
                Percentage = percentage,
                DownloadSpeed = tm?.Monitor?.DownloadSpeed ?? 0,
                UploadSpeed = tm?.Monitor?.UploadSpeed ?? 0,
                PeerInfo = new List<PeerInfo>(),
                Size = tm?.Torrent?.Size ?? 0
            };

            if (tm != null) {
                var peers = await tm.GetPeersAsync();
                foreach (var peer in peers) {
                    status.PeerInfo.Add(new PeerInfo {
                        ConnectionString = peer.Uri.ToString(),
                        DownloadSpeed = peer.Monitor.DownloadSpeed,
                        DownloadTotal = peer.Monitor.DataBytesDownloaded + peer.Monitor.ProtocolBytesDownloaded,
                        UploadSpeed = peer.Monitor.UploadSpeed,
                        UploadTotal = peer.Monitor.DataBytesUploaded + peer.Monitor.ProtocolBytesUploaded,
                        Client = peer.ClientApp.Client.ToString()
                    });
                }
            }

            return status;
        }

        private async void OnTorrentStateChanged(object sender, TorrentStateChangedEventArgs e) {
            var hash = e.TorrentManager?.Torrent?.InfoHash?.ToHex();
            if (!string.IsNullOrEmpty(hash)) {
                string newState = string.Empty;
                switch (e.NewState) {
                    case TorrentState.Downloading:
                        newState = Constants.DownloadState.Downloading;
                        break;
                    case TorrentState.Seeding:
                        newState = Constants.DownloadState.Complete;
                        break;
                    case TorrentState.Error:
                        newState = Constants.DownloadState.Error;
                        break;
                    case TorrentState.Metadata:
                    case TorrentState.Starting:
                    case TorrentState.Paused:
                    case TorrentState.Hashing:
                    case TorrentState.HashingPaused:
                    case TorrentState.Stopping:
                    case TorrentState.Stopped:
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(newState)) {
                    using (var scope = serviceScopeFactory.CreateScope()) {
                        var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();
                        var d = await downloadService.Entities.Where(d => d.Hash == hash).Select(d => new { d.State, d.Id }).FirstOrDefaultAsync();
                        if (d == null) {
                            logger.LogError("Download with hash {0} does not exist", hash);
                            return;
                        }
                        NotifyDownloadStateChanged(new DownloadStateChangedEventArgs {
                            Id = d.Id,
                            Hash = hash,
                            OldState = d.State,
                            NewState = newState
                        });
                    }
                }
            }
        }
        public void NotifyDownloadStateChanged(DownloadStateChangedEventArgs eventArgs) {
            logger.LogInformation("Torrent {0} with hash {1} has changed state, from {2} to {3}", eventArgs.Id, eventArgs.Hash, eventArgs.OldState, eventArgs.NewState);
            OnDownloadStateChanged?.Invoke(this, eventArgs);
        }

        protected virtual void Dispose(bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    if (torrents != null) {
                        foreach (var tm in torrents.Values) {
                            tm.TorrentStateChanged -= OnTorrentStateChanged;
                        }
                    }
                    Teardown(CancellationToken.None).Wait();
                    disposedValue = true;
                }
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public EventHandler<DownloadStateChangedEventArgs> OnDownloadStateChanged { get; set; }

    }

}
