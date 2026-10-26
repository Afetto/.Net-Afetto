namespace Afetto_.Net.Infrastructure.Security
{
    /// <summary>Configurações do JWT (seção "Jwt" do appsettings.json).</summary>
    public class JwtSettings
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int ExpiresInMinutes { get; set; } = 60;
    }
}
