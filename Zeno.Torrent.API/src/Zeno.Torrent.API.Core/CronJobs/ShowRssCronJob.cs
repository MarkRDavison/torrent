using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Core.Services;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Core.Utility;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Framework.Instrumentation;
using Zeno.Torrent.API.Framework.Utility;
using Zeno.Torrent.API.Service.Services.Interfaces;
using Zeno.Torrent.Service.API.Auth;

namespace Zeno.Torrent.API.Core.CronJobs {

    public class ShowRssCronJob : CronJobService {

        private readonly ILogger logger;
        private readonly IFileOperations fileOperations;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly ITVRssFeedReader feedReader;
        private readonly IRssFeedFetcher feedFetcher;

        public ShowRssCronJob(
            ILogger<ShowRssCronJob> logger,
            IScheduleConfig<ShowRssCronJob> config,
            IFileOperations fileOperations,
            IServiceScopeFactory serviceScopeFactory,
            ITVRssFeedReader feedReader,
            IRssFeedFetcher feedFetcher
        ) : base(config.CronExpression, config.TimeZoneInfo) {
            this.logger = logger;
            this.fileOperations = fileOperations;
            this.serviceScopeFactory = serviceScopeFactory;
            this.feedReader = feedReader;
            this.feedFetcher = feedFetcher;
        }

        internal Expression<Func<Show, bool>> FindShowByName(string name) {
            return s =>
                s.Name.Contains(name) ||
                name.Contains(s.Name) ||
                s.Name == name;
        }

        public override async Task DoWork(CancellationToken cancellationToken) {
            Guid ServiceSub = Guid.Parse("8a8678ba-00d1-49c5-a4f2-8513ed0c41aa");
            using (logger.ProfileOperation(context: "ShowRssCronJob running")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var showService = scope.ServiceProvider.GetRequiredService<IEntityService<Show>>();
                    var episodeService = scope.ServiceProvider.GetRequiredService<IEntityService<Episode>>();
                    var downloadInteractionService = scope.ServiceProvider.GetRequiredService<IDownloadInteractionService>();
                    var configuration = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();

                    var feed = await feedFetcher.FetchFeed(configuration.Value.FEED_URL);
                    var feedItems = feedReader.ParseTvSyndicationItems(feed);

                    foreach (var i in feedItems) {
                        var predicate = FindShowByName(i.ShowName);
                        var show = await showService.Entities
                            .Where(predicate)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (show == null) {
                            continue;
                        }

                        if (i.Quality != show.Quality) {
                            continue;
                        }

                        var episodeQuery = episodeService.Entities
                            .Where(e =>
                                e.ShowId == show.Id &&
                                e.SeasonNumber == i.SeasonNumber &&
                                e.EpisodeNumber == i.EpisodeNumber
                            );

                        if (i.Repack) {
                            episodeQuery = episodeQuery.Where(e => e.Repack);
                        }

                        var episode = await episodeQuery
                            .FirstOrDefaultAsync(cancellationToken);

                        if (episode != null) {
                            continue;
                        }

                        var download = new Download {
                            OriginalUri = i.Link,
                            DownloadType = Constants.DownloadType.Episode,
                            DestinationTypeId = show.Id,
                            Source = Constants.DownloadSource.Automated,
                            CreatedByUserId = ServiceSub.ToString(),
                            Hash = i.Hash
                        };

                        var seasonFolder = Path.Combine(configuration.Value.TORRENT_ENGINE_TV_PATH, show.Name, $"Season {i.SeasonNumber}");
                        bool exists = false;

                        if (fileOperations.DirectoryExists(seasonFolder)) {
                            foreach (var f in fileOperations.GetDirectoryContents(seasonFolder)) {
                                var info = TVFilenameOperations.ExtractInfo(Path.GetFileName(f));
                                if (info.Season == i.SeasonNumber && info.Episode == i.EpisodeNumber) {
                                    if (info.Repack || !i.Repack) {
                                        exists = true;
                                        break;
                                    }

                                    // TODO: Can't add repacks at the moment
                                    // TODO: Split this up, tests need to be easier
                                }
                            }
                        }

                        episode = new Episode {
                            ShowId = show.Id,
                            SeasonNumber = i.SeasonNumber,
                            EpisodeNumber = i.EpisodeNumber,
                            Repack = i.Repack,
                            DownloadId = Guid.Empty
                        };


                        if (!exists) {
                            var savedDownload = await downloadInteractionService.AddMagnetDownload(download, new User {
                                Sub = ServiceSub.ToString()
                            }, cancellationToken);
                            episode.DownloadId = savedDownload.Id;
                        }

                        await episodeService.SaveEntityAsync(episode, cancellationToken);
                    }
                }
            }
        }

    }

}
