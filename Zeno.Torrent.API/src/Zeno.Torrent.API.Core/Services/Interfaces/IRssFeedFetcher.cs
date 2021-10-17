using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace Zeno.Torrent.API.Core.Services.Interfaces {

    public interface IRssFeedFetcher {

        Task<IList<SyndicationItem>> FetchFeed(string feed);

    }

}
