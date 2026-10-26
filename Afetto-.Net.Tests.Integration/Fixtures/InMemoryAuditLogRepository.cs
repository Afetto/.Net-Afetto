using Afetto_.Net.Domain.Entities;
using Afetto_.Net.Domain.Filters;
using Afetto_.Net.Domain.Interfaces;

namespace Afetto_.Net.Tests.Integration.Fixtures
{
    /// <summary>Substitui o MongoDB nos testes de integração (sem depender de servidor externo).</summary>
    public class InMemoryAuditLogRepository : IAuditLogRepository
    {
        private readonly List<AuditLog> _logs = new();
        private readonly object _lock = new();

        public Task RegistrarAsync(AuditLog log, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                log.Id ??= Guid.NewGuid().ToString("N");
                _logs.Add(log);
            }
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<AuditLog> Items, long Total)> ListarAsync(
            AuditLogFiltro filtro, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                var query = _logs.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(filtro.EntidadeId))
                    query = query.Where(l => l.EntidadeId == filtro.EntidadeId);
                if (!string.IsNullOrWhiteSpace(filtro.Acao))
                    query = query.Where(l => l.Acao == filtro.Acao.ToLowerInvariant());

                var filtrados = query.OrderByDescending(l => l.OcorridoEm).ToList();
                IReadOnlyList<AuditLog> pagina = filtrados.Skip(filtro.Skip).Take(filtro.Take).ToList();
                return Task.FromResult((pagina, (long)filtrados.Count));
            }
        }
    }
}
