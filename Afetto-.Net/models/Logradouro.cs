using Afetto_.Net.models;


namespace Afetto_.Net.Models
{
    public class Logradouro
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;

        public Guid BairroId { get; set; }
        public Bairro Bairro { get; set; } = null!;

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}