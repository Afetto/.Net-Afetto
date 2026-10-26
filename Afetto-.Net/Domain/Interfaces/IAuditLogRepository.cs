using Afetto_.Net.Domain.Entities;
using Afetto_.Net.Domain.Filters;

namespace Afetto_.Net.Domain.Interfaces
{
    public interface IAuditLogRepository
    {
        Task RegistrarAsync(AuditLog log, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<AuditLog> Items, long Total)> ListarAsync(
            AuditLogFiltro filtro, CancellationToken cancellationToken = default);
    }
}
