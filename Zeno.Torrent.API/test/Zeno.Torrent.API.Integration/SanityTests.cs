using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zeno.Torrent.API.Integration.Framework;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Data;
using System.Linq;
using Zeno.Torrent.API.Data.DTOs;

namespace Zeno.Torrent.API.Integration {

    [TestClass]
    public class SanityTests : TorrentTestBase {
        private static Random random = new Random();
        public static string RandomString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [TestMethod]
        public async Task StartupHealthCheckIsReadyImmediately() {
            var response = await Client
                .GetAsync("/health/startup");
            response
                .EnsureSuccessStatusCode();
        }

        [TestMethod]
        public async Task ReadinessHealthCheckWorks() {
            var response = await Client
                .GetAsync("/health/readiness");
            response
                .EnsureSuccessStatusCode();
        }

        [TestMethod]
        public async Task LivenessHealthCheckWorks() {
            var response = await Client
                .GetAsync("/health/liveness");
            response
                .EnsureSuccessStatusCode();
        }

        [TestMethod]
        public async Task CreateShowDownloadEpisodeWorks() {
            var show = await PostAsAsyncWithSuccessfulResponse<Show>("/api/show", new Show { 
                Name = Guid.NewGuid().ToString(),
                Quality = Constants.Quality.HD_1080p
            });

            Assert.AreNotEqual(Guid.Empty, show.Id);

            var download = await PostAsAsyncWithSuccessfulResponse<Download>("/api/download", new Download {
                DestinationTypeId = show.Id,
                OriginalUri = RandomString(60),
                DownloadType = Constants.DownloadType.Episode
            });

            Assert.AreNotEqual(Guid.Empty, download.Id);

            var episode = await PostAsAsyncWithSuccessfulResponse<Episode>("/api/episode", new Episode { 
                ShowId = show.Id,
                SeasonNumber = 1,
                EpisodeNumber = 1,
                DownloadId = download.Id
            });

            Assert.AreNotEqual(Guid.Empty, episode.Id);
        }

        [TestMethod]
        public async Task RunCronInvokesFeedProcess() {
            var response = await PostAsync("/api/download/runcron", null);
            response.EnsureSuccessStatusCode();
        }

        [TestMethod]
        public async Task CanRetrieveCreatedShow() {
            var show = await PostAsAsyncWithSuccessfulResponse<Show>("/api/show", new Show {
                Name = Guid.NewGuid().ToString(),
                Quality = Constants.Quality.HD_1080p
            });

            Assert.AreNotEqual(Guid.Empty, show.Id);

            var shows = await GetMultipleAsync<Show>("/api/show");

            Assert.IsTrue(shows.Any());

            var fetchedShow = await GetAsync<Show>($"/api/show/{show.Id}");

            Assert.AreEqual(show.Id, fetchedShow.Id);
        }

        [TestMethod]
        public async Task CanAddMagnetDownloadEpisode() {
            var show = await PostAsAsyncWithSuccessfulResponse<Show>("/api/show", new Show {
                Name = Guid.NewGuid().ToString(),
                Quality = Constants.Quality.HD_1080p
            });

            var download = await PostAsAsyncWithSuccessfulResponse<Download>("/api/download", new Download {
                DestinationTypeId = show.Id,
                OriginalUri = RandomString(60),
                DownloadType = Constants.DownloadType.Episode
            });

            Assert.AreNotEqual(Guid.Empty, download.Id);
        }

        [TestMethod]
        public async Task CanDeleteEpisodeByShowIdAndEpisodeId() {
            var show1 = await PostAsAsyncWithSuccessfulResponse<Show>("/api/show", new Show {
                Name = Guid.NewGuid().ToString(),
                Quality = Constants.Quality.HD_1080p
            });

            var show2 = await PostAsAsyncWithSuccessfulResponse<Show>("/api/show", new Show {
                Name = Guid.NewGuid().ToString(),
                Quality = Constants.Quality.HD_1080p
            });

            Assert.AreNotEqual(Guid.Empty, show1.Id);
            Assert.AreNotEqual(Guid.Empty, show2.Id);

            var episode1 = await PostAsAsyncWithSuccessfulResponse<Episode>("/api/episode", new Episode {
                ShowId = show1.Id,
                SeasonNumber = 1,
                EpisodeNumber = 1,
                DownloadId = Guid.Empty
            });
            var episode2 = await PostAsAsyncWithSuccessfulResponse<Episode>("/api/episode", new Episode {
                ShowId = show2.Id,
                SeasonNumber = 1,
                EpisodeNumber = 2,
                DownloadId = Guid.Empty
            });

            Assert.AreNotEqual(Guid.Empty, episode1.Id);
            Assert.AreNotEqual(Guid.Empty, episode2.Id);

            var response = await DeleteAsync($"/api/show/{show1.Id}/episode/{episode1.Id}");
            response.EnsureSuccessStatusCode();

            var episodes = await GetMultipleAsync<Episode>("/api/episode");

            Assert.AreEqual(1, episodes.Count());
            Assert.AreEqual(episode2.Id, episodes.ElementAt(0).Id);
        }

        [TestMethod]
        public async Task CanDeleteAllEpisodesForShow() {

            var show = await PostAsAsyncWithSuccessfulResponse<Show>("/api/show", new Show {
                Name = Guid.NewGuid().ToString(),
                Quality = Constants.Quality.HD_1080p
            });

            Assert.AreNotEqual(Guid.Empty, show.Id);

            var episode1 = await PostAsAsyncWithSuccessfulResponse<Episode>("/api/episode", new Episode {
                ShowId = show.Id,
                SeasonNumber = 1,
                EpisodeNumber = 1,
                DownloadId = Guid.Empty
            });
            var episode2 = await PostAsAsyncWithSuccessfulResponse<Episode>("/api/episode", new Episode {
                ShowId = show.Id,
                SeasonNumber = 1,
                EpisodeNumber = 2,
                DownloadId = Guid.Empty
            });
            var episode3 = await PostAsAsyncWithSuccessfulResponse<Episode>("/api/episode", new Episode {
                ShowId = show.Id,
                SeasonNumber = 1,
                EpisodeNumber = 3,
                DownloadId = Guid.Empty
            });

            Assert.AreNotEqual(Guid.Empty, episode1.Id);
            Assert.AreNotEqual(Guid.Empty, episode2.Id);
            Assert.AreNotEqual(Guid.Empty, episode3.Id);


            var response = await DeleteAsync($"/api/show/{show.Id}/episode/all");
            response.EnsureSuccessStatusCode();

            var episodes = await GetMultipleAsync<Episode>("/api/episode");

            Assert.AreEqual(0, episodes.Count());
        }

    }

}
