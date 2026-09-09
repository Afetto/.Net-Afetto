using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Afetto_.Net.Data;

namespace Afetto_.Net.Tests.Integration.Fixtures
{
    public class PetOSWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove TODOS os serviços relacionados ao EF Core e banco
                var toRemove = services
                    .Where(d =>
                        d.ServiceType.FullName != null && (
                        d.ServiceType.FullName.Contains("DbContext") ||
                        d.ServiceType.FullName.Contains("DbContextOptions") ||
                        d.ServiceType.FullName.Contains("Oracle") ||
                        d.ServiceType.FullName.Contains("EntityFramework")))
                    .ToList();

                foreach (var d in toRemove)
                    services.Remove(d);

                // Registra o InMemory limpo
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString("N"));
                });

                // Garante que o banco é criado antes dos testes
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();

                // Suprime logs durante testes
                services.AddLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Error);
                });
            });
        }
    }
}