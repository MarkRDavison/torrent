﻿using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Zeno.Torrent.API.Service.Services;
using Zeno.Torrent.API.Service.Services.Interfaces;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Data;
using Microsoft.Extensions.Logging;
using Zeno.Torrent.API.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Zeno.Torrent.API.Test.Service.Services {

    [TestClass]
    public class MatrixNotifierTests {

        private const string MatrixRoot = "https://matrix.example.com";
        private const string Username = "BOT";
        private const string Password = "BOT_PASSWORD";

        private MatrixNotifier matrixNotifier;
        private Mock<HttpMessageHandler> messageHandlerMock;
        private Mock<ILogger<MatrixNotifier>> loggerMock;
        private Mock<IOptions<AppSettings>> optionsMock;
        private Mock<IHttpClientFactory> httpClientFactoryMock;
            

        [TestInitialize]
        public void TestInitialize() {
            messageHandlerMock = new Mock<HttpMessageHandler>();
            loggerMock = new Mock<ILogger<MatrixNotifier>>();

            optionsMock = new Mock<IOptions<AppSettings>>();
            optionsMock.Setup(o => o.Value).Returns(() => new AppSettings {
                MATRIX_ROOT = MatrixRoot,
                MATRIX_ROOM_ID = "!asdhjasjhksadkjhdsa:matrix.example.com",
                MATRIX_BOT_PASSWORD = Password,
                MATRIX_BOT_USERNAME = Username
            });

            httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(hcf => hcf.CreateClient(It.IsAny<string>())).Returns(() => new HttpClient(messageHandlerMock.Object));
        }

        [TestMethod]
        public async Task LoginUserInvokesHttpRequestWithExpectedParameters() {
            var returnedUser = new MatrixUser {
                user_id = "userid"
            };

            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback(async (HttpRequestMessage m, CancellationToken c) => {
                    Assert.AreEqual($"{MatrixRoot}/_matrix/client/r0/login", m.RequestUri.OriginalString);

                    var json = JObject.Parse(await m.Content.ReadAsStringAsync());

                    Assert.AreEqual(MatrixNotifier.LoginType_Password, json["type"]);
                    Assert.AreEqual(Username, json["user"]);
                    Assert.AreEqual(Password, json["password"]);
                })
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = System.Net.HttpStatusCode.OK,
                    Content = new StringContent(JsonConvert.SerializeObject(returnedUser))
                })
                .Verifiable();

            matrixNotifier = new MatrixNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            var user = await matrixNotifier.LoginUser();

            messageHandlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            Assert.AreEqual(returnedUser.user_id, user.user_id);
            Assert.AreEqual(returnedUser.home_server, user.home_server);
            Assert.AreEqual(returnedUser.device_id, user.device_id);
            Assert.AreEqual(returnedUser.access_token, user.access_token);
        }

        [TestMethod]
        public async Task SendTextInvokesRequestWithExpectParameters() {
            var user = new MatrixUser {
                user_id = "userid",
                access_token = "ACCESS_TOKEN_OF_DOOM"
            };

            const string message = "The message we are sending";

            messageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Callback(async (HttpRequestMessage m, CancellationToken c) => {
                    Assert.AreEqual($"{MatrixRoot}/_matrix/client/r0/rooms/{optionsMock.Object.Value.MATRIX_ROOM_ID}/send/m.room.message?access_token={user.access_token}", m.RequestUri.OriginalString);

                    var json = JObject.Parse(await m.Content.ReadAsStringAsync());

                    Assert.AreEqual(MatrixNotifier.MessageType_Text, json["msgtype"]);
                    Assert.AreEqual(message, json["body"]);
                })
                .ReturnsAsync(new HttpResponseMessage {
                    StatusCode = System.Net.HttpStatusCode.OK
                })
                .Verifiable();

            matrixNotifier = new MatrixNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            var response = await matrixNotifier.SendText(user, message);

            messageHandlerMock
                .Protected()
                .Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());

            Assert.IsTrue(response);
        }

        [TestMethod]
        public async Task GenerateMessage_ReturnsAsExpectedForMovie() {
            matrixNotifier = new MatrixNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            var media = new CompletedMedia {
                DownloadType = Constants.DownloadType.Movie,
                Download = new Download {
                    Name = "Dune [2021] (1080p)"
                }
            };

            var message = await matrixNotifier.GenerateMessage(media);

            Assert.IsTrue(message.Contains("Movie", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(message.Contains("Dune", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(message.Contains("(1080p)", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(message.Contains("[2021]", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task GenerateMessage_ReturnsAsExpectedForMovieWithCompletedMediaMovieInfo() {
            matrixNotifier = new MatrixNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            var media = new CompletedMedia {
                DownloadType = Constants.DownloadType.Movie,
                Download = new Download {
                    Name = "Dune [2021] (1080p)"
                },
                MovieInfo = new MovieFilenameInfo {
                    Name = "Dune",
                    Quality = Constants.Quality.HD_1080p,
                    Year = 2021
                }
            };

            var message = await matrixNotifier.GenerateMessage(media);

            Assert.IsTrue(message.Contains("Movie", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(message.Contains("Dune", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(message.Contains("(1080p)", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(message.Contains("[2021]", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task GenerateMessage_ReturnsAsExpectedForSeason() {
            matrixNotifier = new MatrixNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            var media = new CompletedMedia {
                DownloadType = Constants.DownloadType.Season,
                Download = new Download {
                },
                Show = new Show {
                    Name = "Loki"
                },
                TvInfo = {
                    new TVFilenameInfo { Season = 1, Episode = 1 },
                    new TVFilenameInfo { Season = 1, Episode = 2 },
                    new TVFilenameInfo { Season = 1, Episode = 3 },
                    new TVFilenameInfo { Season = 1, Episode = 4 },
                    new TVFilenameInfo { Season = 1, Episode = 5 }
                }
            };

            var message = await matrixNotifier.GenerateMessage(media);

            Assert.IsTrue(message.Contains("Season", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(message.Contains(media.Show.Name, System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task GenerateMessage_ReturnsAsExpectedForEpisode() {
            matrixNotifier = new MatrixNotifier(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);

            var media = new CompletedMedia {
                DownloadType = Constants.DownloadType.Episode,
                Download = new Download {
                },
                Show = new Show {
                    Name = "Loki"
                },
                TvInfo = {
                    new TVFilenameInfo { Season = 1, Episode = 1 }
                }
            };

            var message = await matrixNotifier.GenerateMessage(media);

            Assert.IsTrue(message.Contains("Episode", System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(message.Contains(media.Show.Name, System.StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(message.Contains("S01E01", System.StringComparison.OrdinalIgnoreCase));
        }

    }

}
