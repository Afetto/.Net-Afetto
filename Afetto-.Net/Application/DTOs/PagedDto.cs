using System.ComponentModel.DataAnnotations;

namespace Afetto_.Net.Application.DTOs
{
    /// <summary>Link HATEOAS: indica uma ação ou recurso relacionado.</summary>
    public class LinkDto
    {
        public string Href { get; set; } = string.Empty;
        public string Rel { get; set; } = string.Empty;
        public string Method { get; set; } = "GET";
    }

    /// <summary>Resposta paginada com metadados e links de navegação.</summary>
    public class PagedResponse<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<LinkDto> Links { get; set; } = new();
    }

    /// <summary>Parâmetros de paginação, ordenação e filtro de GET /api/usuarios.</summary>
    public class UsuarioQueryParameters
    {
        /// <summary>Número da página (começa em 1).</summary>
        [Range(1, int.MaxValue, ErrorMessage = "page deve ser maior ou igual a 1.")]
        public int Page { get; set; } = 1;

        /// <summary>Itens por página (1 a 50).</summary>
        [Range(1, 50, ErrorMessage = "pageSize deve estar entre 1 e 50.")]
        public int PageSize { get; set; } = 10;

        /// <summary>Campo de ordenação: nome, email, dataNasc ou createdAt.</summary>
        [RegularExpression("(?i)^(nome|email|dataNasc|createdAt)$",
            ErrorMessage = "sortBy deve ser: nome, email, dataNasc ou createdAt.")]
        public string SortBy { get; set; } = "nome";

        /// <summary>Direção da ordenação: asc ou desc.</summary>
        [RegularExpression("(?i)^(asc|desc)$", ErrorMessage = "sortDir deve ser asc ou desc.")]
        public string SortDir { get; set; } = "asc";

        /// <summary>Filtra por parte do nome (sem diferenciar maiúsculas).</summary>
        public string? Nome { get; set; }

        /// <summary>Filtra por parte do e-mail (sem diferenciar maiúsculas).</summary>
        public string? Email { get; set; }

        /// <summary>Filtra pela cidade do endereço.</summary>
        public Guid? CidadeId { get; set; }

        /// <summary>Filtra pelo logradouro do endereço.</summary>
        public Guid? LogradouroId { get; set; }
    }
}
