using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Zeno.Torrent.API.Data;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Service.Services.Interfaces;

namespace Zeno.Torrent.API.Service.Services {


    public class MatrixNotifier : IMatrixNotifier {

        private HttpMessageHandler handler;

        public string RoomId { get; }
        public string Username { get; }
        public string Password { get; }
        public string MatrixRoot { get; }

        public const string MessageType_Text = "m.text";
        public const string LoginType_Password = "m.login.password";

        public MatrixNotifier(HttpMessageHandler handler, string roomId, string username, string password, string matrixRoot) {
            RoomId = roomId;
            Username = username;
            Password = password;
            MatrixRoot = matrixRoot?.TrimEnd(new[] { '/' }); ;
            this.handler = handler;
        }

        public async Task<MatrixUser> LoginUser() {
            var data = new {
                type = LoginType_Password,
                user = Username,
                password = Password
            };
            var uri = $"{MatrixRoot}/_matrix/client/r0/login";
            var message = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri),
                Content = data == null ? null : new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            using (var client = new HttpClient(handler)) {
                var response = await client.SendAsync(message);
                string res = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<MatrixUser>(res);
            }
        }

        public async Task<bool> SendText(MatrixUser user, string text) {
            var data = new {
                msgtype = MessageType_Text,
                body = text
            };

            var uri = $"{MatrixRoot}/_matrix/client/r0/rooms/{RoomId}/send/m.room.message?access_token={user.access_token}";
            var message = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri),
                Content = data == null ? null : new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            using (var client = new HttpClient(handler)) {
                var response = await client.SendAsync(message);
                return response.IsSuccessStatusCode;
            }
        }

        internal string GenerateMovieMessage(CompletedMedia completedMedia) {
            return $"A new movie has been added to your Plex library!\n{completedMedia.Download.Name}";
        }
        internal string GenerateEpisodeMessage(CompletedMedia completedMedia) {
            var info = completedMedia.TvInfo.FirstOrDefault();
            var episodeText = completedMedia.TvInfo.Count > 0
                ? $"({$"S{string.Format("{0:00}", info.Season)}E{string.Format("{0:00}", info.Episode)}"}) "
                : string.Empty;
            return $"A new episode {episodeText}of {completedMedia.Show.Name} has been added to your Plex library!";
        }
        internal string GenerateSeasonMessage(CompletedMedia completedMedia) {
            var info = completedMedia.TvInfo.FirstOrDefault();
            var seasonText = completedMedia.TvInfo.Count > 0
                ? $"(Season {info.Season}) "
                : string.Empty;
            return $"A new season {seasonText}of {completedMedia.Show.Name} has been added to your Plex library!";
        }

        public Task<string> GenerateMessage(CompletedMedia completedMedia) {
            switch (completedMedia.DownloadType) {
                case Constants.DownloadType.Episode:
                    return Task.FromResult(GenerateEpisodeMessage(completedMedia));
                case Constants.DownloadType.Season:
                    return Task.FromResult(GenerateSeasonMessage(completedMedia));
                case Constants.DownloadType.Movie:
                    return Task.FromResult(GenerateMovieMessage(completedMedia));
            }

            return Task.FromResult(string.Empty);
        }

        public async Task NotifyCompletedMedia(CompletedMedia media, CancellationToken cancellationToken) {
            var message = await GenerateMessage(media);
            if (string.IsNullOrEmpty(message)) {
                return;
            }
            var user = await LoginUser();
            await SendText(user, message);
        }
    }
}
