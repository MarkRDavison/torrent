using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace Zeno.Torrent.API.Core.Services.Interfaces {

    public class TvFeedItem {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Hash { get; set; }
        public string ShowName { get; set; }
        public int ShowId { get; set; }
        public int EpisodeId { get; set; }
        public DateTime PublishDate { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Quality { get; set; }
        public bool Repack { get; set; }
    }

    public interface ITVRssFeedReader {
        IList<TvFeedItem> ParseTvSyndicationItems(IList<SyndicationItem >syndicationItems);

    }

}
