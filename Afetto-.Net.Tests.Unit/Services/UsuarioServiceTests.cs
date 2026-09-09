using Afetto_.Net.DTOs;
using Afetto_.Net.Models;
using Afetto_.Net.Repositories;
using Afetto_.Net.Services;
using FluentAssertions;
using Moq;
using System.Timers;

namespace Afetto_.Net.Tests.Unit.Services
{
    public class UsuarioServiceTests
    {
        private readonly Mock<IUsuarioRepository> _repoMock;
        private readonly IUsuarioService _service;

        public UsuarioServiceTests()
        {
            _repoMock = new Mock<IUsuarioRepository>();
            _service = new UsuarioService(_repoMock.Object);
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_UsuarioExistente_RetornaUsuarioResponse()
        {
            // Arrange
            var id = Guid.NewGuid();
            var logradouroId = Guid.NewGuid();

            var usuario = new Usuario
            {
                Id = id,
                Nome = "João Silva",
                Cpf = "12345678901",
                Email = "joao@email.com",
                Senha = "hashed",
                Telefone = "11999999999",
                Numero = "42",
                DataNasc = new DateTime(1995, 6, 15),
                LogradouroId = logradouroId,
                CreatedAt = DateTime.UtcNow,
                Logradouro = new Logradouro
                {
                    Id = logradouroId,
                    Nome = "Rua das Flores",
                    Cep = "01310100",
                    Tipo = "Rua",
                    Bairro = new Bairro
                    {
                        Nome = "Bela Vista",
                        Cidade = new Cidade
                        {
                            Nome = "São Paulo",
                            Estado = new Estado
                            {
                                Nome = "São Paulo",
                                Pais = new Pais { Nome = "Brasil" }
                            }
                        }
                    }
                }
            };

            _repoMock.Setup(r => r.GetByIdAsync(id))
                     .ReturnsAsync(usuario);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Nome.Should().Be("João Silva");
            result.Email.Should().Be("joao@email.com");
            result.Endereco.Should().NotBeNull();
            result.Endereco!.Cidade.Should().Be("São Paulo");
        }

        [Fact]
        public async Task GetByIdAsync_UsuarioNaoExistente_RetornaNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(id))
                     .ReturnsAsync((Usuario?)null);

            // Act
            var result = await _service.GetByIdAsync(id);

            // Assert
            result.Should().BeNull();
        }

