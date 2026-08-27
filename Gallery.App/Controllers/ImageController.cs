using Gallery.App.Dto;
using Gallery.App.Mappers;
using Gallery.Bl.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gallery.App.Controllers;

// Mirrors Spring's ImageController endpoint-for-endpoint:
//   POST   /api/image/upload   (multipart)
//   POST   /api/image/search   (JSON body)
//   GET    /api/image/{id}
//   POST   /api/image/update   (multipart)
//   DELETE /api/image/{id}
[ApiController]
[Route("image")]
public class ImageController : ControllerBase
{
    private readonly IImageService _service;

    public ImageController(IImageService service) => _service = service;

    // ---- POST /api/image/upload ----
    [HttpPost("upload")]
    public async Task<IActionResult> Upload(CancellationToken ct)
    {
        var (dto, file) = await MultipartReader.ReadImageRequestAsync(Request, fileRequired: true);

        // Manual validation that maps to @Validated(CreateRequest.class) + @NotBlank etc.
        if (dto.Id is not null) return BadRequest("Id must be null on create.");
        if (string.IsNullOrWhiteSpace(dto.ImageName)) return BadRequest("imageName is required.");
        if (string.IsNullOrWhiteSpace(dto.AuthorName)) return BadRequest("authorName is required.");
        if (dto.Date > DateOnly.FromDateTime(DateTime.UtcNow)) return BadRequest("date must be in the past or present.");

        await _service.UploadAsync(dto.ToCreateModel(file!), ct);
        return Ok();
    }

    // ---- POST /api/image/search ----
    [HttpPost("search")]
    public async Task<ActionResult<SpringPage<ThumbnailListDto>>> Search(
        [FromBody] ImageSearchRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (!request.IsValid()) return BadRequest("Either query or imageSearchRequestPart should be set");

        var result = await _service.SearchAsync(request.ToModel(), ct);
        var page = SpringPage<ThumbnailListDto>.From(result, ThumbnailListDto.Of,
            new Bl.Models.ImageSortModel(request.Sort.Field, request.Sort.Order));
        return Ok(page);
    }

    // ---- GET /api/image/{id} ----
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ImageViewResponse>> View(long id, CancellationToken ct)
    {
        var view = await _service.ViewAsync(id, ct);
        return view is null ? NotFound() : Ok(ImageViewResponse.Of(view));
    }

    // ---- POST /api/image/update ----
    [HttpPost("update")]
    public async Task<ActionResult<ImageUpdateResponse>> Update(CancellationToken ct)
    {
        var (dto, file) = await MultipartReader.ReadImageRequestAsync(Request, fileRequired: false);

        if (dto.Id is null) return BadRequest("Id is required on update.");
        if (string.IsNullOrWhiteSpace(dto.ImageName)) return BadRequest("imageName is required.");
        if (string.IsNullOrWhiteSpace(dto.AuthorName)) return BadRequest("authorName is required.");
        if (dto.Date > DateOnly.FromDateTime(DateTime.UtcNow)) return BadRequest("date must be in the past or present.");

        try
        {
            var result = await _service.UpdateAsync(dto.ToUpdateModel(file), ct);
            return Ok(ImageUpdateResponse.Of(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ---- DELETE /api/image/{id} ----
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
