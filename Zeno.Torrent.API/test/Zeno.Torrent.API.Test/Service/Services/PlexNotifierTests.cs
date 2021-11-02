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

        private const string sectionXML =
"<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
"<MediaContainer size=\"3\" allowSync=\"0\" identifier=\"com.plexapp.plugins.library\" mediaTagPrefix=\"/system/bundle/media/flags/\" mediaTagVersion=\"1631637217\" title1=\"Plex Library\">"+
"    <Directory allowSync=\"1\" art=\"/:/resources/movie-fanart.jpg\" composite=\"/library/sections/2/composite/1635818483\" filters=\"1\" refreshing=\"0\" thumb=\"/:/resources/movie.png\" key=\"2\" type=\"movie\" title=\"Kate Sheppard Movies\" agent=\"tv.plex.agents.movie\" scanner=\"Plex Movie\" language=\"en-US\" uuid=\"d792a964-8e56-4759-a091-afcedb8988ae\" updatedAt=\"1634181456\" createdAt=\"1634181456\" scannedAt=\"1635818483\" content=\"1\" directory=\"1\" contentChangedAt=\"133714\" hidden=\"0\">"+
"        <Location id=\"2\" path=\"/media/plex/Movies\" />"+
"    </Directory>"+
"    <Directory allowSync=\"1\" art=\"/:/resources/show-fanart.jpg\" composite=\"/library/sections/3/composite/1635818515\" filters=\"1\" refreshing=\"0\" thumb=\"/:/resources/show.png\" key=\"3\" type=\"show\" title=\"Kate Sheppard TV Shows\" agent=\"tv.plex.agents.series\" scanner=\"Plex TV Series\" language=\"en-US\" uuid=\"6d93eb12-d250-42cf-80a7-751a2e360252\" updatedAt=\"1634188272\" createdAt=\"1634188272\" scannedAt=\"1635818515\" content=\"1\" directory=\"1\" contentChangedAt=\"140614\" hidden=\"0\">"+
"        <Location id=\"3\" path=\"/media/plex/TVShows\" />"+
"    </Directory>"+
"    <Directory allowSync=\"1\" art=\"/:/resources/artist-fanart.jpg\" composite=\"/library/sections/4/composite/1635818498\" filters=\"1\" refreshing=\"0\" thumb=\"/:/resources/artist.png\" key=\"4\" type=\"artist\" title=\"Kate Sheppard Music\" agent=\"tv.plex.agents.music\" scanner=\"Plex Music\" language=\"en\" uuid=\"cdde81b1-207d-49e0-9132-3defe1e51ebf\" updatedAt=\"1635637814\" createdAt=\"1635637798\" scannedAt=\"1635818498\" content=\"1\" directory=\"1\" contentChangedAt=\"130237\" hidden=\"2\">"+
"        <Location id=\"4\" path=\"/media/plex/Music\" />"+
"    </Directory>"+
"</MediaContainer>";

        private PlexNotifier plexNotifier;
        private Mock<HttpMessageHandler> messageHandlerMock;
        private Mock<ILogger<PlexNotifier>> loggerMock;
        private Mock<IOptions<AppSettings>> optionsMock;
        private Mock<IHttpClientFactory> httpClientFactoryMock;

        [TestInitialize]
        public void TestInitialize() {
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
                    Content = new StringContent(sectionXML)
                });

            plexNotifier = new PlexNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            var sections = await plexNotifier.FetchSectionIds();

            Assert.AreEqual(3, sections.Count);
            Assert.IsTrue(sections.ContainsKey(PlexSectionType.MOVIE));
            Assert.IsTrue(sections.ContainsKey(PlexSectionType.TVSHOW));
            Assert.IsTrue(sections.ContainsKey(PlexSectionType.MUSIC));
        }

        [TestMethod]
        public async Task NotifyPlexScan_MakesRequestCorrectly() {
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

    }
}
