using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Core.Utility;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Framework.Instrumentation;
using Zeno.Torrent.API.Framework.Utility;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Core.Services {

    public class DownloadInteractionService : IDownloadInteractionService {

        private readonly ILogger logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IWrappedTorrent wrappedTorrent;
        private readonly INotificationAggregator notificationAggregator;

        public DownloadInteractionService(
            ILogger<DownloadInteractionService> logger,
            IServiceScopeFactory serviceScopeFactory,
            IWrappedTorrent wrappedTorrent,
            INotificationAggregator notificationAggregator
        ) {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.wrappedTorrent = wrappedTorrent;
            this.notificationAggregator = notificationAggregator;
        }

        public async Task<Download> AddMagnetDownload(Download download, User user, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();
                    var downloadDefaulter = scope.ServiceProvider.GetRequiredService<IEntityDefaulter<Download>>();

                    await downloadDefaulter.DefaultAsync(download, user);

                    var savedDownload = await downloadService.SaveEntityAsync(download, cancellationToken);

                    if (savedDownload == null) {
                        return null;
                    }
                    
                    return await ResumeDownload(savedDownload, cancellationToken) ?? savedDownload;
                }
            }
        }

        public async Task<Download> ResumeDownload(Download download, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                return await wrappedTorrent.RegisterDownload(download, cancellationToken);
            }
        }

        public async Task ResumeDownloads(int downloads, CancellationToken cancellationToken) {
            using (var scope = serviceScopeFactory.CreateScope()) {
                var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();
                var episodeService = scope.ServiceProvider.GetRequiredService<IEntityService<Episode>>();
                var downloadIteractionService = scope.ServiceProvider.GetRequiredService<IDownloadInteractionService>();

                var downloadsToResume = await downloadService.Entities
                    .Where(t =>
                        t.State != Constants.DownloadState.Processed &&
                        t.State != Constants.DownloadState.Error
                    )
                    .OrderBy(d => d.Id)
                    .Take(downloads)
                    .ToListAsync();
                downloadsToResume.ForEach(async d => {
                    try {
                        await downloadIteractionService.ResumeDownload(d, CancellationToken.None);
                    }
                    catch (Exception e) {
                        logger.LogError("Error resuming download", e);
                    }
                });
            }
        }

        internal async Task<Download> FetchAndUpdateStatus(DownloadStateChangedEventArgs e, string newState, CancellationToken cancellationToken) {
            using (var scope = serviceScopeFactory.CreateScope()) {
                var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();

                var download = await downloadService.Entities
                    .Where(d => d.Id == e.Id || d.Hash == e.Hash)
                    .FirstOrDefaultAsync(cancellationToken);

                if (download == null) { return null; }

                download.State = newState;

                return await downloadService.SaveEntityAsync(download, cancellationToken);
            }
        }

        public async Task HandleDownloadAdded(DownloadStateChangedEventArgs e, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                await FetchAndUpdateStatus(e, Constants.DownloadState.Added, cancellationToken);
            }
        }

        public async Task HandleDownloadComplete(DownloadStateChangedEventArgs e, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();
                    var showService = scope.ServiceProvider.GetRequiredService<IEntityService<Show>>();
                    var configuration = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();
                    var fileOperations = scope.ServiceProvider.GetRequiredService<IFileOperations>();

                    var download = await downloadService.Entities
                        .Where(d => d.Id == e.Id || d.Hash == e.Hash)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (download == null) {
                        return;
                    }

                    var files = wrappedTorrent.GetDownloadFiles(download);

                    string error = string.Empty;

                    if (download.DownloadType == Constants.DownloadType.Episode ||
                        download.DownloadType == Constants.DownloadType.Season) {
                        error = await HandleTvDownload(configuration.Value, showService, fileOperations, download, files, cancellationToken);
                    } else if (download.DownloadType == Constants.DownloadType.Movie) {
                        error = await HandleMovieDownload(configuration.Value, fileOperations, download, files, cancellationToken);
                    }

                    if (string.IsNullOrEmpty(error)) {
                        download.State = Constants.DownloadState.Processed;
                    } else {
                        download.State = Constants.DownloadState.Error;
                        logger.LogError(error);
                    }

                    download = await downloadService.SaveEntityAsync(download, cancellationToken);

                    await wrappedTorrent.RemoveDownload(download, cancellationToken);

                    if (string.IsNullOrEmpty(error) && configuration.Value.AUTO_COMPLETE) {
                        await downloadService.DeleteEntityAsync(download, cancellationToken);
                    }

                    await ResumeDownloads(1, CancellationToken.None);
                }
            }
        }

        internal async Task<string> HandleMovieDownload(AppSettings settings, IFileOperations fileOperations, Download download, string[] files, CancellationToken cancellationToken) {
            string error = string.Empty;
            var completedMedia = new CompletedMedia {
                DownloadType = download.DownloadType,
                Download = download
            };
            foreach (var file in files) {
                var extension = Path.GetExtension(file);

                if (!settings.SplitMediaExtensions.Contains(extension)) {
                    continue;
                }

                string target = Path.Combine(settings.TORRENT_ENGINE_MOVIE_PATH, Path.GetFileName(file));
                string source = file;

                logger.LogInformation("Copying from '{0}' to '{1}'", source, target);

                fileOperations.CopyFile(source, target);
            }

            await notificationAggregator.Notify(completedMedia, cancellationToken);

            return error;
        }

        internal async Task<string> HandleTvDownload(AppSettings settings, IEntityService<Show> showService, IFileOperations fileOperations, Download download, string[] files, CancellationToken cancellationToken) {
            var show = await showService.GetEntityAsync(download.DestinationTypeId, cancellationToken);
            if (show == null) throw new ArgumentException($"Show id ${download.DestinationTypeId} does not exist");

            return await CopyTVFiles(settings, fileOperations, download, show, files, cancellationToken);
        }

        internal async Task<string> CopyTVFiles(AppSettings settings, IFileOperations fileOperations, Download download, Show show, string[] files, CancellationToken cancellationToken) {
            var completedMedia = new CompletedMedia {
                DownloadType = download.DownloadType,
                Download = download,
                Show = show
            };
            string error = string.Empty;
            foreach (var file in files) {
                var filename = Path.GetFileNameWithoutExtension(file);
                var extension = Path.GetExtension(file);
                var location = Path.GetDirectoryName(file);

                if (!settings.SplitMediaExtensions.Contains(extension)) {
                    continue;
                }

                var info = TVFilenameOperations.ExtractInfo(filename);

                if (!info.Valid) {
                    string errorMessage = $"Processing torrent {download.Id} for {show.Name} - {filename + extension} failed.";
                    logger.LogError(errorMessage);
                    if (!string.IsNullOrEmpty(error)) {
                        error += Environment.NewLine;
                    }
                    error += errorMessage;
                    continue;
                }
                else if (file.Contains("sample", StringComparison.OrdinalIgnoreCase)) {
                    logger.LogWarning("We are copying a file that has sample in the name: {0}", file);
                }

                completedMedia.TvInfo.Add(info);

                var destinationLocation = Path.Combine(settings.TORRENT_ENGINE_TV_PATH, show.Name, $"Season {info.Season}");

                if (!fileOperations.DirectoryExists(destinationLocation)) {
                    fileOperations.CreateDirectory(destinationLocation);
                }

                var destinationName = TVFilenameOperations.CreateFileName(show, info, extension);

                string target = Path.Combine(destinationLocation, destinationName);
                string source = file;

                logger.LogInformation("Copying from '{0}' to '{1}'", source, target);

                fileOperations.CopyFile(source, target);
            }

            await notificationAggregator.Notify(completedMedia, cancellationToken);

            return error;
        }

        public async Task HandleDownloadDownloading(DownloadStateChangedEventArgs e, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                await FetchAndUpdateStatus(e, Constants.DownloadState.Downloading, cancellationToken);
            }
        }

        public async Task HandleDownloadDeleted(DownloadStateChangedEventArgs e, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                var download = await FetchAndUpdateStatus(e, Constants.DownloadState.Complete, cancellationToken);
                await wrappedTorrent.RemoveDownload(download, cancellationToken);
            }
        }

        public async Task HandleDownloadError(DownloadStateChangedEventArgs e, CancellationToken cancellationToken) {
            using (logger.ProfileOperation()) {
                await FetchAndUpdateStatus(e, Constants.DownloadState.Error, cancellationToken);
            }
        }
    }

}
