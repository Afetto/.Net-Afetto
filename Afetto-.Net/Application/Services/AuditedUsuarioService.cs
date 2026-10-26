using Afetto_.Net.Application.DTOs;
using Afetto_.Net.Application.Interfaces;
using Afetto_.Net.Domain.Entities;
using Afetto_.Net.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Afetto_.Net.Application.Services
{
    /// <summary>
    /// Decorator de <see cref="IUsuarioService"/>: delega ao serviço real e registra no MongoDB
    /// as operações de escrita (criado/atualizado/removido). Falha no Mongo nunca derruba a operação principal.
    /// </summary>
    public class AuditedUsuarioService : IUsuarioService
    {
        private readonly IUsuarioService _inner;
        private readonly IAuditLogRepository _audit;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<AuditedUsuarioService> _logger;

        public AuditedUsuarioService(
            IUsuarioService inner,
            IAuditLogRepository audit,
            ICurrentUserService currentUser,
            ILogger<AuditedUsuarioService> logger)
        {
            _inner = inner;
            _audit = audit;
            _currentUser = currentUser;
            _logger = logger;
        }

        // ── Leituras: apenas delegam ──────────────────────────────────────────
        public Task<IEnumerable<UsuarioResponse>> GetAllAsync() => _inner.GetAllAsync();
        public Task<PagedResponse<UsuarioResponse>> GetPagedAsync(UsuarioQueryParameters query) => _inner.GetPagedAsync(query);
        public Task<UsuarioResponse?> GetByIdAsync(Guid id) => _inner.GetByIdAsync(id);
        public Task<UsuarioResponse?> GetByEmailAsync(string email) => _inner.GetByEmailAsync(email);
        public Task<UsuarioResponse?> GetByCpfAsync(string cpf) => _inner.GetByCpfAsync(cpf);
        public Task<IEnumerable<UsuarioResponse>> GetByLogradouroAsync(Guid logradouroId) => _inner.GetByLogradouroAsync(logradouroId);
        public Task<IEnumerable<UsuarioResponse>> GetByCidadeAsync(Guid cidadeId) => _inner.GetByCidadeAsync(cidadeId);

        // ── Escritas: delegam e auditam ───────────────────────────────────────
        public async Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request)
        {
            var criado = await _inner.CreateAsync(request);
            await RegistrarAsync(criado.Id, "criado", $"Usuário '{criado.Email}' criado.");
            return criado;
        }

        public async Task<UsuarioResponse?> UpdateAsync(Guid id, UpdateUsuarioRequest request)
        {
            var atualizado = await _inner.UpdateAsync(id, request);
            if (atualizado is not null)
                await RegistrarAsync(id, "atualizado", $"Usuário '{atualizado.Email}' atualizado.");
            return atualizado;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var removido = await _inner.DeleteAsync(id);
            if (removido)
                await RegistrarAsync(id, "removido", "Usuário removido.");
            return removido;
        }

        private async Task RegistrarAsync(Guid entidadeId, string acao, string detalhes)
        {
            try
            {
                await _audit.RegistrarAsync(new AuditLog
                {
                    Entidade = "Usuario",
                    EntidadeId = entidadeId.ToString(),
                    Acao = acao,
                    Usuario = _currentUser.Identificador,
                    Detalhes = detalhes,
                    OcorridoEm = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Falha ao registrar auditoria '{Acao}' do usuário {Id} no repositório de auditoria.", acao, entidadeId);
            }
        }
    }
}
