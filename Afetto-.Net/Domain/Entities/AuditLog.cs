namespace Afetto_.Net.Domain.Entities
{
    /// <summary>Registro de auditoria (persistido em NoSQL pela camada de Infrastructure).</summary>
    public class AuditLog
    {
        public string? Id { get; set; }

        public string Entidade { get; set; } = "Usuario";

        /// <summary>Id da entidade afetada (Guid em texto).</summary>
        public string EntidadeId { get; set; } = string.Empty;

        /// <summary>criado | atualizado | removido</summary>
        public string Acao { get; set; } = string.Empty;

        /// <summary>E-mail de quem executou a operação ("anonimo" quando não autenticado).</summary>
        public string Usuario { get; set; } = "anonimo";

        public string? Detalhes { get; set; }

        public DateTime OcorridoEm { get; set; } = DateTime.UtcNow;
    }
}
