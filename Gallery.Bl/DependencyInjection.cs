using Gallery.Bl.Data;
using Gallery.Bl.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Gallery.Bl;

// Spring uses @ComponentScan + @Service/@Repository annotations to auto-wire.
// ASP.NET Core uses explicit DI registration in a "Composition Root".
// This extension method bundles all BL registrations so Program.cs stays clean.
public static class DependencyInjection
{
    public static IServiceCollection AddGalleryBl(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<GalleryDbContext>(opt =>
            opt.UseNpgsql(connectionString));

        // Scoped = one instance per HTTP request, just like @Service/@Repository default scope in Spring.
        services.AddScoped<IAuthorService, AuthorService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IImageService, ImageService>();

        return services;
    }
}
