using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Framework.Instrumentation;
using Zeno.Torrent.API.Service.Services.Interfaces;
using Zeno.Torrent.API.Data.Models;
using System;
using System.Linq;
using Zeno.Torrent.API.Util;
using Microsoft.EntityFrameworkCore;

namespace Zeno.Torrent.API.Controllers {

    [ApiController]
    [Route("api/[controller]")]
    public class ShowController : Controller {

        private readonly ILogger logger;
        private readonly IServiceScopeFactory serviceScopeFactory;

        public ShowController(
            ILogger<ShowController> logger,
            IServiceScopeFactory serviceScopeFactory) {
            this.logger = logger;
            this.serviceScopeFactory = serviceScopeFactory;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: "GET api/show")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var showService = scope.ServiceProvider.GetRequiredService<IEntityService<Show>>();
                    return Ok(await showService.GetEntitiesAsync(cancellationToken));
                }
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: $"GET api/show/{id}")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var showService = scope.ServiceProvider.GetRequiredService<IEntityService<Show>>();
                    return Ok(await showService.GetEntityAsync(id, cancellationToken));
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Show show, CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: "POST api/show")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var showService = scope.ServiceProvider.GetRequiredService<IEntityService<Show>>();
                    var showDefaulter = scope.ServiceProvider.GetRequiredService<IEntityDefaulter<Show>>();

                    await showDefaulter.DefaultAsync(show, UserUtils.Create(User));
                    
                    try {
                        var savedShow = await showService.SaveEntityAsync(show, cancellationToken);
                        if (savedShow == null) {
                            return new BadRequestResult();
                        }
                        return Ok(savedShow);
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: $"DELETE api/show/{id}")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var showService = scope.ServiceProvider.GetRequiredService<IEntityService<Show>>();

                    var show = await showService.GetEntityAsync(id, cancellationToken);

                    if (show == null) {
                        return new NotFoundResult();
                    }

                    bool result = await showService.DeleteEntityAsync(show, cancellationToken);

                    if (!result) {
                        return new NotFoundResult();
                    }

                    return new NoContentResult();
                }
            }
        }

        [HttpDelete("{showId}/episode/{episodeId}")]
        public async Task<IActionResult> Delete(Guid showId, Guid episodeId, CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: $"POST api/show/{showId}/episode/{episodeId}")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var showService = scope.ServiceProvider.GetRequiredService<IEntityService<Show>>();
                    var episodeService = scope.ServiceProvider.GetRequiredService<IEntityService<Episode>>();

                    try {
                        var show = await showService.GetEntityAsync(showId, cancellationToken);
                        var episode = await episodeService.GetEntityAsync(episodeId, cancellationToken);
                        if (show == null || episode == null) {
                            return new NotFoundResult();
                        }

                        bool result = await episodeService.DeleteEntityAsync(episode, cancellationToken);

                        if (!result) {
                            return new NotFoundResult();
                        }

                        return new NoContentResult();
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

        [HttpDelete("{showId}/episode/all")]
        public async Task<IActionResult> DeleteEpisodes(Guid showId, CancellationToken cancellationToken) {
            using (logger.ProfileOperation(context: $"POST api/show/{showId}/episode/all")) {
                using (var scope = serviceScopeFactory.CreateScope()) {
                    var episodeService = scope.ServiceProvider.GetRequiredService<IEntityService<Episode>>();

                    try {
                        var episodes = await episodeService.Entities.Where(e => e.ShowId == showId).ToListAsync(cancellationToken);

                        foreach (var episode in episodes) {
                            await episodeService.DeleteEntityAsync(episode, cancellationToken);
                        }

                        return new NoContentResult();
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

    }

}
