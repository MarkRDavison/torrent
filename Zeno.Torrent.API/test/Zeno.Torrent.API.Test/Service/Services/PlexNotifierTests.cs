using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Services;

namespace Zeno.Torrent.API.Test.Service.Services {

    [TestClass]
    public class PlexNotifierTests {
        private string SectionXML;
        private PlexNotifier plexNotifier;
        private Mock<HttpMessageHandler> messageHandlerMock;
        private Mock<ILogger<PlexNotifier>> loggerMock;
        private Mock<IOptions<AppSettings>> optionsMock;
        private Mock<IHttpClientFactory> httpClientFactoryMock;

        [TestInitialize]
        public void TestInitialize() {
            SectionXML = File.ReadAllText("LibraryInfo.xml");
            messageHandlerMock = new Mock<HttpMessageHandler>();
            loggerMock = new Mock<ILogger<PlexNotifier>>();

            optionsMock = new Mock<IOptions<AppSettings>>();
            optionsMock.Setup(o => o.Value).Returns(() => new AppSettings {
                PLEX_TOKEN = "asdjkdsafjhsdha",
                PLEX_URL = "https://plex.example.com"
            });

            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(hcf => hcf.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(messageHandlerMock.Object));
        }

        [TestMethod]
        public async Task FetchSectionIds_ParsesSectionsCorrectly() {

            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback((HttpRequestMessage m, CancellationToken c) => {
                    Assert.AreEqual($"{optionsMock.Object.Value.PLEX_URL}/library/sections?X-Plex-Token={optionsMock.Object.Value.PLEX_TOKEN}", m.RequestUri.OriginalString);
                })
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(SectionXML)
                });

            plexNotifier = new PlexNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            var sections = await plexNotifier.FetchSectionIds();

            Assert.AreEqual(3, sections.Count);
            Assert.IsTrue(sections.ContainsKey(PlexSectionType.MOVIE));
            Assert.IsTrue(sections.ContainsKey(PlexSectionType.TVSHOW));
            Assert.IsTrue(sections.ContainsKey(PlexSectionType.MUSIC));
        }

        [TestMethod]
        public async Task NotifyPlexScan_MakesRequestCorrectly_ForDownloadNameOnly() {
            var section = new PlexSection {
                Key = 2,
                Path = "/media/Plex/Movies"
            };

            var media = new CompletedMedia {
                DownloadType = Constants.DownloadType.Movie
            };

            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback((HttpRequestMessage m, CancellationToken c) => {
                    Assert.AreEqual(
                        $"{optionsMock.Object.Value.PLEX_URL}/library/sections/{section.Key}/refresh?X-Plex-Token={optionsMock.Object.Value.PLEX_TOKEN}&path={HttpUtility.UrlEncode(section.Path)}",
                        m.RequestUri.OriginalString);
                })
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            plexNotifier = new PlexNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            await plexNotifier.NotifyPlexScan(media, section, section.Path, CancellationToken.None);
        }

        [TestMethod]
        public async Task NotifyPlexScan_MakesRequestCorrectly_WithMovieInfo() {
            var section = new PlexSection {
                Key = 2,
                Path = "/media/Plex/Movies"
            };

            var media = new CompletedMedia {
                DownloadType = Constants.DownloadType.Movie,
                MovieInfo = new MovieFilenameInfo {
                    Name = "Movie Name Which Becomes The Folder Name",
                    Year = 2021,
                    Quality = Constants.Quality.HD_1080p
                }
            };

            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback((HttpRequestMessage m, CancellationToken c) => {
                    Assert.AreEqual(
                        $"{optionsMock.Object.Value.PLEX_URL}/library/sections/{section.Key}/refresh?X-Plex-Token={optionsMock.Object.Value.PLEX_TOKEN}&path={HttpUtility.UrlEncode(section.Path + "/" + media.MovieInfo.Name)}",
                        m.RequestUri.OriginalString);
                })
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = System.Net.HttpStatusCode.OK
                });

            plexNotifier = new PlexNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            await plexNotifier.NotifyPlexScan(media, section, section.Path, CancellationToken.None);
        }

    }
}
