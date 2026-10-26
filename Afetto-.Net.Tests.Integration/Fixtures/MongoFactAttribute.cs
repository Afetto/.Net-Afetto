using Xunit;

namespace Afetto_.Net.Tests.Integration.Fixtures
{
    /// <summary>
    /// Teste que exige um MongoDB real. Só executa se a variável de ambiente
    /// MONGO_TEST_CONNECTION estiver definida (ex.: mongodb://localhost:27017); caso contrário é ignorado.
    /// </summary>
    public sealed class MongoFactAttribute : FactAttribute
    {
        public const string EnvVar = "MONGO_TEST_CONNECTION";

        public MongoFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(EnvVar)))
                Skip = $"Defina {EnvVar} para executar contra um MongoDB real.";
        }
    }
}
