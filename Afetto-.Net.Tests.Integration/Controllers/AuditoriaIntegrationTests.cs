using Afetto_.Net.Data;
using Afetto_.Net.DTOs;
using Afetto_.Net.Models;
using Afetto_.Net.Repositories;
using Afetto_.Net.Tests.Integration.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Afetto_.Net.Tests.Integration.Controllers
{
    [Collection("Integration")]
    public class AuditoriaIntegrationTests
    {
        private readonly PetOSWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AuditoriaIntegrationTests(PetOSWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient(); // autenticado
        }

        // Cria uma hierarquia geográfica exclusiva e devolve o Id do logradouro
        private Guid CriarLogradouro()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pais = new Pais { Nome = "Brasil", Sigla = "BRA" };
            var estado = new Estado { Nome = "Parana", Sigla = "PR", PaisId = pais.Id };
            var cidade = new Cidade { Nome = "Curitiba", EstadoId = estado.Id };
            var bairro = new Bairro { Nome = "Batel", CidadeId = cidade.Id };
            var logradouro = new Logradouro { Nome = "Rua B", Cep = "80000000", Tipo = "Rua", BairroId = bairro.Id };

            db.AddRange(pais, estado, cidade, bairro, logradouro);
            db.SaveChanges();
            return logradouro.Id;
        }

        private object NovoUsuario(Guid logradouroId) => new
        {
            Nome = "Usuario Auditado",
            Cpf = Random.Shared.NextInt64(10_000_000_000, 99_999_999_999).ToString(),
            DataNasc = new DateTime(1995, 5, 5),
            Email = $"auditoria{Guid.NewGuid():N}@teste.com",
            Senha = "Senha@123",
            Telefone = "11999999999",
            Numero = "10",
            LogradouroId = logradouroId
        };

        private async Task<PagedResponse<AuditLogResponse>> ConsultarAsync(string queryString)
        {
            var pagina = await _client.GetFromJsonAsync<PagedResponse<AuditLogResponse>>($"/api/auditoria?{queryString}");
            return pagina!;
        }

        [Fact]
        public async Task GetAuditoria_SemToken_Retorna401()
        {
            // Arrange
            var anonimo = _factory.CreateAnonymousClient();

            // Act
            var response = await anonimo.GetAsync("/api/auditoria");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task PostUsuario_Sucesso_GeraLogDeAuditoriaCriado()
        {
            // Arrange
            var anonimo = _factory.CreateAnonymousClient();
            var logradouroId = CriarLogradouro();

            // Act
            var post = await anonimo.PostAsJsonAsync("/api/usuarios", NovoUsuario(logradouroId));
            var criado = await post.Content.ReadFromJsonAsync<UsuarioResponse>();
            var auditoria = await ConsultarAsync($"entidadeId={criado!.Id}&acao=criado");

            // Assert
            post.StatusCode.Should().Be(HttpStatusCode.Created);
            auditoria.TotalItems.Should().Be(1);
            var log = auditoria.Items.Single();
            log.Acao.Should().Be("criado");
            log.Usuario.Should().Be("anonimo");
            log.Links.Select(l => l.Rel).Should().Contain("usuario");
        }

        [Fact]
        public async Task DeleteUsuario_Sucesso_GeraLogRemovidoComUsuarioAutenticado()
        {
            // Arrange
            var logradouroId = CriarLogradouro();
            var post = await _factory.CreateAnonymousClient().PostAsJsonAsync("/api/usuarios", NovoUsuario(logradouroId));
            var criado = await post.Content.ReadFromJsonAsync<UsuarioResponse>();

            // Act
            var delete = await _client.DeleteAsync($"/api/usuarios/{criado!.Id}");
            var auditoria = await ConsultarAsync($"entidadeId={criado.Id}&acao=removido");

            // Assert
            delete.StatusCode.Should().Be(HttpStatusCode.NoContent);
            auditoria.TotalItems.Should().Be(1);
            auditoria.Items.Single().Usuario.Should().Be("teste@petos.com");
            auditoria.Items.Single().Links.Should().BeEmpty(); // recurso não existe mais
        }

        [Fact]
        public async Task GetAuditoria_VariosLogs_PaginaEOrdenaMaisRecentePrimeiro()
        {
            // Arrange
            var repo = _factory.Services.GetRequiredService<IAuditLogRepository>();
            var entidadeId = Guid.NewGuid().ToString();
            for (var i = 1; i <= 12; i++)
            {
                await repo.RegistrarAsync(new AuditLog
                {
                    EntidadeId = entidadeId,
                    Acao = "atualizado",
                    Detalhes = $"alteracao {i}",
                    OcorridoEm = DateTime.UtcNow.AddMinutes(i)
                });
            }

            // Act
            var pagina = await ConsultarAsync($"entidadeId={entidadeId}&page=1&pageSize=5");

            // Assert
            pagina.TotalItems.Should().Be(12);
            pagina.TotalPages.Should().Be(3);
            pagina.Items.Should().HaveCount(5);
            pagina.Items.First().Detalhes.Should().Be("alteracao 12");
            pagina.Items.Select(l => l.OcorridoEm).Should().BeInDescendingOrder();
            pagina.Links.Select(l => l.Rel).Should().Contain(new[] { "self", "first", "last", "next" });
            pagina.Links.Select(l => l.Rel).Should().NotContain("prev");
        }

        [Theory]
        [InlineData("pageSize=500")]
        [InlineData("page=0")]
        [InlineData("acao=explodiu")]
        [InlineData("entidadeId=nao-e-guid")]
        public async Task GetAuditoria_ParametrosInvalidos_Retorna400(string queryString)
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/auditoria?{queryString}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
