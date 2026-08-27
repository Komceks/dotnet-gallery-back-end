using System.Text.Json;
using Gallery.App.Dto;

namespace Gallery.App.Mappers;

// Spring's @RequestPart auto-deserializes JSON-typed multipart parts.
// ASP.NET Core's default model binder doesn't, so we do it explicitly.
// The Angular code sends "dto" as a Blob with type:application/json plus an "imageFile" part.
public static class MultipartReader
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public static async Task<(ImageSaveRequest Dto, byte[]? FileBytes)> ReadImageRequestAsync(
        HttpRequest request, bool fileRequired)
    {
        if (!request.HasFormContentType)
            throw new BadHttpRequestException("Expected multipart/form-data.");

        var form = await request.ReadFormAsync();

        // The "dto" part is a Blob of application/json — it arrives as a file in Form.Files.
        var dtoPart = form.Files["dto"]
            ?? throw new BadHttpRequestException("Missing 'dto' part.");

        using var reader = new StreamReader(dtoPart.OpenReadStream());
        var dtoJson = await reader.ReadToEndAsync();
        var dto = JsonSerializer.Deserialize<ImageSaveRequest>(dtoJson, JsonOpts)
            ?? throw new BadHttpRequestException("Invalid 'dto' JSON.");

        // The "imageFile" part — required on create, optional on update.
        var imageFile = form.Files["imageFile"];
        if (imageFile is null)
        {
            if (fileRequired) throw new BadHttpRequestException("Missing 'imageFile' part.");
            return (dto, null);
        }

        using var ms = new MemoryStream();
        await imageFile.CopyToAsync(ms);
        return (dto, ms.ToArray());
    }
}
