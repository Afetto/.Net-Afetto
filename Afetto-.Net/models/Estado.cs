using Afetto_.Net.models;


namespace Afetto_.Net.Models
{
    public class Estado
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Sigla { get; set; } = string.Empty;

        public Guid PaisId { get; set; }
        public Pais Pais { get; set; } = null!;

        public ICollection<Cidade> Cidades { get; set; } = new List<Cidade>();
    }
}