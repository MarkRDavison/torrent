using System;
using Microsoft.Extensions.DependencyInjection;

namespace Zeno.Torrent.API {

    public static class ConfigureSignalR {
        public static void ConfigureAuthServices(
            this IServiceCollection services) {

            services.AddSignalR(o => {
                o.KeepAliveInterval = TimeSpan.FromSeconds(120);
            });
        }
    }

}
