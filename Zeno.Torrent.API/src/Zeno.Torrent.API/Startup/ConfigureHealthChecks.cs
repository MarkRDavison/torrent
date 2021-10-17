using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zeno.Torrent.API.Util;

namespace Zeno.Torrent.API {

    public static class ConfigureHealthChecks {
        public static void ConfigureHealthCheckServices(this IServiceCollection services) {
            services.AddHealthChecks()
                .AddCheck<StartupHealthCheck>(StartupHealthCheck.Name)
                .AddCheck<LiveHealthCheck>(LiveHealthCheck.Name)
                .AddCheck<ReadyHealthCheck>(ReadyHealthCheck.Name);
        }

        public static void MapHealthChecks(this IEndpointRouteBuilder endpoints) {
            endpoints.MapHealthChecks("/health/startup", new HealthCheckOptions {
                Predicate = r => r.Name == StartupHealthCheck.Name
            });
            endpoints.MapHealthChecks("/health/liveness", new HealthCheckOptions {
                Predicate = r => r.Name == LiveHealthCheck.Name
            });
            endpoints.MapHealthChecks("/health/readiness", new HealthCheckOptions {
                Predicate = r => r.Name == ReadyHealthCheck.Name
            });
        }
    }

}
