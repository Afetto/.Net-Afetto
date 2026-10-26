namespace Afetto_.Net.Domain.Filters
{
    public enum UsuarioOrdenacao
    {
        Nome,
        Email,
        DataNasc,
        CreatedAt
    }

    /// <summary>Critérios de consulta do repositório (independente de camada web/DTO).</summary>
    public class UsuarioFiltro
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
        public Guid? CidadeId { get; set; }
        public Guid? LogradouroId { get; set; }
        public UsuarioOrdenacao Ordenacao { get; set; } = UsuarioOrdenacao.Nome;
        public bool Descendente { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; } = 10;
    }
}
