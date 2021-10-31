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

            return appSettings;
        }

    }
}
