using Afetto_.Net.DTOs;
using Afetto_.Net.Models;
using Afetto_.Net.Repositories;


namespace Afetto_.Net.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _repo;

        public UsuarioService(IUsuarioRepository repo)
        {
            _repo = repo;
        }

        // ── GET ALL ───────────────────────────────────────────────────────────
        public async Task<IEnumerable<UsuarioResponse>> GetAllAsync()
        {
            var usuarios = await _repo.GetAllAsync();
            return usuarios.Select(ParaResponse);
        }

        // ── GET BY ID ─────────────────────────────────────────────────────────
        public async Task<UsuarioResponse?> GetByIdAsync(Guid id)
        {
            var usuario = await _repo.GetByIdAsync(id);
            return usuario is null ? null : ParaResponse(usuario);
        }

        // ── GET BY EMAIL ──────────────────────────────────────────────────────
        public async Task<UsuarioResponse?> GetByEmailAsync(string email)
        {
            var usuario = await _repo.GetByEmailAsync(email);
            return usuario is null ? null : ParaResponse(usuario);
        }

        // ── GET BY CPF ────────────────────────────────────────────────────────
        public async Task<UsuarioResponse?> GetByCpfAsync(string cpf)
        {
            var usuario = await _repo.GetByCpfAsync(cpf);
            return usuario is null ? null : ParaResponse(usuario);
        }

        // ── GET BY LOGRADOURO ─────────────────────────────────────────────────
        public async Task<IEnumerable<UsuarioResponse>> GetByLogradouroAsync(Guid logradouroId)
        {
            var usuarios = await _repo.GetByLogradouroAsync(logradouroId);
            return usuarios.Select(ParaResponse);
        }

        // ── GET BY CIDADE ─────────────────────────────────────────────────────
        public async Task<IEnumerable<UsuarioResponse>> GetByCidadeAsync(Guid cidadeId)
        {
            var usuarios = await _repo.GetByCidadeAsync(cidadeId);
            return usuarios.Select(ParaResponse);
        }

        // ── CREATE ────────────────────────────────────────────────────────────
        public async Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request)
        {
            if (await _repo.EmailExistsAsync(request.Email))
                throw new InvalidOperationException($"E-mail '{request.Email}' já está em uso.");

            if (await _repo.CpfExistsAsync(request.Cpf))
                throw new InvalidOperationException($"CPF '{request.Cpf}' já está cadastrado.");

            var usuario = new Usuario
            {
                Nome = request.Nome.Trim(),
                Cpf = request.Cpf.Trim(),
                DataNasc = request.DataNasc,
                Email = request.Email.Trim().ToLower(),
                Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha, workFactor: 12),
                Telefone = request.Telefone.Trim(),
                Numero = request.Numero.Trim(),
                Complemento = request.Complemento?.Trim(),
                LogradouroId = request.LogradouroId,
                CreatedAt = DateTime.UtcNow
            };

            var criado = await _repo.CreateAsync(usuario);

            // Recarrega com endereço completo para retornar no response
            var completo = await _repo.GetByIdAsync(criado.Id);
            return ParaResponse(completo!);
        }

        // ── UPDATE ────────────────────────────────────────────────────────────
        public async Task<UsuarioResponse?> UpdateAsync(Guid id, UpdateUsuarioRequest request)
        {
            var usuario = await _repo.GetByIdAsync(id);
            if (usuario is null) return null;

            if (await _repo.EmailExistsAsync(request.Email, excludeId: id))
                throw new InvalidOperationException($"E-mail '{request.Email}' já está em uso por outro usuário.");

            usuario.Nome = request.Nome.Trim();
            usuario.DataNasc = request.DataNasc;
            usuario.Email = request.Email.Trim().ToLower();
            usuario.Telefone = request.Telefone.Trim();
            usuario.Numero = request.Numero.Trim();
            usuario.Complemento = request.Complemento?.Trim();
            usuario.LogradouroId = request.LogradouroId;

            await _repo.UpdateAsync(usuario);

            var atualizado = await _repo.GetByIdAsync(id);
            return ParaResponse(atualizado!);
        }

        // ── DELETE ────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(Guid id)
        {
            var usuario = await _repo.GetByIdAsync(id);
            if (usuario is null) return false;

            await _repo.DeleteAsync(usuario);
            return true;
        }

        // ── MAPEAMENTO Entity → DTO ───────────────────────────────────────────
        private static UsuarioResponse ParaResponse(Usuario u)
        {
            EnderecoResponse? endereco = null;

            if (u.Logradouro is not null)
            {
                endereco = new EnderecoResponse
                {
                    Logradouro = u.Logradouro.Nome,
                    Cep = u.Logradouro.Cep,
                    Tipo = u.Logradouro.Tipo,
                    Bairro = u.Logradouro.Bairro?.Nome ?? string.Empty,
                    Cidade = u.Logradouro.Bairro?.Cidade?.Nome ?? string.Empty,
                    Estado = u.Logradouro.Bairro?.Cidade?.Estado?.Nome ?? string.Empty,
                    Pais = u.Logradouro.Bairro?.Cidade?.Estado?.Pais?.Nome ?? string.Empty
                };
            }

            return new UsuarioResponse
            {
                Id = u.Id,
                Nome = u.Nome,
                Cpf = u.Cpf,
                DataNasc = u.DataNasc,
                Email = u.Email,
                Telefone = u.Telefone,
                Numero = u.Numero,
                Complemento = u.Complemento,
                CreatedAt = u.CreatedAt,
                LogradouroId = u.LogradouroId,
                Endereco = endereco
            };
        }
    }
}