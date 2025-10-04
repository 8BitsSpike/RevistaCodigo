using Media.Intf.Models;
using Media.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

[ApiController]
[Route("api/[controller]")]
public class MediaController(MediaService MediaService, IConfiguration configuration) : ControllerBase
{
    private readonly MediaService _MediaService = MediaService;
    private readonly IConfiguration _configuration = configuration;

    [HttpGet]
    [ProducesResponseType(typeof(List<Midia>), StatusCodes.Status200OK)]
    public async Task<List<Midia>> Get() =>
        await _MediaService.GetMediaAsync();

    [HttpGet("{id:length(24)}")]
    [ProducesResponseType(typeof(Midia), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Midia>> Get(string id)
    {
        var media = await _MediaService.GetMediaAsync(id);

        if (media is null)
            return NotFound();

        return media;
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    [ProducesResponseType(typeof(Midia), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(Midia newMidia)
    {
        if (newMidia is null)
        {
            return BadRequest("Dados inválidos!");
        }

        if (newMidia.Id != ObjectId.Empty)
        {
            var existingMedia = await _MediaService.GetMediaAsync(newMidia.Id.ToString());
            if (existingMedia is not null)
                return BadRequest("Media já existe");
        }

        var novo = await _MediaService.CreateMediaAsync(newMidia);

        if (novo is null)
            return BadRequest("Falha ao criar mídia!");

        return CreatedAtAction(nameof(Get), new { id = novo.Id.ToString() }, novo);
    }

    [HttpPut("{id:length(24)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, Midia updatedMidia)
    {
        var media = await _MediaService.GetMediaAsync(id);

        if (media is null)
            return NotFound();

        if (updatedMidia.Origem != ObjectId.Empty)
            media.Origem = updatedMidia.Origem;

        if (!string.IsNullOrEmpty(updatedMidia.Tipo))
            media.Tipo = updatedMidia.Tipo;

        if (!string.IsNullOrEmpty(updatedMidia.Url))
            media.Url = updatedMidia.Url;

        await _MediaService.UpdateMediaAsync(id, media);

        return NoContent();
    }

    [HttpDelete("{id:length(24)}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var media = await _MediaService.GetMediaAsync(id);

        if (media is null)
            return NotFound();

        await _MediaService.DeleteMediaAsync(id);

        return NoContent();
    }

}
