using System.Text.Json.Serialization;
using Gallery.Bl;
using Gallery.Bl.Data;
using Microsoft.EntityFrameworkCore;

// ============================================================================
// Program.cs is the .NET equivalent of:
//   - @SpringBootApplication class (the entry point)
//   - application.properties (config binding)
//   - WebMvcConfigurer / SecurityConfig (middleware setup)
//
// Top-level statements + minimal hosting: no `static void main(String[])`,
// no @ComponentScan — everything is registered explicitly below.
// ============================================================================

var builder = WebApplication.CreateBuilder(args);

// ---- 1. Services (DI container) — equivalent to @Bean + @ComponentScan ----

// Read connection string from appsettings.json (the .NET application.properties).
var connectionString = builder.Configuration.GetConnectionString("Gallery")
    ?? throw new InvalidOperationException("Missing connection string 'Gallery'.");

builder.Services.AddGalleryBl(connectionString);

builder.Services
    .AddControllers()
    .AddJsonOptions(opt =>
    {
        // Match Jackson defaults: camelCase property names, enums as strings ("ASC", "UPLOAD_DATE").
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// CORS — your Angular runs on its own port via `ng serve`. The frontend's proxy.conf.json
// already forwards /api → localhost:8080, so CORS would only matter if you skip the proxy.
// We add a permissive policy here for local development.
builder.Services.AddCors(opt => opt.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// Spring's swagger-ui equivalent.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ---- 2. Middleware pipeline — like Spring's filter chain ----

// Spring property `server.servlet.contextPath=/api` becomes UsePathBase here.
// Every controller route ([Route("image")], [Route("greeting")]) is automatically prefixed with /api.
app.UsePathBase("/api");
app.UseRouting();

app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/api/swagger/v1/swagger.json", "Gallery API"));
}

app.MapControllers();

// ---- 3. Auto-apply EF Core migrations on startup (dev-friendly) ----
// In production you'd run `dotnet ef database update` deliberately.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<GalleryDbContext>();
    db.Database.Migrate();
}

app.Run();
