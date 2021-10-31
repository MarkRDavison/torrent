using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Service.Services {

    public class NotificationAggregator : INotificationAggregator {

        private readonly List<IMediaNotifier> notifiers = new List<IMediaNotifier>();

        public void RegisterNotificationSink(IMediaNotifier notifier) {
            notifiers.Add(notifier);
        }
        public async Task Notify(CompletedMedia media, CancellationToken cancellationToken) {
            await Task.WhenAll(notifiers.Select(n => n.NotifyCompletedMedia(media, cancellationToken)));
        }
    }

}
