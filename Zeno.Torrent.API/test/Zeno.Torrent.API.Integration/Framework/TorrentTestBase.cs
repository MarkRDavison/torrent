using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.Service.API.Auth;

namespace Zeno.Torrent.API.Integration.Framework {

    [ExcludeFromCodeCoverage]
    public abstract class TorrentTestBase : IDisposable {

        protected TorrentTestBase() {
            Factory = new TorrentWebApplicationFactory(() => ConfigureSettings);
            Client = Factory.CreateClient();
        }

        public void Dispose() {
            Client?.Dispose();
            Factory?.Dispose();
        }

        protected Task<HttpResponseMessage> PostAsync(string uri, object data, IEnumerable<Claim> claims = null) {
            return CallAsync(HttpMethod.Post, uri, data, claims);
        }

        protected Task<HttpResponseMessage> DeleteAsync(string uri, IEnumerable<Claim> claims = null) {
            return CallAsync(HttpMethod.Delete, uri, null, claims);
        }

        protected async Task<HttpResponseMessage> CallAsync(HttpMethod httpMethod, string uri, object data, IEnumerable<Claim> claims = null) {
            var message = new HttpRequestMessage {
                Method = httpMethod,
                RequestUri = new Uri(uri, UriKind.Relative),
                Headers = {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer { MockJwtTokens.GenerateJwtToken(claims ?? DefaultClaims) }" },
                    { AuthConstants.Token.Sub, Sub.ToString() }
                },
                Content = data == null ? null : new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")
            };

            return await Client.SendAsync(message);

        }

        protected async Task<T> ReadAsAsync<T>(HttpResponseMessage response) {
            string res = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(res);
        }

        protected async Task<T> PostAsAsyncWithSuccessfulResponse<T>(string uri, object data, IEnumerable<Claim> claims = null) {
            var response = await PostAsync(uri, data, claims);
            response.EnsureSuccessStatusCode();
            return await ReadAsAsync<T>(response);
        }

        protected async Task<IEnumerable<T>> GetMultipleAsync<T>(string uri, IEnumerable<Claim> claims = null) {
            var message = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri, UriKind.Relative),
                Headers = {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer { MockJwtTokens.GenerateJwtToken(claims ?? DefaultClaims) }" },
                    { AuthConstants.Token.Sub, Sub.ToString() }
                }
            };

            var response = await Client.SendAsync(message);
            response.EnsureSuccessStatusCode();
            return await ReadAsAsync<IEnumerable<T>>(response);
        }

        protected async Task<T> GetAsync<T>(string uri, IEnumerable<Claim> claims = null) {
            var message = new HttpRequestMessage {
                Method = HttpMethod.Get,
                RequestUri = new Uri(uri, UriKind.Relative),
                Headers = {
                    { HttpRequestHeader.Authorization.ToString(), $"Bearer { MockJwtTokens.GenerateJwtToken(claims ?? DefaultClaims) }" },
                    { AuthConstants.Token.Sub, Sub.ToString() }
                }
            };

            var response = await Client.SendAsync(message);
            response.EnsureSuccessStatusCode();
            return await ReadAsAsync<T>(response);
        }

        protected TorrentWebApplicationFactory Factory { get; }
        protected HttpClient Client { get; }
        protected Guid Sub { get; } = Guid.NewGuid();

        protected virtual private IEnumerable<Claim> DefaultClaims => new[] {
            new Claim(AuthConstants.Token.Sub, Sub.ToString()),
            new Claim(AuthConstants.Token.Scope, AuthConstants.API.Scope)
        };

        protected Action<AppSettings> ConfigureSettings { get; set; } = a => a.CONNECTION_STRING = $"Data Source={Guid.NewGuid()}.db";
    }
}
