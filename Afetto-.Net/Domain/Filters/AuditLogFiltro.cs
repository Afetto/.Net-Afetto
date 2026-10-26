namespace Afetto_.Net.Domain.Filters
{
    public class AuditLogFiltro
    {
        public string? EntidadeId { get; set; }
        public string? Acao { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; } = 10;
    }
}
