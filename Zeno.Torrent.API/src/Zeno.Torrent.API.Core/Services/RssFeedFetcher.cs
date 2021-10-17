using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Framework.Instrumentation;

namespace Zeno.Torrent.API.Core.Services {

    public class RssFeedFetcher : IRssFeedFetcher {

        private readonly ILogger logger;

        public RssFeedFetcher(ILogger<RssFeedFetcher> logger) {
            this.logger = logger;
        }

        public async Task<IList<SyndicationItem>> FetchFeed(string feed) {
            using (logger.ProfileOperation()) {
                for (int attempt = 1; attempt < 10; ++attempt) {
                    if (attempt > 1) {
                        await Task.Delay(1000 * (attempt - 1));
                    }
                    try {
                        logger.LogInformation("Fetching feed {0}from {1}", attempt > 1 ? $"{attempt} " : string.Empty, feed);
                        using (var reader = XmlReader.Create(feed)) {
                            var syndicationFeed = SyndicationFeed.Load(reader);
                            return syndicationFeed.Items.ToList();
                        }
                    }
                    catch (Exception e) {
                        logger.LogError("Error fetching feed", e);
                    }
                }
                return new List<SyndicationItem>();
            }
        }

    }

}
