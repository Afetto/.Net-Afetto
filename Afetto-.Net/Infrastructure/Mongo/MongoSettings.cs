namespace Afetto_.Net.Infrastructure.Mongo
{
    /// <summary>Configuração do MongoDB (seção "MongoDb" do appsettings.json).</summary>
    public class MongoSettings
    {
        public string ConnectionString { get; set; } = "mongodb://localhost:27017";
        public string DatabaseName { get; set; } = "afetto";
        public string AuditCollection { get; set; } = "audit_logs";

        /// <summary>Tempo máximo (s) para encontrar o servidor Mongo antes de falhar.</summary>
        public int TimeoutSeconds { get; set; } = 3;
    }
}
