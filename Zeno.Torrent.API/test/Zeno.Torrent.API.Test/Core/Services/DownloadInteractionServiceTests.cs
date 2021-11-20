using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Core.Services;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Framework.Utility;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Test.Core.Services {


    [TestClass]
    public class DownloadInteractionServiceTests {

        private AppSettings appSettings;
        private DownloadInteractionService downloadInteractionService;

        private Mock<ILogger<DownloadInteractionService>> loggerMock;
        private Mock<IServiceScopeFactory> serviceScopeFactoryMock;
        private Mock<IWrappedTorrent> wrappedTorrentMock;
        private Mock<INotificationAggregator> notificationAggregatorMock;
        private Mock<IFileOperations> fileOperationsMock;

        [TestInitialize]
        public void TestInitialize() {
            appSettings = new AppSettings {
                TORRENT_ENGINE_MOVIE_PATH = "\\mnt\\media\\Movies",
                MEDIA_EXTENSIONS = ".mkv"
            };
            loggerMock = new Mock<ILogger<DownloadInteractionService>>();
            serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
            wrappedTorrentMock = new Mock<IWrappedTorrent>();
            notificationAggregatorMock = new Mock<INotificationAggregator>();
            fileOperationsMock = new Mock<IFileOperations>();

            downloadInteractionService = new DownloadInteractionService(
                loggerMock.Object,
                serviceScopeFactoryMock.Object,
                wrappedTorrentMock.Object,
                notificationAggregatorMock.Object
            );
        }

        [TestMethod]
        public async Task HandleMovieDownload_PutsMovieInFolderIfNameCanBeExtracted() {
            var download = new Download {
                DownloadType = Constants.DownloadType.Movie
            };

            fileOperationsMock.Setup(f => f.CopyFile(It.IsAny<string>(), DownloadInteractionService.NormalisePaths("\\mnt\\media\\Movies\\The Movie Name\\The.Movie.Name.2021.1080p.mkv"))).Verifiable();

            var error = await downloadInteractionService.HandleMovieDownload(
                appSettings,
                fileOperationsMock.Object,
                download,
                new[] { "The.Movie.Name.2021.1080p.mkv" },
                CancellationToken.None
            );

            Assert.IsTrue(string.IsNullOrEmpty(error));
            
            fileOperationsMock.Verify(f => f.CopyFile(It.IsAny<string>(), DownloadInteractionService.NormalisePaths("\\mnt\\media\\Movies\\The Movie Name\\The.Movie.Name.2021.1080p.mkv")), Times.Once);
        }

        [TestMethod]
        public async Task HandleMovieDownload_DoesntPutMovieInFolderIfNameCannotBeExtracted() {
            var download = new Download {
                DownloadType = Constants.DownloadType.Movie
            };

            fileOperationsMock.Setup(f => f.CopyFile(It.IsAny<string>(), DownloadInteractionService.NormalisePaths("\\mnt\\media\\Movies\\The.Movie.Name.mkv"))).Verifiable();

            var error = await downloadInteractionService.HandleMovieDownload(
                appSettings,
                fileOperationsMock.Object,
                download,
                new[] { "The.Movie.Name.mkv" },
                CancellationToken.None
            );

            Assert.IsTrue(string.IsNullOrEmpty(error));

            fileOperationsMock.Verify(f => f.CopyFile(It.IsAny<string>(), DownloadInteractionService.NormalisePaths("\\mnt\\media\\Movies\\The.Movie.Name.mkv")), Times.Once);
        }

        [TestMethod]
        public async Task HandleMovieDownload_NotifiesWithExtractedMovieName() {
            var download = new Download {
                DownloadType = Constants.DownloadType.Movie
            };

            bool invoked = false;

            notificationAggregatorMock.Setup(na => na.Notify(It.IsAny<CompletedMedia>(), It.IsAny<CancellationToken>())).Callback((CompletedMedia media, CancellationToken cancellationToken) => {
                invoked = true;

                Assert.AreEqual("The Movie Name", media.MovieInfo.Name);
                Assert.AreEqual(Constants.Quality.HD_1080p, media.MovieInfo.Quality);
                Assert.AreEqual(2021, media.MovieInfo.Year);
            });

            var error = await downloadInteractionService.HandleMovieDownload(
                appSettings,
                fileOperationsMock.Object,
                download,
                new[] { "The.Movie.Name.2021.1080p.mkv" },
                CancellationToken.None
            );

            Assert.IsTrue(string.IsNullOrEmpty(error));
            Assert.IsTrue(invoked);
        }

    }

}
