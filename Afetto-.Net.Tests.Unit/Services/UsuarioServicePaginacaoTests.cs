using Afetto_.Net.Application.DTOs;
using Afetto_.Net.Application.Services;
using Afetto_.Net.Domain.Entities;
using Afetto_.Net.Domain.Filters;
using Afetto_.Net.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace Afetto_.Net.Tests.Unit.Services
{
    public class UsuarioServicePaginacaoTests
    {
        private readonly Mock<IUsuarioRepository> _repoMock = new();
        private readonly UsuarioService _service;

        public UsuarioServicePaginacaoTests()
        {
            _service = new UsuarioService(_repoMock.Object);
        }

        private static IReadOnlyList<Usuario> CriarUsuarios(int quantidade) =>
            Enumerable.Range(1, quantidade)
                .Select(i => new Usuario { Nome = $"Usuario {i}", Email = $"u{i}@teste.com" })
                .ToList();

        private UsuarioFiltro? _filtroCapturado;

        private void ConfigurarRepo(IReadOnlyList<Usuario> itens, int total)
        {
            _repoMock
                .Setup(r => r.GetPagedAsync(It.IsAny<UsuarioFiltro>()))
                .Callback<UsuarioFiltro>(f => _filtroCapturado = f)
                .ReturnsAsync((itens, total));
        }

        [Fact]
        public async Task GetPagedAsync_Pagina2Tamanho10_CalculaSkipETotalPages()
        {
            // Arrange
            ConfigurarRepo(CriarUsuarios(10), total: 25);
            var query = new UsuarioQueryParameters { Page = 2, PageSize = 10 };

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            _filtroCapturado!.Skip.Should().Be(10);
            _filtroCapturado.Take.Should().Be(10);
            result.Page.Should().Be(2);
            result.TotalItems.Should().Be(25);
            result.TotalPages.Should().Be(3);
            result.Items.Should().HaveCount(10);
        }

        [Fact]
        public async Task GetPagedAsync_SortByEmailDesc_RepassaOrdenacaoAoRepositorio()
        {
            // Arrange
            ConfigurarRepo(CriarUsuarios(1), total: 1);
            var query = new UsuarioQueryParameters { SortBy = "EMAIL", SortDir = "DESC" };

            // Act
            await _service.GetPagedAsync(query);

            // Assert
            _filtroCapturado!.Ordenacao.Should().Be(UsuarioOrdenacao.Email);
            _filtroCapturado.Descendente.Should().BeTrue();
        }

        [Fact]
        public async Task GetPagedAsync_SortByDesconhecido_UsaNomeAscendente()
        {
            // Arrange
            ConfigurarRepo(CriarUsuarios(1), total: 1);
            var query = new UsuarioQueryParameters { SortBy = "campoInexistente", SortDir = "asc" };

            // Act
            await _service.GetPagedAsync(query);

            // Assert
            _filtroCapturado!.Ordenacao.Should().Be(UsuarioOrdenacao.Nome);
            _filtroCapturado.Descendente.Should().BeFalse();
        }

        [Fact]
        public async Task GetPagedAsync_ComFiltros_RepassaFiltrosAoRepositorio()
        {
            // Arrange
            ConfigurarRepo(CriarUsuarios(0), total: 0);
            var cidadeId = Guid.NewGuid();
            var query = new UsuarioQueryParameters { Nome = "maria", Email = "@gmail", CidadeId = cidadeId };

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            _filtroCapturado!.Nome.Should().Be("maria");
            _filtroCapturado.Email.Should().Be("@gmail");
            _filtroCapturado.CidadeId.Should().Be(cidadeId);
            result.Items.Should().BeEmpty();
            result.TotalPages.Should().Be(0);
        }
    }
}
