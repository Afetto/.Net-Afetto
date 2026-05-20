namespace Afetto_.Net.Models
{
    public class Pais
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Sigla { get; set; } = string.Empty;

        public ICollection<Estado> Estados { get; set; } = new List<Estado>();
    }
}
