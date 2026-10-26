namespace Afetto_.Net.Application.Interfaces
{
    /// <summary>Identifica quem está executando a operação (abstrai o contexto HTTP da camada Application).</summary>
    public interface ICurrentUserService
    {
        /// <summary>E-mail (ou id) do usuário autenticado; "anonimo" quando não há autenticação.</summary>
        string Identificador { get; }
    }
}
