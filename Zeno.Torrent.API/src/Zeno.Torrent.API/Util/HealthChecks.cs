using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Zeno.Torrent.API.Util {
    public class StartupHealthCheck : IHealthCheck {
        public static string Name = "StartupHealthCheck";
        private readonly IApplicationState applicationState;

        public StartupHealthCheck(IApplicationState applicationState) {
            this.applicationState = applicationState;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
            if (applicationState.Started.GetValueOrDefault()) {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
    public class ReadyHealthCheck : IHealthCheck {
        public static string Name = "ReadyHealthCheck";
        private readonly IApplicationState applicationState;

        public ReadyHealthCheck(IApplicationState applicationState) {
            this.applicationState = applicationState;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
            if (applicationState.Ready.GetValueOrDefault()) {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
    public class LiveHealthCheck : IHealthCheck {
        public static string Name = "LiveHealthCheck";
        private readonly IApplicationState applicationState;

        public LiveHealthCheck(IApplicationState applicationState) {
            this.applicationState = applicationState;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default) {
            if (applicationState.Healthy.GetValueOrDefault()) {
                return Task.FromResult(HealthCheckResult.Healthy());
            }
            return Task.FromResult(HealthCheckResult.Unhealthy());
        }
    }
}
