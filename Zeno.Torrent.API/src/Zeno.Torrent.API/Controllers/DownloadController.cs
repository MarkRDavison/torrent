using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Core.CronJobs;
using Zeno.Torrent.API.Core.Services;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Data.DTOs;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Framework.Instrumentation;
using Zeno.Torrent.API.Framework.Utility;
using Zeno.Torrent.API.Service.Services.Interfaces;
using Zeno.Torrent.API.Util;

namespace Zeno.Torrent.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class DownloadController : Controller {

        private readonly ILogger logger;
        private readonly IServiceScopeFactory serviceScopeFactory;
        private readonly IWrappedTorrent wrappedTorrent;

        public DownloadController(
            ILogger<DownloadController> logger,
            IServiceScopeFactory serviceScopeFactory,
            IWrappedTorrent wrappedTorrent
        ) {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
            this.wrappedTorrent = wrappedTorrent;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: "GET api/download")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();

                    return Ok(await downloadService.GetEntitiesAsync(cancellationToken));
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Download download, CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: "POST api/download")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var interactionService = scope.ServiceProvider.GetRequiredService<IDownloadInteractionService>();
                    
                    try {
                        var savedDownload = await interactionService.AddMagnetDownload(download, UserUtils.Create(User), cancellationToken);
                        if (savedDownload == null) {
                            return new BadRequestResult();
                        }
                        return Ok(savedDownload);
                    }
                    catch (AggregateException e) {
                        return new BadRequestObjectResult(new {
                            Error = e.Message,
                            Validations = e.InnerExceptions.Select(e => e.Message).ToList()
                        });
                    }
                }
            }
        }

        [HttpGet("{id}/state")]
        public async Task<IActionResult> GetState(Guid id, CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: $"GET api/download/{id}/state")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var interactionService = scope.ServiceProvider.GetRequiredService<IDownloadInteractionService>();
                    var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();
                    
                    try {
                        var download = await downloadService.GetEntityAsync(id, cancellationToken);

                        if (download == null) {
                            return new NotFoundResult();
                        }

                        return Ok(await wrappedTorrent.GetDownloadStatus(download, cancellationToken));
                    }
                    catch (Exception e) {
                        return new BadRequestObjectResult(e.Message);
                    }
                }
            }
        }

        [HttpGet("all/state")]
        public async Task<IActionResult> GetAllState(CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: "GET api/download/all/state")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var interactionService = scope.ServiceProvider.GetRequiredService<IDownloadInteractionService>();
                    var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();

                    var downloads = await downloadService.GetEntitiesAsync(cancellationToken);
                    var state = new List<DownloadStatus>();
                    foreach (var d in downloads) {
                        state.Add(await wrappedTorrent.GetDownloadStatus(d, cancellationToken));
                    }
                    return Ok(state);
                }
            }
        }

        [HttpPost("runcron")]
        public async Task<IActionResult> Post(CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: "POST api/runcron")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var cron = new ShowRssCronJob(
                        scope.ServiceProvider.GetRequiredService<ILogger<ShowRssCronJob>>(),
                        scope.ServiceProvider.GetRequiredService<IScheduleConfig<ShowRssCronJob>>(),
                        scope.ServiceProvider.GetRequiredService<IFileOperations>(),
                        serviceScopeFactory,
                        scope.ServiceProvider.GetRequiredService<ITVRssFeedReader>(),
                        scope.ServiceProvider.GetRequiredService<IRssFeedFetcher>()
                    );

                    try {
                        await cron.DoWork(cancellationToken);
                        return Ok();
                    }
                    catch (Exception e) {
                        return new BadRequestObjectResult(e.Message);
                    }
                }
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: $"DELETE api/download/{id}")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var downloadService = scope.ServiceProvider.GetRequiredService<IEntityService<Download>>();

                    var download = await downloadService.GetEntityAsync(id, cancellationToken);

                    if (download == null) {
                        return new NotFoundResult();
                    }

                    bool result = await downloadService.DeleteEntityAsync(download, cancellationToken);

                    if (!result) {
                        return new NotFoundResult();
                    }

                    return new NoContentResult();
                }
            }
        }

    }

}
