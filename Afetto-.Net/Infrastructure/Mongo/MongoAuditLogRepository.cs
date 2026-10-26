using Afetto_.Net.Domain.Entities;
using Afetto_.Net.Domain.Filters;
using Afetto_.Net.Domain.Interfaces;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Afetto_.Net.Infrastructure.Mongo
{
    /// <summary>Repositório NoSQL (MongoDB) dos logs de auditoria.</summary>
    public class MongoAuditLogRepository : IAuditLogRepository
    {
        private readonly IMongoCollection<AuditLog> _collection;

        public MongoAuditLogRepository(IMongoClient client, IOptions<MongoSettings> options)
        {
            MongoMappings.Register();
            var settings = options.Value;
            _collection = client
                .GetDatabase(settings.DatabaseName)
                .GetCollection<AuditLog>(settings.AuditCollection);
        }

        public Task RegistrarAsync(AuditLog log, CancellationToken cancellationToken = default) =>
            _collection.InsertOneAsync(log, cancellationToken: cancellationToken);

        public async Task<(IReadOnlyList<AuditLog> Items, long Total)> ListarAsync(
            AuditLogFiltro filtro, CancellationToken cancellationToken = default)
        {
            var builder = Builders<AuditLog>.Filter;
            var filtros = new List<FilterDefinition<AuditLog>>();

            if (!string.IsNullOrWhiteSpace(filtro.EntidadeId))
                filtros.Add(builder.Eq(a => a.EntidadeId, filtro.EntidadeId));

            if (!string.IsNullOrWhiteSpace(filtro.Acao))
                filtros.Add(builder.Eq(a => a.Acao, filtro.Acao.ToLowerInvariant()));

            var filtroFinal = filtros.Count == 0 ? builder.Empty : builder.And(filtros);

            var total = await _collection.CountDocumentsAsync(filtroFinal, cancellationToken: cancellationToken);

            var itens = await _collection
                .Find(filtroFinal)
                .SortByDescending(a => a.OcorridoEm)
                .Skip(filtro.Skip)
                .Limit(filtro.Take)
                .ToListAsync(cancellationToken);

            return (itens, total);
        }
    }
}
