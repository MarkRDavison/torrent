using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Framework.Instrumentation;
using Zeno.Torrent.API.Service.Services.Interfaces;
using Zeno.Torrent.API.Util;

namespace Zeno.Torrent.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class EpisodeController : Controller
    {

        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        public EpisodeController(
            ILogger<EpisodeController> logger,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            using (logger.ProfileOperation(context: "GET api/episode"))
            {
                var episodeService = serviceProvider.GetRequiredService<IEntityService<Episode>>();

                return Ok(await episodeService.GetEntitiesAsync(cancellationToken));
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Episode episode, CancellationToken cancellationToken)
        {
            using (logger.ProfileOperation(context: "POST api/episode"))
            {
                var episodeService = serviceProvider.GetRequiredService<IEntityService<Episode>>();
                var episodeDefaulter = serviceProvider.GetRequiredService<IEntityDefaulter<Episode>>();

                await episodeDefaulter.DefaultAsync(episode, UserUtils.Create(User));

                try
                {
                    var savedEpisode = await episodeService.SaveEntityAsync(episode, cancellationToken);
                    if (savedEpisode == null)
                    {
                        return new BadRequestResult();
                    }
                    return Ok(savedEpisode);
                }
                catch (AggregateException e)
                {
                    return new BadRequestObjectResult(new
                    {
                        Error = e.Message,
                        Validations = e.InnerExceptions.Select(e => e.Message).ToList()
                    });
                }
            }
        }

        [HttpPost("reset")]
        public async Task<IActionResult> Reset(CancellationToken cancellationToken)
        {
            using (logger.ProfileOperation(context: "POST api/episode/reset"))
            {
                var episodeService = serviceProvider.GetRequiredService<IEntityService<Episode>>();
                await episodeService.DeleteAllEntitiesAsync(cancellationToken);
                return Ok();
            }
        }

    }

}