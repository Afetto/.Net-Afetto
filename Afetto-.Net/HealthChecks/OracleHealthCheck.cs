using Microsoft.Extensions.Diagnostics.HealthChecks;
using Afetto_.Net.Data;
using Microsoft.EntityFrameworkCore;

namespace Afetto_.Net.HealthChecks
{
    public class OracleHealthCheck : IHealthCheck
    {
        private readonly AppDbContext _context;

        public OracleHealthCheck(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Tenta executar uma query simples no Oracle
                await _context.Database.ExecuteSqlRawAsync("SELECT 1 FROM DUAL", cancellationToken);
                return HealthCheckResult.Healthy("Oracle Database está acessível.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Oracle Database inacessível.",
                    exception: ex,
                    data: new Dictionary<string, object>
                    {
                        { "error", ex.Message }
                    });
            }
        }
    }
}