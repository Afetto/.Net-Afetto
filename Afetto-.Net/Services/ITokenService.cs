namespace Afetto_.Net.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiraEm) GerarToken(Guid usuarioId, string nome, string email);
    }
}
