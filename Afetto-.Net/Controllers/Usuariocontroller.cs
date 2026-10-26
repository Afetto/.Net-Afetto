using Afetto_.Net.Hateoas;
using Afetto_.Net.Application.DTOs;
using Afetto_.Net.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Afetto_.Net.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _service;

        public UsuarioController(IUsuarioService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lista usuários com paginação, ordenação e filtros. Retorna links HATEOAS.
        /// </summary>
        /// <remarks>
        /// Exemplo: <c>GET /api/usuarios?page=1&amp;pageSize=10&amp;sortBy=nome&amp;sortDir=asc&amp;nome=maria</c>
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResponse<UsuarioResponse>), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetAll([FromQuery] UsuarioQueryParameters query)
        {
            var pagina = await _service.GetPagedAsync(query);

            pagina.Items.AdicionarLinks(Url, Request.Scheme);
            pagina.AdicionarLinks(Url, Request.Scheme, query);

            return Ok(pagina);
        }

        /// <summary>Retorna um usuário pelo ID.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var usuario = await _service.GetByIdAsync(id);
            if (usuario is null)
                return NotFound(new { message = $"Usuário com ID '{id}' não encontrado." });

            usuario.AdicionarLinks(Url, Request.Scheme);
            return Ok(usuario);
        }

        /// <summary>Retorna um usuário pelo e-mail.</summary>
        [HttpGet("email/{email}")]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var usuario = await _service.GetByEmailAsync(email);
            if (usuario is null)
                return NotFound(new { message = $"Usuário com e-mail '{email}' não encontrado." });

            usuario.AdicionarLinks(Url, Request.Scheme);
            return Ok(usuario);
        }

        /// <summary>Retorna um usuário pelo CPF.</summary>
        [HttpGet("cpf/{cpf}")]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByCpf(string cpf)
        {
            var usuario = await _service.GetByCpfAsync(cpf);
            if (usuario is null)
                return NotFound(new { message = $"Usuário com CPF '{cpf}' não encontrado." });

            usuario.AdicionarLinks(Url, Request.Scheme);
            return Ok(usuario);
        }

        /// <summary>Retorna todos os usuários de um logradouro.</summary>
        [HttpGet("logradouro/{logradouroId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByLogradouro(Guid logradouroId)
        {
            var usuarios = (await _service.GetByLogradouroAsync(logradouroId)).ToList();
            if (!usuarios.Any())
                return NotFound(new { message = "Nenhum usuário encontrado para este logradouro." });

            usuarios.AdicionarLinks(Url, Request.Scheme);
            return Ok(usuarios);
        }

        /// <summary>Retorna todos os usuários de uma cidade.</summary>
        [HttpGet("cidade/{cidadeId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByCidade(Guid cidadeId)
        {
            var usuarios = (await _service.GetByCidadeAsync(cidadeId)).ToList();
            if (!usuarios.Any())
                return NotFound(new { message = "Nenhum usuário encontrado para esta cidade." });

            usuarios.AdicionarLinks(Url, Request.Scheme);
            return Ok(usuarios);
        }

        /// <summary>Cria um novo usuário (cadastro público, não exige token).</summary>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(UsuarioResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // InvalidOperationException (e-mail/CPF duplicado) é tratada pelo ExceptionHandlingMiddleware
            var criado = await _service.CreateAsync(request);
            criado.AdicionarLinks(Url, Request.Scheme);
            return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
        }

        /// <summary>Atualiza os dados de um usuário existente.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(UsuarioResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUsuarioRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var atualizado = await _service.UpdateAsync(id, request);
            if (atualizado is null)
                return NotFound(new { message = $"Usuário com ID '{id}' não encontrado." });

            atualizado.AdicionarLinks(Url, Request.Scheme);
            return Ok(atualizado);
        }

        /// <summary>Remove um usuário pelo ID.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var removido = await _service.DeleteAsync(id);
            if (!removido)
                return NotFound(new { message = $"Usuário com ID '{id}' não encontrado." });

            return NoContent();
        }
    }
}
