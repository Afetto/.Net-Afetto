using Afetto_.Net.Data;
using Afetto_.Net.Models;
using Microsoft.EntityFrameworkCore;

namespace Afetto_.Net.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        // Inclui toda a hierarquia geográfica no SELECT
        private IQueryable<Usuario> QueryComEndereco() =>
            _context.Usuarios
                .Include(u => u.Logradouro)
                    .ThenInclude(l => l.Bairro)
                        .ThenInclude(b => b.Cidade)
                            .ThenInclude(c => c.Estado)
                                .ThenInclude(e => e.Pais)
                .AsNoTracking();

        public async Task<IEnumerable<Usuario>> GetAllAsync() =>
            await QueryComEndereco().ToListAsync();

        public async Task<Usuario?> GetByIdAsync(Guid id) =>
            await QueryComEndereco().FirstOrDefaultAsync(u => u.Id == id);

        public async Task<Usuario?> GetByEmailAsync(string email) =>
            await QueryComEndereco()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        public async Task<Usuario?> GetByCpfAsync(string cpf) =>
            await QueryComEndereco()
                .FirstOrDefaultAsync(u => u.Cpf == cpf);

        public async Task<IEnumerable<Usuario>> GetByLogradouroAsync(Guid logradouroId) =>
            await QueryComEndereco()
                .Where(u => u.LogradouroId == logradouroId)
                .ToListAsync();

        public async Task<IEnumerable<Usuario>> GetByCidadeAsync(Guid cidadeId) =>
            await QueryComEndereco()
                .Where(u => u.Logradouro.Bairro.CidadeId == cidadeId)
                .ToListAsync();

        public async Task<Usuario> CreateAsync(Usuario usuario)
        {
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task<Usuario> UpdateAsync(Usuario usuario)
        {
            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();
            return usuario;
        }

        public async Task DeleteAsync(Usuario usuario)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id) =>
            await _context.Usuarios.AnyAsync(u => u.Id == id);

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null) =>
            await _context.Usuarios.AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() &&
                (excludeId == null || u.Id != excludeId));

        public async Task<bool> CpfExistsAsync(string cpf, Guid? excludeId = null) =>
            await _context.Usuarios.AnyAsync(u =>
                u.Cpf == cpf &&
                (excludeId == null || u.Id != excludeId));
    }
}