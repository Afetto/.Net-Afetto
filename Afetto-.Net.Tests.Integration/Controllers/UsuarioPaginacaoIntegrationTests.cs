using System.Net.Http.Json;
using System.Net;
using Afetto_.Net.Application.DTOs;
using Afetto_.Net.Domain.Entities;
using Afetto_.Net.Infrastructure.Persistence;
using Afetto_.Net.Tests.Integration.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Afetto_.Net.Tests.Integration.Controllers
{
    [Collection("Integration")]
    public class UsuarioPaginacaoIntegrationTests
    {
        private const string Prefixo = "PagTeste";
        private const int TotalSeed = 25;
        private static readonly object Lock = new();
        private static bool _seeded;

        private readonly HttpClient _client;
        private readonly AfettoWebApplicationFactory _factory;

        public UsuarioPaginacaoIntegrationTests(AfettoWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
            SeedUsuarios();
        }

        // Cria hierarquia geográfica + 25 usuários (uma única vez para a collection)
        private void SeedUsuarios()
        {
            lock (Lock)
            {
                if (_seeded) return;

                using var scope = _factory.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var pais = new Pais { Nome = "Brasil", Sigla = "BRA" };
                var estado = new Estado { Nome = "Sao Paulo", Sigla = "SP", PaisId = pais.Id };
                var cidade = new Cidade { Nome = "Sao Paulo", EstadoId = estado.Id };
                var bairro = new Bairro { Nome = "Centro", CidadeId = cidade.Id };
                var logradouro = new Logradouro
                {
                    Nome = "Rua A", Cep = "01000000", Tipo = "Rua", BairroId = bairro.Id
                };

                db.AddRange(pais, estado, cidade, bairro, logradouro);

                for (var i = 1; i <= TotalSeed; i++)
                {
                    db.Usuarios.Add(new Usuario
                    {
                        Nome = $"{Prefixo} {i:00}",
                        Email = $"pag{i:00}@teste.com",
                        Cpf = $"9000000{i:0000}",
                        Senha = "hash",
                        DataNasc = new DateTime(1990, 1, 1).AddDays(i),
                        Telefone = "11999999999",
                        Numero = "1",
                        LogradouroId = logradouro.Id
                    });
                }

                db.SaveChanges();
                _seeded = true;
            }
        }

        [Fact]
        public async Task GetAll_Pagina2Tamanho10_RetornaMetadadosEItens()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios?nome={Prefixo}&page=2&pageSize=10");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            pagina!.Page.Should().Be(2);
            pagina.PageSize.Should().Be(10);
            pagina.TotalItems.Should().Be(TotalSeed);
            pagina.TotalPages.Should().Be(3);
            pagina.Items.Should().HaveCount(10);
        }

        [Fact]
        public async Task GetAll_UltimaPagina_RetornaItensRestantes()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios?nome={Prefixo}&page=3&pageSize=10");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            pagina!.Items.Should().HaveCount(5);
        }

        [Fact]
        public async Task GetAll_OrdenacaoPorNomeDesc_PrimeiroItemEOUltimo()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios?nome={Prefixo}&sortBy=nome&sortDir=desc&pageSize=5");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            pagina!.Items.First().Nome.Should().Be($"{Prefixo} {TotalSeed}");
            pagina.Items.Select(u => u.Nome).Should().BeInDescendingOrder();
        }

        [Fact]
        public async Task GetAll_OrdenacaoPorNomeAsc_PrimeiroItemEOPrimeiro()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios?nome={Prefixo}&sortBy=nome&sortDir=asc&pageSize=5");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            pagina!.Items.First().Nome.Should().Be($"{Prefixo} 01");
        }

        [Fact]
        public async Task GetAll_FiltroPorEmail_RetornaApenasCorrespondente()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/usuarios?email=pag07@");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            pagina!.TotalItems.Should().Be(1);
            pagina.Items.Single().Email.Should().Be("pag07@teste.com");
        }

        [Fact]
        public async Task GetAll_FiltroSemResultado_RetornaListaVazia()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/usuarios?nome=NaoExisteNinguemAssim");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            pagina!.Items.Should().BeEmpty();
            pagina.TotalItems.Should().Be(0);
        }

        [Fact]
        public async Task GetAll_PrimeiraPagina_RetornaLinksSelfFirstLastNextSemPrev()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios?nome={Prefixo}&page=1&pageSize=10");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            var rels = pagina!.Links.Select(l => l.Rel).ToList();
            rels.Should().Contain(new[] { "self", "first", "last", "next" });
            rels.Should().NotContain("prev");
            pagina.Links.Single(l => l.Rel == "next").Href.Should().Contain("page=2");
            pagina.Links.Single(l => l.Rel == "last").Href.Should().Contain("page=3");
        }

        [Fact]
        public async Task GetAll_PaginaDoMeio_RetornaPrevENext()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios?nome={Prefixo}&page=2&pageSize=10");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            var rels = pagina!.Links.Select(l => l.Rel).ToList();
            rels.Should().Contain(new[] { "prev", "next" });
        }

        [Fact]
        public async Task GetAll_ItensPossuemLinksSelfUpdateDelete()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios?nome={Prefixo}&pageSize=3");
            var pagina = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

            // Assert
            foreach (var usuario in pagina!.Items)
            {
                usuario.Links.Select(l => l.Rel).Should().Contain(new[] { "self", "update", "delete" });
                usuario.Links.Single(l => l.Rel == "self").Href.Should().Contain(usuario.Id.ToString());
            }
        }

        [Fact]
        public async Task GetById_UsuarioExistente_RetornaLinks()
        {
            // Arrange
            var lista = await _client.GetFromJsonAsync<PagedResponse<UsuarioResponse>>(
                $"/api/usuarios?nome={Prefixo}&pageSize=1");
            var id = lista!.Items.First().Id;

            // Act
            var response = await _client.GetAsync($"/api/usuarios/{id}");
            var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            usuario!.Links.Should().HaveCount(3);
        }

        [Theory]
        [InlineData("pageSize=500")]
        [InlineData("page=0")]
        [InlineData("sortBy=senha")]
        [InlineData("sortDir=sideways")]
        public async Task GetAll_ParametrosInvalidos_Retorna400(string queryString)
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios?{queryString}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
