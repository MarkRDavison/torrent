﻿using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Service.Services.Interfaces {

    public interface INotificationAggregator {

        void RegisterNotificationSink(IMediaNotifier notifier);

        Task Notify(CompletedMedia media, CancellationToken cancellationToken);

    }

}
