using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zeno.Torrent.API.Core.Configuration;

namespace Zeno.Torrent.API {
    public static class ConfigureSettings {

        public static AppSettings ConfigureSettingsServices(
            this IServiceCollection services,
            IConfiguration configuration
        ) {
            var configured = configuration.GetSection(AppSettings.ZENO_TORRENT_DAEMON);

            services.Configure<AppSettings>(configured);
            var appSettings = new AppSettings();
            configured.Bind(appSettings);

            System.Console.WriteLine("TORRENT_ENGINE_SAVE_PATH: {0}", appSettings.TORRENT_ENGINE_SAVE_PATH);
            System.Console.WriteLine("TORRENT_ENGINE_MOVIE_PATH: {0}", appSettings.TORRENT_ENGINE_MOVIE_PATH);
            System.Console.WriteLine("TORRENT_ENGINE_TV_PATH: {0}", appSettings.TORRENT_ENGINE_TV_PATH);
            System.Console.WriteLine("MEDIA_EXTENSIONS: {0}", appSettings.MEDIA_EXTENSIONS);
            System.Console.WriteLine("DATABASE_TYPE: {0}", appSettings.DATABASE_TYPE);
            System.Console.WriteLine("CONNECTION_STRING: {0}", appSettings.CONNECTION_STRING);
            System.Console.WriteLine("DATABASE_HOST: {0}", appSettings.DATABASE_HOST);
            System.Console.WriteLine("DATABASE_PORT: {0}", appSettings.DATABASE_PORT);
            System.Console.WriteLine("DATABASE_NAME: {0}", appSettings.DATABASE_NAME);
            System.Console.WriteLine("DATABASE_USER: {0}", appSettings.DATABASE_USER);
            System.Console.WriteLine("DATABASE_PASSWORD: {0}", appSettings.DATABASE_PASSWORD);
            System.Console.WriteLine("URL: {0}", appSettings.URL);
            System.Console.WriteLine("FEED_URL: {0}", appSettings.FEED_URL);
            System.Console.WriteLine("CRON_JOB_PARAM: {0}", appSettings.CRON_JOB_PARAM);
            System.Console.WriteLine("SMTP_ADDRESS: {0}", appSettings.SMTP_ADDRESS);
            System.Console.WriteLine("SMTP_PORT: {0}", appSettings.SMTP_PORT);
            System.Console.WriteLine("SMTP_USERNAME: {0}", appSettings.SMTP_USERNAME);
            System.Console.WriteLine("SMTP_SECRET: {0}", appSettings.SMTP_SECRET);
            System.Console.WriteLine("AUTO_COMPLETE: {0}", appSettings.AUTO_COMPLETE);
            System.Console.WriteLine("AUTHORITY: {0}", appSettings.AUTHORITY);
            System.Console.WriteLine("NUM_DOWNLOADS: {0}", appSettings.NUM_DOWNLOADS);
            System.Console.WriteLine("BFF_ORIGIN: {0}", appSettings.BFF_ORIGIN);

            return appSettings;
        }

    }
}
