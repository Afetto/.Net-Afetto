using Afetto_.Net.Tests.Integration.Fixtures;
using Xunit;

namespace Afetto_.Net.Tests.Integration.Fixtures
{
    /// <summary>
    /// Collection fixture — compartilha a factory entre todos os testes da collection.
    /// Evita recriar o servidor a cada teste.
    /// </summary>
    [CollectionDefinition("Integration")]
    public class IntegrationCollection : ICollectionFixture<PetOSWebApplicationFactory> { }
}