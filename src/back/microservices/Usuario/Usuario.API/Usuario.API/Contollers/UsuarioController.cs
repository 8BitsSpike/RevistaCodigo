using Usuario.Intf.Models;
using Usuario.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Usuario.Api.Controllers
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

        [HttpGet]
        public async Task<List<Usuar>> Get() =>
            await _usuarioService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Usuar>> Get(ObjectId id)
        {
            var usuario = await _usuarioService.GetAsync(id);

            if (usuario is null)
                return NotFound();

            return usuario;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Create(UsuarioDto newUsuario)
        {
            if (newUsuario is null)
            {
                return BadRequest("Dados inválidos!");
            }

            var existingUsuario = await _usuarioService.AuthAsync(newUsuario.Email);

            if (existingUsuario is not null)
                return BadRequest("Email já está em uso!");

            var novo = await _usuarioService.CreateAsync(newUsuario);

            if (novo is null)
                return BadRequest("Dados inválidos!");

            return CreatedAtAction(nameof(Get), new { id = novo.Id }, novo);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(ObjectId id, UsuarioDto updatedUsuario)
        {
            var existingUsuario = await _usuarioService.GetAsync(id);

            if (existingUsuario is null)
                return NotFound();

            if (!String.IsNullOrEmpty(updatedUsuario.Name))
                existingUsuario.Name = updatedUsuario.Name;

            if (!String.IsNullOrEmpty(updatedUsuario.Email))
                existingUsuario.Email = updatedUsuario.Email;

            if (!String.IsNullOrEmpty(updatedUsuario.Password))
                existingUsuario.Password = BCrypt.Net.BCrypt.HashPassword(updatedUsuario.Password);

            await _usuarioService.UpdateAsync(id, existingUsuario);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(ObjectId id)
        {
            var usuario = await _usuarioService.GetAsync(id);

            if (usuario is null)
                return NotFound();

            await _usuarioService.DeleteAsync(id);

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateDto model)
        {
            var usuario = await _usuarioService.AuthAsync(model.Email);

            if (usuario is null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.Password))
                return Unauthorized();

            var jwt = _usuarioService.GenerateJwtToken(usuario);

            return Ok(new { jwtToken = jwt, id = usuario.Id });
        }

        [AllowAnonymous]
        [HttpGet("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword(string email)
        {
            var usuario = await _usuarioService.AuthAsync(email);
            if (usuario is null)
                return BadRequest("O Email não existe");

            var jwt = _usuarioService.GenerateJwtToken(usuario);

            try
            {
                var frontendBaseUrl = _configuration["FrontendUrl"];
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];

                if (string.IsNullOrEmpty(frontendBaseUrl) || string.IsNullOrEmpty(senderEmail))
                {
                    return StatusCode(500, "Server configuration error.");
                }

                string recoveryLink = $"{frontendBaseUrl}/alterandoSenha/?id={usuario.Id}&token={jwt}";

                using (var smtpClient = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new System.Net.NetworkCredential(senderEmail, senderPassword);

                    var emailMessage = new System.Net.Mail.MailMessage
                    {
                        From = new System.Net.Mail.MailAddress(senderEmail, "Revista Brasileira da Educação Básica"),
                        Subject = "Redefinição de Senha de acesso à Revista Brasileira da Educação Básica",
                        Body = $"Olá, {usuario.Name}, para redefinir sua senha, clique no link: <a href='{recoveryLink}'>Recuperar senha</a>",
                        IsBodyHtml = true,
                    };
                    emailMessage.To.Add(email);

                    await smtpClient.SendMailAsync(emailMessage);
                }

                return Ok("Password recovery email sent.");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, "An error occurred while sending the recovery email.");
            }
        }
    }
}
