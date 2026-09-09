using Afetto_.Net.DTOs;
using Afetto_.Net.Tests.Integration.Fixtures;
using FluentAssertions;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Afetto_.Net.Tests.Integration.Controllers
{
    [Collection("Integration")]
    public class UsuarioControllerIntegrationTests : IAsyncLifetime
    {
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public UsuarioControllerIntegrationTests(PetOSWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public Task InitializeAsync() => Task.CompletedTask;
        public Task DisposeAsync() => Task.CompletedTask;

        private StringContent ToJson(object obj) =>
            new StringContent(
                JsonSerializer.Serialize(obj),
                Encoding.UTF8,
                "application/json");

        // ── GET ALL ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_SemUsuarios_Retorna200ComLista()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/usuarios");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────

        [Fact]
        public async Task GetById_IdInexistente_Retorna404()
        {
            // Arrange
            var idInexistente = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/usuarios/{idInexistente}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetById_GuidValido_NaoRetorna500()
        {
            // Arrange
            var guid = Guid.NewGuid();

            // Act
            var response = await _client.GetAsync($"/api/usuarios/{guid}");

            // Assert
            // Aceita 404 (não encontrado) mas não 500 (erro interno)
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        // ── GET BY EMAIL ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetByEmail_EmailInexistente_Retorna404()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/usuarios/email/naoexiste@email.com");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── GET BY CPF ────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByCpf_CpfInexistente_Retorna404()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/api/usuarios/cpf/00000000000");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── GET BY LOGRADOURO ─────────────────────────────────────────────────

        [Fact]
        public async Task GetByLogradouro_LogradouroSemUsuarios_Retorna404()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios/logradouro/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── GET BY CIDADE ─────────────────────────────────────────────────────

        [Fact]
        public async Task GetByCidade_CidadeSemUsuarios_Retorna404()
        {
            // Arrange & Act
            var response = await _client.GetAsync($"/api/usuarios/cidade/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── POST ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Post_RequestInvalida_Retorna400()
        {
            // Arrange
            var requestInvalida = new { Nome = "", Email = "invalido", Cpf = "123" };

            // Act
            var response = await _client.PostAsync("/api/usuarios", ToJson(requestInvalida));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_BodyVazio_Retorna400()
        {
            // Arrange
            var content = new StringContent("{}", Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/usuarios", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ── PUT ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Put_RequestInvalida_Retorna400()
        {
            // Arrange
            var id = Guid.NewGuid();
            var requestInvalida = new { Nome = "" };

            // Act
            var response = await _client.PutAsync($"/api/usuarios/{id}", ToJson(requestInvalida));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Put_IdInexistenteComDadosValidos_NaoRetorna500()
        {
            // Arrange
            var id = Guid.NewGuid();
            var request = new UpdateUsuarioRequest
            {
                Nome = "Teste",
                Email = "teste@email.com",
                DataNasc = DateTime.UtcNow,
                Telefone = "11999999999",
                Numero = "10",
                LogradouroId = Guid.NewGuid()
            };

            // Act
            var response = await _client.PutAsync($"/api/usuarios/{id}", ToJson(request));

            // Assert
            // Aceita 404 mas não 500
            response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
        }

        // ── DELETE ────────────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_IdInexistente_Retorna404()
        {
            // Arrange & Act
            var response = await _client.DeleteAsync($"/api/usuarios/{Guid.NewGuid()}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── HEALTH CHECKS ─────────────────────────────────────────────────────

        [Fact]
        public async Task HealthCheck_Live_Retorna200()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/health/live");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task HealthCheck_Geral_RetornaJsonComStatus()
        {
            // Arrange & Act
            var response = await _client.GetAsync("/health");

            // Assert — aceita qualquer status (pode ser Unhealthy por falta de Oracle)
            // mas deve retornar JSON com campo "status"
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("status");
            content.Should().Contain("checks");
        }
    }
}