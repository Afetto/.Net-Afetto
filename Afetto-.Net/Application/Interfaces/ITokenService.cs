namespace Afetto_.Net.Application.Interfaces
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiraEm) GerarToken(Guid usuarioId, string nome, string email);
    }
}
