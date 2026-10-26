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

        public async Task<(IReadOnlyList<Usuario> Items, int Total)> GetPagedAsync(UsuarioFiltro filtro)
        {
            var query = QueryComEndereco();

            if (!string.IsNullOrWhiteSpace(filtro.Nome))
            {
                var nome = filtro.Nome.Trim().ToLower();
                query = query.Where(u => u.Nome.ToLower().Contains(nome));
            }

            if (!string.IsNullOrWhiteSpace(filtro.Email))
            {
                var email = filtro.Email.Trim().ToLower();
                query = query.Where(u => u.Email.ToLower().Contains(email));
            }

            if (filtro.CidadeId.HasValue)
                query = query.Where(u => u.Logradouro.Bairro.CidadeId == filtro.CidadeId.Value);

            if (filtro.LogradouroId.HasValue)
                query = query.Where(u => u.LogradouroId == filtro.LogradouroId.Value);

            var total = await query.CountAsync();

            // ThenBy(Id) garante ordem estável entre páginas (empates no campo principal)
            var itens = await Ordenar(query, filtro.Ordenacao, filtro.Descendente)
                .ThenBy(u => u.Id)
                .Skip(filtro.Skip)
                .Take(filtro.Take)
                .ToListAsync();

            return (itens, total);
        }

        private static IOrderedQueryable<Usuario> Ordenar(
            IQueryable<Usuario> query, UsuarioOrdenacao ordenacao, bool descendente) =>
            ordenacao switch
            {
                UsuarioOrdenacao.Email => descendente ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
                UsuarioOrdenacao.DataNasc => descendente ? query.OrderByDescending(u => u.DataNasc) : query.OrderBy(u => u.DataNasc),
                UsuarioOrdenacao.CreatedAt => descendente ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt),
                _ => descendente ? query.OrderByDescending(u => u.Nome) : query.OrderBy(u => u.Nome)
            };

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