using Afetto_.Net.DTOs;
using Afetto_.Net.Models;
using Afetto_.Net.Repositories;
using Afetto_.Net.Services;
using FluentAssertions;
using Moq;

namespace Afetto_.Net.Tests.Unit.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<IUsuarioRepository> _repo = new();
        private readonly Mock<ITokenService> _tokens = new();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _sut = new AuthService(_repo.Object, _tokens.Object);
        }

        private static Usuario CriarUsuario() => new()
        {
            Id = Guid.NewGuid(),
            Nome = "Maria",
            Email = "maria@petos.com",
            Senha = BCrypt.Net.BCrypt.HashPassword("Senha@123", 4)
        };

        [Fact]
        public async Task LoginAsync_CredenciaisValidas_RetornaToken()
        {
            // Arrange
            var usuario = CriarUsuario();
            var expira = DateTime.UtcNow.AddHours(1);
            _repo.Setup(r => r.GetByEmailAsync(usuario.Email)).ReturnsAsync(usuario);
            _tokens.Setup(t => t.GerarToken(usuario.Id, usuario.Nome, usuario.Email))
                   .Returns(("jwt-token", expira));

            // Act
            var result = await _sut.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = "Senha@123" });

            // Assert
            result.Token.Should().Be("jwt-token");
            result.UsuarioId.Should().Be(usuario.Id);
            result.ExpiraEm.Should().Be(expira);
        }

        [Fact]
        public async Task LoginAsync_EmailInexistente_LancaUnauthorized()
        {
            // Arrange
            _repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((Usuario?)null);

            // Act
            var act = () => _sut.LoginAsync(new LoginRequest { Email = "x@x.com", Senha = "qualquer" });

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task LoginAsync_SenhaIncorreta_LancaUnauthorized()
        {
            // Arrange
            var usuario = CriarUsuario();
            _repo.Setup(r => r.GetByEmailAsync(usuario.Email)).ReturnsAsync(usuario);

            // Act
            var act = () => _sut.LoginAsync(new LoginRequest { Email = usuario.Email, Senha = "errada" });

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _tokens.Verify(t => t.GerarToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
