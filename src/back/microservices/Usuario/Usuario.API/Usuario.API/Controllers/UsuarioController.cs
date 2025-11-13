using Usuario.Intf.Models;
using Usuario.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using BCrypt.Net;
using Microsoft.Extensions.Configuration;

namespace Usuario.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly UsuarioService _usuarioService;
        private readonly IConfiguration _configuration;

        public UsuarioController(UsuarioService usuarioService, IConfiguration configuration)
        {
            _usuarioService = usuarioService;
            _configuration = configuration;
        }

        // --- GET ALL ---
        [HttpGet]
        public async Task<List<Usuario.Intf.Models.Usuario>> Get() =>
            await _usuarioService.GetAsync();

        // --- GET BY ID ---
        [HttpGet("{id}")]
        public async Task<ActionResult<Usuario.Intf.Models.Usuario>> Get(string id)
        {
            var trimmedId = id.Trim();

            if (!ObjectId.TryParse(trimmedId, out var objectId))
                return BadRequest("O ID fornecido não é um formato válido do MongoDB.");

            var usuario = await _usuarioService.GetAsync(objectId);
            if (usuario is null)
                return NotFound();

            return usuario;
        }

        // --- POST /Register ---
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Create(UsuarioDto newUsuario)
        {
            if (newUsuario is null)
                return BadRequest("Dados inválidos!");

            var existingUsuario = await _usuarioService.FindUser(newUsuario.Email);
            if (existingUsuario is not null)
                return BadRequest("Email já está em uso!");

            if (!string.IsNullOrEmpty(newUsuario.Password))
            {
                if (newUsuario.Password != newUsuario.PasswordConfirm)
                    return BadRequest("As senhas não são iguais");

                newUsuario.Password = BCrypt.Net.BCrypt.HashPassword(newUsuario.Password);
            }
            else
            {
                return BadRequest("Por favor inserir uma senha");
            }

            var novo = await _usuarioService.CreateAsync(newUsuario);
            if (novo is null)
                return BadRequest("Erro ao criar usuário.");

            return CreatedAtAction(nameof(Get), new { id = novo.Id.ToString() }, novo);
        }

        // --- PUT (ATUALIZAR) ---
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, Usuario.Intf.Models.Usuario updatedUsuario)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("O ID fornecido não é um formato válido do MongoDB.");

            var existingUsuario = await _usuarioService.GetAsync(objectId);
            if (existingUsuario is null)
                return NotFound();

            if (!string.IsNullOrEmpty(updatedUsuario.Name))
                existingUsuario.Name = updatedUsuario.Name;

            if (!string.IsNullOrEmpty(updatedUsuario.Sobrenome))
                existingUsuario.Sobrenome = updatedUsuario.Sobrenome;

            if (!string.IsNullOrEmpty(updatedUsuario.Email))
                existingUsuario.Email = updatedUsuario.Email;

            if (!string.IsNullOrEmpty(updatedUsuario.Password))
                existingUsuario.Password = BCrypt.Net.BCrypt.HashPassword(updatedUsuario.Password);

            if (!string.IsNullOrEmpty(updatedUsuario.Foto))
                existingUsuario.Foto = updatedUsuario.Foto;

            if (!string.IsNullOrEmpty(updatedUsuario.Biografia))
                existingUsuario.Biografia = updatedUsuario.Biografia;

            if (updatedUsuario.InfoInstitucionais != null)
            {
                existingUsuario.InfoInstitucionais.Clear();
                existingUsuario.InfoInstitucionais.AddRange(updatedUsuario.InfoInstitucionais);
            }

            if (updatedUsuario.Atuacoes != null)
            {
                existingUsuario.Atuacoes.Clear();
                existingUsuario.Atuacoes.AddRange(updatedUsuario.Atuacoes);
            }

            await _usuarioService.UpdateAsync(objectId, existingUsuario);
            return NoContent(); // 204 Sucesso
        }

        // --- DELETE ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                return BadRequest("O ID fornecido não é um formato válido do MongoDB.");

            var usuario = await _usuarioService.GetAsync(objectId);
            if (usuario is null)
                return NotFound();

            await _usuarioService.DeleteAsync(objectId);
            return NoContent(); // 204 Sucesso
        }

        // --- POST /Authenticate ---
        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(UserDto model)
        {
            var usuario = await _usuarioService.FindUser(model.Email);

            if (usuario is null)
                return Unauthorized(new { message = "Usuário não encontrado" });

            if (!BCrypt.Net.BCrypt.Verify(model.Password, usuario.Password))
                return Unauthorized(new { message = "Senha inválida" });

            var jwt = _usuarioService.GenerateJwtToken(usuario);
            return Ok(new { jwtToken = jwt, id = usuario.Id.ToString() });
        }

        // --- ENDPOINTS DE RECUPERAÇÃO DE SENHA ---

        // Solicita o link de recuperação
        [AllowAnonymous]
        [HttpPost("RequestPasswordReset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] RecoverPasswordRequestDto request)
        {
            var result = await _usuarioService.RequestPasswordRecoveryAsync(request.Email);

            if (result.IsSuccess)
                return Ok("Se o e-mail estiver cadastrado, um link de recuperação foi enviado.");
            else
                return StatusCode(result.StatusCode, result.Message);
        }

        // Redefine a senha usando o ID do usuário e o token
        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
        {
            if (!ObjectId.TryParse(request.UserId, out var objectId))
                return BadRequest("ID de usuário inválido.");

            var result = await _usuarioService.ResetPasswordAsync(objectId, request.Token, request.NewPassword);

            if (result.IsSuccess)
                return Ok(result.Message); // 200 OK
            else
                return StatusCode(result.StatusCode, result.Message);
        }
    }
}
