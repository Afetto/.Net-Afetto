using Afetto_.Net.Data;
using Afetto_.Net.Models;
using Afetto_.Net.Repositories;
using Afetto_.Net.Tests.Integration.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Xunit;

namespace Afetto_.Net.Tests.Integration.Controllers
{
    /// <summary>Valida o repositório contra um MongoDB real (executa só com MONGO_TEST_CONNECTION).</summary>
    public class MongoAuditLogRepositoryTests
    {
        private static MongoAuditLogRepository CriarRepositorio(out MongoClient client, out string banco)
        {
            var conexao = Environment.GetEnvironmentVariable(MongoFactAttribute.EnvVar)!;
            banco = "petos_test_" + Guid.NewGuid().ToString("N");
            client = new MongoClient(conexao);
            var settings = Options.Create(new MongoSettings { DatabaseName = banco, AuditCollection = "audit_logs" });
            return new MongoAuditLogRepository(client, settings);
        }

        [MongoFact]
        public async Task RegistrarEListar_FiltraPorEntidadeEAcao_OrdenaMaisRecentePrimeiro()
        {
            // Arrange
            var repo = CriarRepositorio(out var client, out var banco);
            var entidadeId = Guid.NewGuid().ToString();
            try
            {
                for (var i = 1; i <= 7; i++)
                    await repo.RegistrarAsync(new AuditLog
                    {
                        EntidadeId = entidadeId,
                        Acao = i % 2 == 0 ? "atualizado" : "criado",
                        Detalhes = $"evento {i}",
                        OcorridoEm = DateTime.UtcNow.AddMinutes(i)
                    });
                await repo.RegistrarAsync(new AuditLog { EntidadeId = Guid.NewGuid().ToString(), Acao = "criado" });

                // Act
                var (todos, total) = await repo.ListarAsync(new AuditLogFiltro { EntidadeId = entidadeId, Take = 3 });
                var (criados, totalCriados) = await repo.ListarAsync(new AuditLogFiltro { EntidadeId = entidadeId, Acao = "CRIADO" });

                // Assert
                total.Should().Be(7);
                todos.Should().HaveCount(3);
                todos.First().Detalhes.Should().Be("evento 7");
                todos.First().Id.Should().NotBeNullOrEmpty();
                totalCriados.Should().Be(4);
                criados.Should().OnlyContain(l => l.Acao == "criado");
            }
            finally
            {
                await client.DropDatabaseAsync(banco);
            }
        }

        [MongoFact]
        public async Task Listar_ComSkip_RetornaPaginaSeguinte()
        {
            // Arrange
            var repo = CriarRepositorio(out var client, out var banco);
            var entidadeId = Guid.NewGuid().ToString();
            try
            {
                for (var i = 1; i <= 5; i++)
                    await repo.RegistrarAsync(new AuditLog
                    {
                        EntidadeId = entidadeId, Acao = "atualizado",
                        Detalhes = $"evento {i}", OcorridoEm = DateTime.UtcNow.AddMinutes(i)
                    });

                // Act
                var (pagina2, _) = await repo.ListarAsync(new AuditLogFiltro { EntidadeId = entidadeId, Skip = 2, Take = 2 });

                // Assert
                pagina2.Select(l => l.Detalhes).Should().Equal("evento 3", "evento 2");
            }
            finally
            {
                await client.DropDatabaseAsync(banco);
            }
        }
    }
}
