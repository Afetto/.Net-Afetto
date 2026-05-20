
namespace Afetto_.Net.Models
{
    public class Pet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string Raca { get; set; } = string.Empty;
        public string Sexo { get; set; } = string.Empty;
        public float Peso { get; set; }
        public DateTime DataNasc { get; set; }
        public string? Descricao { get; set; }
        public string QrCodeToken { get; set; } = Guid.NewGuid().ToString();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
    }
}