using Afetto_.Net.DTOs;


namespace Afetto_.Net.Services
{
    public interface IUsuarioService
    {
        Task<IEnumerable<UsuarioResponse>> GetAllAsync();
        Task<UsuarioResponse?> GetByIdAsync(Guid id);
        Task<UsuarioResponse?> GetByEmailAsync(string email);
        Task<UsuarioResponse?> GetByCpfAsync(string cpf);
        Task<IEnumerable<UsuarioResponse>> GetByLogradouroAsync(Guid logradouroId);
        Task<IEnumerable<UsuarioResponse>> GetByCidadeAsync(Guid cidadeId);
        Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request);
        Task<UsuarioResponse?> UpdateAsync(Guid id, UpdateUsuarioRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}