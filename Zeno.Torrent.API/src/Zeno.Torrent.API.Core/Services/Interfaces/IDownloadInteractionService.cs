using System.Threading;
using System.Threading.Tasks;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Core.Services.Interfaces {

    public interface IDownloadInteractionService {

        Task<Download> AddMagnetDownload(Download download, User user, CancellationToken cancellationToken);

        Task<Download> ResumeDownload(Download download, CancellationToken cancellationToken);

        Task ResumeDownloads(int downloads, CancellationToken cancellationToken);

        Task HandleDownloadAdded(DownloadStateChangedEventArgs e, CancellationToken cancellationToken);
        Task HandleDownloadComplete(DownloadStateChangedEventArgs e, CancellationToken cancellationToken);
        Task HandleDownloadDownloading(DownloadStateChangedEventArgs e, CancellationToken cancellationToken);
        Task HandleDownloadDeleted(DownloadStateChangedEventArgs e, CancellationToken cancellationToken);
        Task HandleDownloadError(DownloadStateChangedEventArgs e, CancellationToken cancellationToken);

    }

}
