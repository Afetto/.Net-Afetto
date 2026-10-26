using System.ComponentModel.DataAnnotations;

namespace Afetto_.Net.Application.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string TipoToken { get; set; } = "Bearer";
        public DateTime ExpiraEm { get; set; }
        public Guid UsuarioId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
