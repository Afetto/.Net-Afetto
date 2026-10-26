using System.ComponentModel.DataAnnotations;
using Afetto_.Net.Domain.Entities;

namespace Afetto_.Net.Application.DTOs
{
    public class AuditLogResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Entidade { get; set; } = string.Empty;
        public string EntidadeId { get; set; } = string.Empty;
        public string Acao { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string? Detalhes { get; set; }
        public DateTime OcorridoEm { get; set; }
        public List<LinkDto> Links { get; set; } = new();
    }

    /// <summary>Parâmetros de GET /api/auditoria.</summary>
    public class AuditoriaQueryParameters
    {
        [Range(1, int.MaxValue, ErrorMessage = "page deve ser maior ou igual a 1.")]
        public int Page { get; set; } = 1;

        [Range(1, 50, ErrorMessage = "pageSize deve estar entre 1 e 50.")]
        public int PageSize { get; set; } = 10;

        /// <summary>Filtra pelo Id do usuário afetado.</summary>
        public Guid? EntidadeId { get; set; }

        /// <summary>Filtra pela ação: criado, atualizado ou removido.</summary>
        [RegularExpression("(?i)^(criado|atualizado|removido)$",
            ErrorMessage = "acao deve ser: criado, atualizado ou removido.")]
        public string? Acao { get; set; }
    }
}
