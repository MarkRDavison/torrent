using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Framework.Utility;
using Zeno.Torrent.API.Service.Services.Interfaces;
using Zeno.Torrent.API.Util;

namespace Zeno.Torrent.API.Core {

    public interface ITorrentEngineService : IHostedService {

    }

    public class TorrentEngineService : ITorrentEngineService {

        private readonly ILogger logger;
        private readonly IHostApplicationLifetime hostApplicationLifetime;
        private readonly IApplicationState applicationState;
        private readonly ITorrentClientEngine torrentClientEngine;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public TorrentEngineService(
            ILogger<TorrentEngineService> logger,
            IHostApplicationLifetime hostApplicationLifetime,
            IApplicationState applicationState,
            ITorrentClientEngine torrentClientEngine,
            IServiceScopeFactory serviceScopeFactory
        ) {
            this.logger = logger;
            this.hostApplicationLifetime = hostApplicationLifetime;
            this.applicationState = applicationState;
            this.torrentClientEngine = torrentClientEngine;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        /// <inheritdoc/>
        public async Task StartAsync(CancellationToken cancellationToken) {
            hostApplicationLifetime.ApplicationStarted.Register(async () => {
                applicationState.Started = true;

                using (var scope = serviceScopeFactory.CreateScope()) {
                    var downloadIteractionService = scope.ServiceProvider.GetRequiredService<IDownloadInteractionService>();
                    var configuration = scope.ServiceProvider.GetRequiredService<IOptions<AppSettings>>();

                    await downloadIteractionService.ResumeDownloads(configuration.Value.NUM_DOWNLOADS, CancellationToken.None);
                }
            });

            hostApplicationLifetime.ApplicationStopping.Register(() => {
                applicationState.Ready = false;
            });

            hostApplicationLifetime.ApplicationStopped.Register(() => {
                applicationState.Ready = false;
            });

            try {
                await Retry.Do(torrentClientEngine.Initialize, TimeSpan.FromSeconds(5), cancellationToken, 5);
                applicationState.Ready = true;
            }
            catch (Exception e) {
                logger.LogError("Failed to initialize torrent client engine", e);
                applicationState.Healthy = false;
            }
        }

        /// <inheritdoc/>
        public async Task StopAsync(CancellationToken cancellationToken) {
            await torrentClientEngine.Teardown(cancellationToken);
            if (cancellationToken.IsCancellationRequested) {
                logger.LogWarning("ApplicationStopped Ungracefully");
            }
            else {
                logger.LogInformation("ApplicationStopped Gracefully");
            }
        }

    }
}
