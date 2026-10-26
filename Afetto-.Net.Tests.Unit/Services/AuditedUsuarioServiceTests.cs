using Afetto_.Net.Application.DTOs;
using Afetto_.Net.Application.Interfaces;
using Afetto_.Net.Application.Services;
using Afetto_.Net.Domain.Entities;
using Afetto_.Net.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Afetto_.Net.Tests.Unit.Services
{
    public class AuditedUsuarioServiceTests
    {
        private readonly Mock<IUsuarioService> _inner = new();
        private readonly Mock<IAuditLogRepository> _audit = new();
        private readonly Mock<ICurrentUserService> _currentUser = new();
        private readonly AuditedUsuarioService _sut;

        public AuditedUsuarioServiceTests()
        {
            _currentUser.SetupGet(c => c.Identificador).Returns("anonimo");
            _sut = new AuditedUsuarioService(
                _inner.Object, _audit.Object, _currentUser.Object,
                NullLogger<AuditedUsuarioService>.Instance);
        }

        [Fact]
        public async Task CreateAsync_Sucesso_RegistraAuditoriaCriado()
        {
            // Arrange
            var criado = new UsuarioResponse { Id = Guid.NewGuid(), Email = "maria@petos.com" };
            _inner.Setup(s => s.CreateAsync(It.IsAny<CreateUsuarioRequest>())).ReturnsAsync(criado);

            // Act
            var result = await _sut.CreateAsync(new CreateUsuarioRequest());

            // Assert
            result.Should().BeSameAs(criado);
            _audit.Verify(a => a.RegistrarAsync(
                It.Is<AuditLog>(l => l.Acao == "criado"
                                     && l.EntidadeId == criado.Id.ToString()
                                     && l.Usuario == "anonimo"),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ServicoLancaExcecao_NaoRegistraAuditoria()
        {
            // Arrange
            _inner.Setup(s => s.CreateAsync(It.IsAny<CreateUsuarioRequest>()))
                  .ThrowsAsync(new InvalidOperationException("duplicado"));

            // Act
            var act = () => _sut.CreateAsync(new CreateUsuarioRequest());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            _audit.Verify(a => a.RegistrarAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_UsuarioExistente_RegistraAuditoriaAtualizado()
        {
            // Arrange
            var id = Guid.NewGuid();
            _inner.Setup(s => s.UpdateAsync(id, It.IsAny<UpdateUsuarioRequest>()))
                  .ReturnsAsync(new UsuarioResponse { Id = id, Email = "a@a.com" });

            // Act
            await _sut.UpdateAsync(id, new UpdateUsuarioRequest());

            // Assert
            _audit.Verify(a => a.RegistrarAsync(
                It.Is<AuditLog>(l => l.Acao == "atualizado" && l.EntidadeId == id.ToString()),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UsuarioNaoExistente_NaoRegistraAuditoria()
        {
            // Arrange
            _inner.Setup(s => s.UpdateAsync(It.IsAny<Guid>(), It.IsAny<UpdateUsuarioRequest>()))
                  .ReturnsAsync((UsuarioResponse?)null);

            // Act
            var result = await _sut.UpdateAsync(Guid.NewGuid(), new UpdateUsuarioRequest());

            // Assert
            result.Should().BeNull();
            _audit.Verify(a => a.RegistrarAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_Removido_RegistraAuditoriaRemovido()
        {
            // Arrange
            var id = Guid.NewGuid();
            _inner.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

            // Act
            var result = await _sut.DeleteAsync(id);

            // Assert
            result.Should().BeTrue();
            _audit.Verify(a => a.RegistrarAsync(
                It.Is<AuditLog>(l => l.Acao == "removido"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_NaoExistente_NaoRegistraAuditoria()
        {
            // Arrange
            _inner.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(false);

            // Act
            var result = await _sut.DeleteAsync(Guid.NewGuid());

            // Assert
            result.Should().BeFalse();
            _audit.Verify(a => a.RegistrarAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_MongoIndisponivel_NaoPropagaExcecao()
        {
            // Arrange
            var criado = new UsuarioResponse { Id = Guid.NewGuid(), Email = "x@x.com" };
            _inner.Setup(s => s.CreateAsync(It.IsAny<CreateUsuarioRequest>())).ReturnsAsync(criado);
            _audit.Setup(a => a.RegistrarAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new TimeoutException("mongo fora do ar"));

            // Act
            var result = await _sut.CreateAsync(new CreateUsuarioRequest());

            // Assert
            result.Should().BeSameAs(criado);
        }

        [Fact]
        public async Task GetByIdAsync_Leitura_ApenasDelegaSemAuditar()
        {
            // Arrange
            var id = Guid.NewGuid();
            _inner.Setup(s => s.GetByIdAsync(id)).ReturnsAsync(new UsuarioResponse { Id = id });

            // Act
            var result = await _sut.GetByIdAsync(id);

            // Assert
            result!.Id.Should().Be(id);
            _audit.Verify(a => a.RegistrarAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_UsuarioAutenticado_RegistraIdentificadorDoUsuarioCorrente()
        {
            // Arrange
            _currentUser.SetupGet(c => c.Identificador).Returns("maria@petos.com");
            _inner.Setup(s => s.DeleteAsync(It.IsAny<Guid>())).ReturnsAsync(true);

            // Act
            await _sut.DeleteAsync(Guid.NewGuid());

            // Assert
            _audit.Verify(a => a.RegistrarAsync(
                It.Is<AuditLog>(l => l.Usuario == "maria@petos.com"), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
