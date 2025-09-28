using Media.Intf.Models;
using Media.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace Media.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController(MediaService MediaService, IConfiguration configuration) : ControllerBase
    {
        private readonly MediaService _MediaService = MediaService;
        private readonly IConfiguration _configuration = configuration;

        [HttpGet]
        public async Task<List<Media>> Get() =>
            await _MediaService.GetAsync();

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Media>> Get(ObjectId id)
        {
            var media = await _MediaService.GetAsync(id);

            if (media is null)
                return NotFound();

            return media;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Create(Media newMedia)
        {
            if (newMedia is null)
            {
                return BadRequest("Dados inválidos!");
            }

            var existingMedia = await _MediaService.AuthAsync(newMedia.Id);

            if (existingMedia is not null)
                return BadRequest("Media já existe");

            var novo = await _MediaService.CreateAsync(newMedia);

            if (novo is null)
                return BadRequest("Dados inválidos!");

            return CreatedAtAction(nameof(Get), new { id = novo.Id }, novo);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(ObjectId id, Media updatedMedia)
        {
            var media = await _MediaService.GetAsync(id);

            if (media is null)
                return NotFound();

            if (!String.IsNullOrEmpty(updatedMedia.Origem))
                media.Name = updatedMedia.Origem;

            if (!String.IsNullOrEmpty(updatedMedia.Tipo))
                media.Email = updatedMedia.Tipo;

            if (!String.IsNullOrEmpty(updatedMedia.Url))
                media.Url = updatedMedia(updatedMedia.Url);

            await _MediaService.UpdateAsync(id, Media);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(ObjectId id)
        {
            var media = await _MediaService.GetAsync(id);

            if (media is null)
                return NotFound();

            await _MediaService.DeleteAsync(id);

            return NoContent();
        }

    }
}