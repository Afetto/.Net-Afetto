using System.Net.Http.Json;
using System.Net;
using Afetto_.Net.Application.DTOs;
using Afetto_.Net.Tests.Integration.Fixtures;
using FluentAssertions;
using Xunit;

namespace Afetto_.Net.Tests.Integration.Controllers
{
    [Collection("Integration")]
    public class AuthIntegrationTests
    {
        private readonly AfettoWebApplicationFactory _factory;

        public AuthIntegrationTests(AfettoWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetUsuarios_SemToken_Retorna401()
        {
            // Arrange
            var client = _factory.CreateAnonymousClient();

            // Act
            var response = await client.GetAsync("/api/usuarios");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetUsuarios_ComToken_Retorna200()
        {
            // Arrange
            var client = _factory.CreateClient(); // já vem autenticado

            // Act
            var response = await client.GetAsync("/api/usuarios");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Login_UsuarioInexistente_Retorna401()
        {
            // Arrange
            var client = _factory.CreateAnonymousClient();
            var request = new LoginRequest { Email = "naoexiste@afetto.com", Senha = "Senha@123" };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_EmailInvalido_Retorna400()
        {
            // Arrange
            var client = _factory.CreateAnonymousClient();
            var request = new LoginRequest { Email = "isso-nao-e-email", Senha = "x" };

            // Act
            var response = await client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}
