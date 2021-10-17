using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Zeno.Torrent.API.Core.Configuration;
using Zeno.Torrent.API.Core.Services.Interfaces;

namespace Zeno.Torrent.API.Integration.Framework {
    public class TorrentWebApplicationFactory : WebApplicationFactory<Startup> {

        public TorrentWebApplicationFactory(Func<Action<AppSettings>> configureSettings) {
            ConfigureSettings = configureSettings;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            builder.ConfigureTestServices(ConfigureServices);
            builder.ConfigureLogging((WebHostBuilderContext context, ILoggingBuilder loggingBuilder) =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
            });
        }

        protected virtual void ConfigureServices(IServiceCollection services) {
            services.AddSingleton<IWrappedTorrent, WrappedTorrentMock>();
            services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                var config = new OpenIdConnectConfiguration() {
                    Issuer = MockJwtTokens.Issuer
                };

                config.SigningKeys.Add(MockJwtTokens.SecurityKey);
                options.Configuration = config;
            });

            services.Configure<AppSettings>(a => { 
                if (ConfigureSettings() != null) {
                    ConfigureSettings()(a);
                }
            });
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
            }
            base.Dispose(disposing);
        }

        protected Func<Action<AppSettings>> ConfigureSettings { get; set; }
    }
}