        // ── GET ALL ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ComUsuariosCadastrados_RetornaLista()
        {
            // Arrange
            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    Id = Guid.NewGuid(), Nome = "João", Email = "joao@email.com",
                    Cpf = "11111111111", Senha = "hash", DataNasc = DateTime.UtcNow,
                    LogradouroId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow,
                    Logradouro = new Logradouro { Nome = "Rua A", Cep = "00000000", Tipo = "Rua",
                        Bairro = new Bairro { Nome = "B",
                            Cidade = new Cidade { Nome = "C",
                                Estado = new Estado { Nome = "D",
                                    Pais = new Pais { Nome = "Brasil" } } } } }
                },
                new Usuario
                {
                    Id = Guid.NewGuid(), Nome = "Maria", Email = "maria@email.com",
                    Cpf = "22222222222", Senha = "hash", DataNasc = DateTime.UtcNow,
                    LogradouroId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow,
                    Logradouro = new Logradouro { Nome = "Rua B", Cep = "00000001", Tipo = "Avenida",
                        Bairro = new Bairro { Nome = "B",
                            Cidade = new Cidade { Nome = "C",
                                Estado = new Estado { Nome = "D",
                                    Pais = new Pais { Nome = "Brasil" } } } } }
                }
            };

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(usuarios);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetAllAsync_SemUsuarios_RetornaListaVazia()
        {
            // Arrange
            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new List<Usuario>());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().BeEmpty();
        }

        // ── CREATE ────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_DadosValidos_CriaUsuarioComSucesso()
        {
            // Arrange
            var logradouroId = Guid.NewGuid();
            var request = new CreateUsuarioRequest
            {
                Nome = "Ana Paula",
                Cpf = "33333333333",
                Email = "ana@email.com",
                Senha = "senha123",
                Telefone = "11988888888",
                Numero = "10",
                DataNasc = new DateTime(1998, 3, 20),
                LogradouroId = logradouroId
            };

            var usuarioCriado = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = request.Nome,
                Cpf = request.Cpf,
                Email = request.Email.ToLower(),
                Senha = "hashed_senha",
                Telefone = request.Telefone,
                Numero = request.Numero,
                DataNasc = request.DataNasc,
                LogradouroId = logradouroId,
                CreatedAt = DateTime.UtcNow,
                Logradouro = new Logradouro
                {
                    Nome = "Rua X",
                    Cep = "00000000",
                    Tipo = "Rua",
                    Bairro = new Bairro
                    {
                        Nome = "B",
                        Cidade = new Cidade
                        {
                            Nome = "SP",
                            Estado = new Estado
                            {
                                Nome = "SP",
                                Pais = new Pais { Nome = "Brasil" }
                            }
                        }
                    }
                }
            };

            _repoMock.Setup(r => r.EmailExistsAsync(request.Email, null))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CpfExistsAsync(request.Cpf, null))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CreateAsync(It.IsAny<Usuario>()))
                     .ReturnsAsync(usuarioCriado);
            _repoMock.Setup(r => r.GetByIdAsync(usuarioCriado.Id))
                     .ReturnsAsync(usuarioCriado);

            // Act
            var result = await _service.CreateAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Nome.Should().Be("Ana Paula");
            result.Email.Should().Be("ana@email.com");
            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_EmailDuplicado_LancaInvalidOperationException()
        {
            // Arrange
            var request = new CreateUsuarioRequest
            {
                Nome = "Carlos",
                Cpf = "44444444444",
                Email = "duplicado@email.com",
                Senha = "senha123",
                DataNasc = DateTime.UtcNow,
                LogradouroId = Guid.NewGuid()
            };

            _repoMock.Setup(r => r.EmailExistsAsync(request.Email, null))
                     .ReturnsAsync(true);

            // Act
            var act = async () => await _service.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*já está em uso*");

            _repoMock.Verify(r => r.CreateAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task CreateAsync_CpfDuplicado_LancaInvalidOperationException()
        {
            // Arrange
            var request = new CreateUsuarioRequest
            {
                Nome = "Pedro",
                Cpf = "55555555555",
                Email = "pedro@email.com",
                Senha = "senha123",
                DataNasc = DateTime.UtcNow,
                LogradouroId = Guid.NewGuid()
            };

            _repoMock.Setup(r => r.EmailExistsAsync(request.Email, null))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.CpfExistsAsync(request.Cpf, null))
                     .ReturnsAsync(true);

            // Act
            var act = async () => await _service.CreateAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*já está cadastrado*");
        }

        // ── UPDATE ────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_UsuarioExistente_AtualizaComSucesso()
        {
            // Arrange
            var id = Guid.NewGuid();
            var logradouroId = Guid.NewGuid();

            var usuarioExistente = new Usuario
            {
                Id = id,
                Nome = "Nome Antigo",
                Email = "antigo@email.com",
                Cpf = "66666666666",
                Senha = "hash",
                DataNasc = DateTime.UtcNow,
                LogradouroId = logradouroId,
                CreatedAt = DateTime.UtcNow,
                Logradouro = new Logradouro
                {
                    Nome = "Rua A",
                    Cep = "00000000",
                    Tipo = "Rua",
                    Bairro = new Bairro
                    {
                        Nome = "B",
                        Cidade = new Cidade
                        {
                            Nome = "C",
                            Estado = new Estado
                            {
                                Nome = "D",
                                Pais = new Pais { Nome = "Brasil" }
                            }
                        }
                    }
                }
            };

            var request = new UpdateUsuarioRequest
            {
                Nome = "Nome Novo",
                Email = "novo@email.com",
                DataNasc = DateTime.UtcNow,
                Telefone = "11977777777",
                Numero = "99",
                LogradouroId = logradouroId
            };

            _repoMock.Setup(r => r.GetByIdAsync(id))
                     .ReturnsAsync(usuarioExistente);
            _repoMock.Setup(r => r.EmailExistsAsync(request.Email, id))
                     .ReturnsAsync(false);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Usuario>()))
                     .ReturnsAsync(usuarioExistente);

            // Act
            var result = await _service.UpdateAsync(id, request);

            // Assert
            result.Should().NotBeNull();
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_UsuarioNaoExistente_RetornaNull()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(id))
                     .ReturnsAsync((Usuario?)null);

            var request = new UpdateUsuarioRequest
            {
                Nome = "Teste",
                Email = "teste@email.com",
                DataNasc = DateTime.UtcNow,
                LogradouroId = Guid.NewGuid()
            };

            // Act
            var result = await _service.UpdateAsync(id, request);

            // Assert
            result.Should().BeNull();
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Never);
        }

        // ── DELETE ────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_UsuarioExistente_RetornaTrue()
        {
            // Arrange
            var id = Guid.NewGuid();
            var usuario = new Usuario
            {
                Id = id,
                Nome = "Deletar",
                Email = "del@email.com",
                Cpf = "77777777777",
                Senha = "hash",
                DataNasc = DateTime.UtcNow,
                LogradouroId = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            _repoMock.Setup(r => r.GetByIdAsync(id))
                     .ReturnsAsync(usuario);
            _repoMock.Setup(r => r.DeleteAsync(usuario))
                     .Returns(Task.CompletedTask);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Should().BeTrue();
            _repoMock.Verify(r => r.DeleteAsync(usuario), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_UsuarioNaoExistente_RetornaFalse()
        {
            // Arrange
            var id = Guid.NewGuid();

            _repoMock.Setup(r => r.GetByIdAsync(id))
                     .ReturnsAsync((Usuario?)null);

            // Act
            var result = await _service.DeleteAsync(id);

            // Assert
            result.Should().BeFalse();
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<Usuario>()), Times.Never);
        }
    }
}