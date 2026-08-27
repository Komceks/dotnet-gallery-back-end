using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Gallery.Bl.Services;

// Equivalent to your Java ThumbnailGenerator (imgscalr-based).
// ImageSharp is the de-facto cross-platform image library on .NET.
public static class ThumbnailGenerator
{
    private const int TargetWidth = 300;

    public static byte[] CreateThumbnail(byte[] source)
    {
        using var image = Image.Load(source);
        // Resize to TargetWidth, keep aspect ratio (height = 0 means "auto").
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(TargetWidth, 0),
            Mode = ResizeMode.Max
        }));

        using var ms = new MemoryStream();
        image.Save(ms, new JpegEncoder());
        return ms.ToArray();
    }
}
