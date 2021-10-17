using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Core.Utility;
using Zeno.Torrent.API.Framework.Instrumentation;

namespace Zeno.Torrent.API.Core.Services {

    public class TVRssFeedReader : ITVRssFeedReader {

        private readonly ILogger logger;

        public TVRssFeedReader(ILogger<TVRssFeedReader> logger) {
            this.logger = logger;
        }

        public IList<TvFeedItem> ParseTvSyndicationItems(IList<SyndicationItem> syndicationItems) {
            return syndicationItems.Select(ParseTvSyndicationItem).ToList();
        }
        internal TvFeedItem ParseTvSyndicationItem(SyndicationItem syndicationItem) {
            var item = new TvFeedItem {
                Title = syndicationItem.Title.Text,
                Link = syndicationItem.Links.Select(l => l.Uri.ToString()).FirstOrDefault(l => !string.IsNullOrEmpty(l)),
                PublishDate = syndicationItem.PublishDate.DateTime
            };

            // TODO: Extract this magic?
            item.Hash = item.Link.Substring(20, 40);

            var info = TVFilenameOperations.ExtractInfo(item.Title);

            item.Quality = info.Quality;
            item.SeasonNumber = info.Season;
            item.EpisodeNumber = info.Episode;
            item.Repack = info.Repack;

            foreach (var ee in syndicationItem.ElementExtensions) {
                using (var reader = ee.GetReader()) {
                    var name = reader.Name;
                    var content = reader.ReadElementContentAsString();
                    switch (name) {
                        case "tv:show_id":
                            item.ShowId = int.Parse(content);
                            break;
                        case "tv:show_name":
                            item.ShowName = content;
                            break;
                        case "tv:episode_id":
                            item.EpisodeId = int.Parse(content);
                            break;
                    }
                }
            }
            return item;
        }
    }

}
