using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Services;

namespace Zeno.Torrent.API.Test.Service.Services {

    [TestClass]
    public class DownloadDefaulterTests {

        public const string ExampleMagnet = "magnet:?xt=urn:btih:33969D34FC1A1988ABDFE061F4EC19AD70D7BE4B&dn=Jungle+Cruise+%282021%29+%5B1080p%5D+%5BWebRip%5D+%5B5.1%5D&tr=udp%3A%2F%2Ftracker.coppersurfer.tk%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.openbittorrent.com%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.opentrackr.org%3A1337&tr=udp%3A%2F%2Ftracker.leechers-paradise.org%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.dler.org%3A6969%2Fannounce&tr=udp%3A%2F%2Fopentracker.i2p.rocks%3A6969%2Fannounce&tr=udp%3A%2F%2F47.ip-51-68-199.eu%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.internetwarriors.net%3A1337%2Fannounce&tr=udp%3A%2F%2F9.rarbg.to%3A2920%2Fannounce&tr=udp%3A%2F%2Ftracker.pirateparty.gr%3A6969%2Fannounce&tr=udp%3A%2F%2Ftracker.cyberia.is%3A6969%2Fannounce";

        private DownloadDefaulter downloadDefaulter;

        public DownloadDefaulterTests() {
            downloadDefaulter = new DownloadDefaulter();
        }

        [TestMethod]
        public async Task DefaultDownloadFromMagnetLinkWorks() {

            var download = new Download {
                OriginalUri = ExampleMagnet,
                CreatedByUserId = Guid.NewGuid().ToString(),
                DownloadType = Constants.DownloadType.Movie
            };

            await downloadDefaulter.DefaultAsync(download, null);

            Assert.AreEqual(ExampleMagnet.Substring(20, 40), download.Hash);
        }

    }

}
