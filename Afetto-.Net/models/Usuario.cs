namespace Afetto_.Net.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public DateTime DataNasc { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid LogradouroId { get; set; }
        public Logradouro Logradouro { get; set; } = null!;

        public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    }
}