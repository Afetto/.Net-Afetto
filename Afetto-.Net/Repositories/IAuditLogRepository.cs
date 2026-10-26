using Afetto_.Net.Models;

namespace Afetto_.Net.Repositories
{
    public class AuditLogFiltro
    {
        public string? EntidadeId { get; set; }
        public string? Acao { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; } = 10;
    }

    public interface IAuditLogRepository
    {
        Task RegistrarAsync(AuditLog log, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<AuditLog> Items, long Total)> ListarAsync(
            AuditLogFiltro filtro, CancellationToken cancellationToken = default);
    }
}
