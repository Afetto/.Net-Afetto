using Afetto_.Net.DTOs;
using Afetto_.Net.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Afetto_.Net.Controllers
{
    /// <summary>Consulta dos logs de auditoria armazenados no MongoDB.</summary>
    [ApiController]
    [Authorize]
    [Route("api/auditoria")]
    public class AuditoriaController : ControllerBase
    {
        private readonly IAuditLogRepository _repository;

        public AuditoriaController(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        /// <summary>Lista logs de auditoria (paginado, mais recentes primeiro) com links HATEOAS.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<AuditLogResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Get([FromQuery] AuditoriaQueryParameters query, CancellationToken cancellationToken)
        {
            var (logs, total) = await _repository.ListarAsync(new AuditLogFiltro
            {
                EntidadeId = query.EntidadeId?.ToString(),
                Acao = query.Acao,
                Skip = (query.Page - 1) * query.PageSize,
                Take = query.PageSize
            }, cancellationToken);

            var itens = logs.Select(l =>
            {
                var item = new AuditLogResponse
                {
                    Id = l.Id ?? string.Empty,
                    Entidade = l.Entidade,
                    EntidadeId = l.EntidadeId,
                    Acao = l.Acao,
                    Usuario = l.Usuario,
                    Detalhes = l.Detalhes,
                    OcorridoEm = l.OcorridoEm
                };

                // Usuário removido não possui mais recurso para navegar
                if (l.Acao != "removido" && Guid.TryParse(l.EntidadeId, out var id))
                    item.Links.Add(new LinkDto
                    {
                        Rel = "usuario",
                        Method = "GET",
                        Href = Url.Action("GetById", "Usuario", new { id }, Request.Scheme)!
                    });

                return item;
            }).ToList();

            var totalPaginas = (int)Math.Ceiling(total / (double)query.PageSize);

            string Href(int page) => Url.Action("Get", "Auditoria", new
            {
                page,
                pageSize = query.PageSize,
                entidadeId = query.EntidadeId,
                acao = query.Acao
            }, Request.Scheme)!;

            var pagina = new PagedResponse<AuditLogResponse>
            {
                Items = itens,
                Page = query.Page,
                PageSize = query.PageSize,
                TotalItems = (int)total,
                TotalPages = totalPaginas,
                Links = new List<LinkDto>
                {
                    new() { Rel = "self",  Href = Href(query.Page) },
                    new() { Rel = "first", Href = Href(1) },
                    new() { Rel = "last",  Href = Href(Math.Max(totalPaginas, 1)) }
                }
            };

            if (query.Page > 1)
                pagina.Links.Add(new LinkDto { Rel = "prev", Href = Href(query.Page - 1) });
            if (query.Page < totalPaginas)
                pagina.Links.Add(new LinkDto { Rel = "next", Href = Href(query.Page + 1) });

            return Ok(pagina);
        }
    }
}
