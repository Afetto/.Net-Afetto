using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Afetto_.Net.Models
{
    /// <summary>Documento de auditoria armazenado no MongoDB (coleção audit_logs).</summary>
    public class AuditLog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Entidade { get; set; } = "Usuario";

        /// <summary>Id da entidade afetada (Guid em texto).</summary>
        public string EntidadeId { get; set; } = string.Empty;

        /// <summary>criado | atualizado | removido</summary>
        public string Acao { get; set; } = string.Empty;

        /// <summary>E-mail de quem executou a operação ("anonimo" quando não autenticado).</summary>
        public string Usuario { get; set; } = "anonimo";

        public string? Detalhes { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime OcorridoEm { get; set; } = DateTime.UtcNow;
    }
}
