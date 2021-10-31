using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;
using Zeno.Torrent.API.Core;
using Zeno.Torrent.API.Service.Services;
using Zeno.Torrent.API.Service.Services.Interfaces;
using Zeno.Torrent.API.Util;
using Zeno.Torrent.API.Data.Models;
using Zeno.Torrent.API.Data.Services;
using Zeno.Torrent.API.Data.Services.Interfaces;
using Zeno.Torrent.API.Core.Services;
using Zeno.Torrent.API.Core.Services.Interfaces;
using Zeno.Torrent.API.Framework.Utility;
using Zeno.Torrent.API.Core.CronJobs;
using Zeno.Torrent.API.Core.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Zeno.Torrent.API {
    public class Startup {

        public IConfiguration Configuration { get; set; }

        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services) {
            var appSettings = services.ConfigureSettingsServices(Configuration);

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            services.AddControllers();
            services.AddSignalR();
            services.ConfigureHealthCheckServices();

            services
                .AddOptions<HostOptions>()
                .Configure(opt => opt.ShutdownTimeout = TimeSpan.FromSeconds(5));

            services.AddSingleton<IApplicationState, ApplicationState>();
            services.AddSingleton<ITorrentClientEngine, TorrentClientEngine>(); 
            services.AddSingleton<IFileOperations, FileOperations>();
            services.AddSingleton<IWrappedTorrent, WrappedTorrent>();
            services.AddSingleton<ITVRssFeedReader, TVRssFeedReader>();
            services.AddSingleton<IRssFeedFetcher, RssFeedFetcher>();
            services.AddSingleton<IDownloadInteractionService, DownloadInteractionService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<INotificationAggregator, NotificationAggregator>();

            services.AddTransient<ITorrentRepository, TorrentRepository>();

            services.AddTransient<IEntityService<Show>, EntityService<Show>>();
            services.AddTransient<IEntityValidator<Show>, ShowValidator>();
            services.AddTransient<IEntityDefaulter<Show>, ShowDefaulter>();

            services.AddTransient<IEntityService<Episode>, EntityService<Episode>>();
            services.AddTransient<IEntityValidator<Episode>, EpisodeValidator>();
            services.AddTransient<IEntityDefaulter<Episode>, EpisodeDefaulter>();

            services.AddTransient<IEntityService<Download>, EntityService<Download>>();
            services.AddTransient<IEntityValidator<Download>, DownloadValidator>();
            services.AddTransient<IEntityDefaulter<Download>, DownloadDefaulter>();

            services.AddScoped<IClaimsTransformation, AddRolesClaimsTransformation>();

            services.AddHostedService<TorrentEngineService>();

            services.ConfigureAuthServices(appSettings);
            
            services.ConfigureDatabaseServices();


            if (!string.IsNullOrEmpty(appSettings.FEED_URL)) {
                services.AddCronJob<ShowRssCronJob>(c => {
                    c.TimeZoneInfo = TimeZoneInfo.Local;
                    c.CronExpression = appSettings.CRON_JOB_PARAM;
                });
            }
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<AppSettings> appSettings) {
            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseForwardedHeaders();
            app.UseCors(options => {
                options.AllowCredentials();
                options.AllowAnyMethod();
                options.AllowAnyHeader();
                options.SetIsOriginAllowed(o => true);
            });

            app.Use((context, next) => {
                context.Response.Headers.Add(HeaderNames.AccessControlAllowCredentials, "true");
                return next.Invoke();
            });
            app.UseRouting();
            app.UseAuthentication();
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseMiddleware<ExtractSubHeaderMiddleware>();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => {
                endpoints
                    .MapHealthChecks();
                endpoints
                    .MapControllers()
                    .RequireAuthorization();
            });
        }
    }
}
