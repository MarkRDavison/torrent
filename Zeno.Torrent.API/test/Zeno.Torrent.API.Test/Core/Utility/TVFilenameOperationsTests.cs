using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zeno.Torrent.API.Core.Utility;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;

namespace Zeno.Torrent.API.Test.Core.Utility {
    [TestClass]
    public class TVnameOperationsTests {

        [TestMethod]
        public void ParsingSxEyFormatWorks() {
            const string name = "Loki.S01E01.YIFY";

            int episode;
            int season;

            Assert.IsTrue(TVFilenameOperations.MatchSxEyFormat(name, out season, out episode));
            Assert.AreEqual(1, episode);
            Assert.AreEqual(1, season);
        }

        [TestMethod]
        public void ParsingSxEFormatWorks() {
            const string name = "Loki.1x01.YIFY";

            int episode;
            int season;

            Assert.IsTrue(TVFilenameOperations.MatchSxEFormat(name, out season, out episode));
            Assert.AreEqual(1, episode);
            Assert.AreEqual(1, season);
        }

        [TestMethod]
        [DataRow("Loki.S01E01.720p.YIFY", Constants.Quality.HD_720p)]
        [DataRow("Loki.S01E01.1080p.YIFY", Constants.Quality.HD_1080p)]
        [DataRow("Loki.S01E01.2k.YIFY", "")]
        public void GetQualityWorks(string name, string expectedQuality) {
            Assert.AreEqual(expectedQuality, TVFilenameOperations.GetQuality(name));
        }

        [TestMethod]
        [DataRow("Loki.S01E01.720p.YIFY", 1, 1, false, Constants.Quality.HD_720p, true)]
        [DataRow("Loki.S05E07.1080p.YIFY.repack", 5, 7, true, Constants.Quality.HD_1080p, true)]
        [DataRow("Loki.5x07.1080p.YIFY.REPACK", 5, 7, true, Constants.Quality.HD_1080p, true)]
        [DataRow("Loki.5xsa07.1asd080p.YIFY.REPACK", 5, 7, true, Constants.Quality.HD_1080p, false)]
        [DataRow("brooklyn.nine-nine.s08e02.720p.hdtv.x264-syncopy.mkv", 8, 2, false, Constants.Quality.HD_720p, true)]
        public void ExtractInfoWorks(string fullPath, int season, int episode, bool repack, string quality, bool valid) {
            var info = TVFilenameOperations.ExtractInfo(fullPath);

            Assert.AreEqual(valid, info.Valid);
            if (valid) {
                Assert.AreEqual(season, info.Season);
                Assert.AreEqual(episode, info.Episode);
                Assert.AreEqual(repack, info.Repack);
                Assert.AreEqual(quality, info.Quality);
            }
        }

        [TestMethod]
        [DataRow("The Bad Batch", 2, 5, Constants.Quality.HD_720p, false, "mp4", "The.Bad.Batch.S02E05.720p.mp4")]
        [DataRow("Rick and Morty", 11, 12, Constants.Quality.HD_1080p, true, ".mkv", "Rick.And.Morty.S11E12.1080p.REPACK.mkv")]
        public void CreateFileName_WorksAsExpected(string name, int season, int episode, string quality, bool repack, string extension, string expected) {
            Show show = new Show {
                Name = name
            };
            var info = new TVFilenameInfo {
                Season = season,
                Episode = episode,
                Quality = quality,
                Repack = repack
            };

            Assert.AreEqual(expected, TVFilenameOperations.CreateFileName(show, info, extension));
        }
    }
}
