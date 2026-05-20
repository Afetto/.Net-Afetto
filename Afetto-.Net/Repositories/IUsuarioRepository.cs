using Afetto_.Net.Models;


namespace Afetto_.Net.Repositories
{
    public interface IUsuarioRepository
    {
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task<Usuario?> GetByIdAsync(Guid id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByCpfAsync(string cpf);
        Task<IEnumerable<Usuario>> GetByLogradouroAsync(Guid logradouroId);
        Task<IEnumerable<Usuario>> GetByCidadeAsync(Guid cidadeId);

        Task<Usuario> CreateAsync(Usuario usuario);
        Task<Usuario> UpdateAsync(Usuario usuario);
        Task DeleteAsync(Usuario usuario);

        Task<bool> ExistsAsync(Guid id);
        Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
        Task<bool> CpfExistsAsync(string cpf, Guid? excludeId = null);
    }
}