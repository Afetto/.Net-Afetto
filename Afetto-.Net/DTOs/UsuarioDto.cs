using System.ComponentModel.DataAnnotations;

namespace Afetto_.Net.DTOs
{
    // ── RESPONSE ─────────────────────────────────────────────────────────────
    // Retornado nas consultas GET — nunca expõe a senha
    public class UsuarioResponse
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public DateTime DataNasc { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string? Complemento { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid LogradouroId { get; set; }
        public EnderecoResponse? Endereco { get; set; }
    }

    public class EnderecoResponse
    {
        public string Logradouro { get; set; } = string.Empty;
        public string Cep { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Bairro { get; set; } = string.Empty;
        public string Cidade { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
    }

    // ── CREATE REQUEST ────────────────────────────────────────────────────────
    public class CreateUsuarioRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(150, ErrorMessage = "Nome deve ter no máximo 150 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "CPF é obrigatório.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "CPF deve ter exatamente 11 dígitos.")]
        public string Cpf { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data de nascimento é obrigatória.")]
        public DateTime DataNasc { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres.")]
        public string Senha { get; set; } = string.Empty;

        [MaxLength(15)]
        public string Telefone { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Numero { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Complemento { get; set; }

        [Required(ErrorMessage = "LogradouroId é obrigatório.")]
        public Guid LogradouroId { get; set; }
    }

    // ── UPDATE REQUEST ────────────────────────────────────────────────────────
    // CPF e senha não são alteráveis aqui (fluxos separados por segurança)
    public class UpdateUsuarioRequest
    {
        [Required(ErrorMessage = "Nome é obrigatório.")]
        [MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "Data de nascimento é obrigatória.")]
        public DateTime DataNasc { get; set; }

        [Required(ErrorMessage = "E-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(15)]
        public string Telefone { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Numero { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Complemento { get; set; }

        [Required(ErrorMessage = "LogradouroId é obrigatório.")]
        public Guid LogradouroId { get; set; }
    }
}