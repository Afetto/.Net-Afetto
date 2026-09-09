using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Afetto_.Net.DTOs;
using Afetto_.Net.Services;

namespace Afetto_.Net.Controllers
{
    [ApiController]
    [Route("api/usuarios")]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _service;

        public UsuarioController(IUsuarioService service)
        {
            _service = service;
        }

        /// <summary>Retorna todos os usuários cadastrados.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), 200)]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _service.GetAllAsync();
            return Ok(usuarios);
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

            return Ok(usuario);
        }

        /// <summary>Retorna todos os usuários de um logradouro.</summary>
        [HttpGet("logradouro/{logradouroId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByLogradouro(Guid logradouroId)
        {
            var usuarios = await _service.GetByLogradouroAsync(logradouroId);
            if (!usuarios.Any())
                return NotFound(new { message = "Nenhum usuário encontrado para este logradouro." });

            return Ok(usuarios);
        }

        /// <summary>Retorna todos os usuários de uma cidade.</summary>
        [HttpGet("cidade/{cidadeId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<UsuarioResponse>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetByCidade(Guid cidadeId)
        {
            var usuarios = await _service.GetByCidadeAsync(cidadeId);
            if (!usuarios.Any())
                return NotFound(new { message = "Nenhum usuário encontrado para esta cidade." });

            return Ok(usuarios);
        }

        /// <summary>Cria um novo usuário.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(UsuarioResponse), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var criado = await _service.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = criado.Id }, criado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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

            try
            {
                var atualizado = await _service.UpdateAsync(id, request);
                if (atualizado is null)
                    return NotFound(new { message = $"Usuário com ID '{id}' não encontrado." });

                return Ok(atualizado);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
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