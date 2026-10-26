using Afetto_.Net.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Afetto_.Net.HealthChecks
{
    /// <summary>Verifica a conectividade com o MongoDB executando o comando "ping".</summary>
    public class MongoHealthCheck : IHealthCheck
    {
        private readonly IMongoClient _client;
        private readonly MongoSettings _settings;

        public MongoHealthCheck(IMongoClient client, IOptions<MongoSettings> options)
        {
            _client = client;
            _settings = options.Value;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var db = _client.GetDatabase(_settings.DatabaseName);
                await db.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cancellationToken);
                return HealthCheckResult.Healthy("MongoDB acessível.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "MongoDB indisponível.", ex);
            }
        }
    }
}
