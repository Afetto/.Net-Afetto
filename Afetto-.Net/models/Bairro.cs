using Afetto_.Net.models;

namespace Afetto_.Net.Models
{
    public class Bairro
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;

        public Guid CidadeId { get; set; }
        public Cidade Cidade { get; set; } = null!;

        public ICollection<Logradouro> Logradouros { get; set; } = new List<Logradouro>();
    }
}