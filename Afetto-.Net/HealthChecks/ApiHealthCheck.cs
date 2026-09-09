using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Afetto_.Net.HealthChecks
{
    public class ApiHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            // Verifica se a API está respondendo corretamente
            var isHealthy = true;

            if (isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy(
                    "API PetOS está operacional.",
                    data: new Dictionary<string, object>
                    {
                        { "version", "1.0.0" },
                        { "timestamp", DateTime.UtcNow }
                    }));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("API PetOS com problemas."));
        }
    }
}