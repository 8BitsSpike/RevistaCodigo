using Media.Intf.Models;
using Media.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using static Media.Intf.Models.MidiaMappers;

[ApiController]
[Route("api/[controller]")]
public class MediaController(MediaService MediaService, IConfiguration configuration) : ControllerBase
{
    private readonly MediaService _MediaService = MediaService;
    private readonly IConfiguration _configuration = configuration;

    [HttpGet]
    [ProducesResponseType(typeof(List<MidiaDTO>), StatusCodes.Status200OK)]
    public async Task<List<MidiaDTO>> Get() =>
        await _MediaService.GetMediaAsync(); 


    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MidiaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MidiaDTO>> Get(string id)
    {
        var media = await _MediaService.GetMediaAsync(id);

        if (media is null)
            return NotFound();

        return media;
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    [ProducesResponseType(typeof(MidiaDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(MidiaDTO newMidiaDTO)
    {
        if (newMidiaDTO is null)
        {
            return BadRequest("Dados inválidos!");
        }


        var newMidia = newMidiaDTO.ToDbModel();

        if (newMidia.Id != ObjectId.Empty)
        {
            var existingMedia = await _MediaService.GetMediaAsync(newMidia.Id.ToString());
            if (existingMedia is not null)
                return BadRequest("Media já existe");
        }

        var novo = await _MediaService.CreateMediaAsync(newMidia);

        if (novo is null)
            return BadRequest("Falha ao criar mídia!");

        var novoDTO = novo.ToDto();

        return CreatedAtAction(nameof(Get), new { id = novoDTO.Id }, novoDTO);
    }


    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, MidiaDTO updatedMidiaDTO)
    {
        var mediaDTO = await _MediaService.GetMediaAsync(id);

        if (mediaDTO is null)
            return NotFound();

        var updatedMidia = updatedMidiaDTO.ToDbModel();

        await _MediaService.UpdateMediaAsync(id, updatedMidia);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var mediaDTO = await _MediaService.GetMediaAsync(id);

        if (mediaDTO is null)
            return NotFound();

        await _MediaService.DeleteMediaAsync(id);

        return NoContent();
    }
}
