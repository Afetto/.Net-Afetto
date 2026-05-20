namespace Afetto_.Net.Models
{
    public class Cidade
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;

        public Guid EstadoId { get; set; }
        public Estado Estado { get; set; } = null!;

        public ICollection<Bairro> Bairros { get; set; } = new List<Bairro>();
    }
}
