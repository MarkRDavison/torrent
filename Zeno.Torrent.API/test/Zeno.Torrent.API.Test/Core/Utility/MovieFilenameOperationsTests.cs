using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zeno.Torrent.API.Core.Utility;
using Zeno.Torrent.API.Data;

namespace Zeno.Torrent.API.Test.Core.Utility {

    [TestClass]
    public class MovieFilenameOperationsTests {

        [DataRow("Last.Night.In.Soho.2021.1080p.WEBRip.x264.AAC5.1-[YTS.MX].mp4", "Last Night In Soho", 2021, Constants.Quality.HD_1080p)]
        [DataRow("Red.Notice.2021.1080p.WEBRip.x264.AAC5.1-[YTS.MX].mp4", "Red Notice", 2021, Constants.Quality.HD_1080p)]
        [DataRow("17.Thor - Ragnarok (2017) 1080p 10bit Bluray x265 HEVC [Org BD 5.1 Hindi + DD 5.1 English] MSubs ~ TombDoc.mkv", "Thor Ragnarok", 2017, Constants.Quality.HD_1080p)]
        [DataRow("The.Martian.2015.EXTENDED.1080p.BRRip.x264.AAC-ETRG.mp4", "The Martian", 2015, Constants.Quality.HD_1080p)]
        [DataRow("02 The Chronicles Of Riddick - Directors Cut 2004 Eng Subs 1080p [H264-mp4].mp4", "The Chronicles Of Riddick Directors Cut", 2004, Constants.Quality.HD_1080p)]
        [DataRow("City Island [2009]DVDRip[Xvid]AC3 5.1[Eng]BlueLady.avi", "City Island", 2009, "")]
        [DataRow("Lord of War 2005 REMASTERED 1080p BluRay HEVC H265 5.1 BONE.mp4", "Lord of War", 2005, Constants.Quality.HD_1080p)]
        [DataRow("Pig.2021.1080p.WEBRip.x264.AAC5.1-[YTS.MX].mp4", "Pig", 2021, Constants.Quality.HD_1080p)]
        [DataRow("Blade.Runner.2049.2017.1080p.WEBRip.6CH.AAC.x264-EiE.mkv", "Blade Runner 2049", 2017, Constants.Quality.HD_1080p)]
        [TestMethod]
        public void MovieFilenameInfoIsRetrievedCorrectly(string filename, string name, int year, string quality) {
            var info = MovieFilenameOperations.ExtractInfo(filename);

            Assert.AreEqual(name, info.Name);
            Assert.AreEqual(year, info.Year);
            Assert.AreEqual(quality, info.Quality);
        }

    }

}
