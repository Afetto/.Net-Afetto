using Afetto_.Net.Application.DTOs;
using Afetto_.Net.Application.Interfaces;
using Afetto_.Net.Domain.Interfaces;

namespace Afetto_.Net.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUsuarioRepository _repository;
        private readonly ITokenService _tokenService;

        public AuthService(IUsuarioRepository repository, ITokenService tokenService)
        {
            _repository = repository;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var usuario = await _repository.GetByEmailAsync(request.Email);

            // Mesma mensagem para e-mail inexistente e senha errada (não revela qual falhou)
            if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.Senha))
                throw new UnauthorizedAccessException("E-mail ou senha inválidos.");

            var (token, expiraEm) = _tokenService.GerarToken(usuario.Id, usuario.Nome, usuario.Email);

            return new LoginResponse
            {
                Token = token,
                ExpiraEm = expiraEm,
                UsuarioId = usuario.Id,
                Nome = usuario.Nome,
                Email = usuario.Email
            };
        }
    }
}
