# Gallery — .NET 10 port of `gallery-back-end`

A working ASP.NET Core 10 backend that serves the same API contract as
[`Komceks/gallery-back-end`](https://github.com/Komceks/gallery-back-end), so the existing
[`Komceks/gallery-front-end`](https://github.com/Komceks/gallery-front-end) Angular app
works against it unchanged.

## Why the structure looks familiar

The Spring project has three Maven modules. This .NET solution has three projects with the same roles:

| Spring (Maven module) | .NET (project)   | Role                                                     |
|-----------------------|------------------|----------------------------------------------------------|
| `model`               | `Gallery.Model`  | JPA entities → EF Core entity classes                    |
| `bl`                  | `Gallery.Bl`     | Services, DbContext, search logic, internal BL models    |
| `app`                 | `Gallery.App`    | Controllers, DTOs, multipart handling, composition root  |

## Prerequisites

1. **.NET 10 SDK** — https://dotnet.microsoft.com/download
2. **Docker** (for Postgres + pgAdmin)
3. **VS Code** with the **C# Dev Kit** extension (auto-suggested on open)
4. **EF Core CLI tools** — one-time install:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

## Run it

```bash
# 1. Start Postgres + pgAdmin (same as the Spring repo's docker-compose)
docker compose up -d

# 2. Create the initial EF Core migration (only the first time)
dotnet ef migrations add Initial \
  --project Gallery.Bl \
  --startup-project Gallery.App

# 3. Run the API. Migrations apply automatically on startup (see Program.cs).
dotnet run --project Gallery.App
```

Backend listens on **`http://localhost:8080`** — the same port Spring uses, so the frontend's
`src/proxy.conf.json` (`/api → localhost:8080`) works unchanged. Swagger UI at
`http://localhost:8080/api/swagger`.

Then in the frontend repo:

```bash
npm install
npm start    # ng serve with proxy
```

## VS Code workflow

- **F5** → builds and launches with debugger attached.
- **Ctrl/Cmd+Shift+B** → runs the `build` task.
- **Ctrl/Cmd+Shift+P → "Tasks: Run Task" → "ef-migrations-add"** → adds a new migration interactively.

## API endpoints (identical contract to Spring backend)

| Method | Path                  | Purpose                                                          |
|--------|-----------------------|------------------------------------------------------------------|
| GET    | `/api/greeting?name=` | Hello-world endpoint                                             |
| POST   | `/api/image/upload`   | Multipart upload: `dto` (JSON blob) + `imageFile` (binary)       |
| POST   | `/api/image/search`   | Paged search; returns the Spring `Page<T>` JSON shape            |
| GET    | `/api/image/{id}`     | Returns image bytes + tags                                       |
| POST   | `/api/image/update`   | Multipart update: `dto` + optional `imageFile`                   |
| DELETE | `/api/image/{id}`     | Delete by id                                                     |

## Spring → .NET concept map (this project)

| Spring concept                              | .NET concept here                                                |
|---------------------------------------------|------------------------------------------------------------------|
| `@SpringBootApplication` + `application.properties` | `Program.cs` + `appsettings.json`                        |
| `@ComponentScan` / annotations              | Explicit registration in `Gallery.Bl/DependencyInjection.cs`     |
| `@RestController` + `@RequestMapping`       | `[ApiController]` + `[Route("...")]`                             |
| `@RequestMapping("/api")` context path      | `app.UsePathBase("/api")` in `Program.cs`                        |
| `@Autowired` / constructor injection        | Constructor injection (no attribute needed)                      |
| `@Service`, `@Repository`                   | `services.AddScoped<IFoo, Foo>()` in DI extension                |
| `@Entity`, `@Table`, `@Column`              | `[Table]`, `[Column]` data annotations                           |
| `JpaRepository<T, ID>`                      | `DbSet<T>` on `DbContext`                                        |
| JPA Criteria API (`ImageSpecification`)     | LINQ + `EF.Functions.ILike` (see `ImageService.SearchAsync`)     |
| `Spring Data Page<T>`                       | `SpringPage<T>` DTO (shape-compatible for the frontend)          |
| `@RequestPart("dto") + MultipartFile`       | `MultipartReader.ReadImageRequestAsync` (manual, explicit)       |
| `@Valid` / `@AssertTrue`                    | `[Required]`, `[Range]`, plus `IsValid()` for XOR rules          |
| Lombok `@Data`/`@Builder`                   | `record` types + auto-properties                                 |
| Flyway / Hibernate `ddl-auto`               | EF Core Migrations (`dotnet ef migrations add ...`)              |
| `imgscalr` (Java image lib)                 | `SixLabors.ImageSharp`                                           |
| swagger-ui Spring starter                   | `Swashbuckle.AspNetCore`                                         |

## Files most worth reading first

1. **`Gallery.App/Program.cs`** — the explicit composition that replaces Spring's auto-config magic.
2. **`Gallery.Bl/Services/ImageService.cs`** — see `SearchAsync`; this is ~40 lines of LINQ doing what `CustomImageRepositoryImpl` + `ImageSpecification` do in ~300 lines of JPA Criteria.
3. **`Gallery.Bl/DependencyInjection.cs`** — what `@Service`/`@Repository` annotations are doing behind the scenes in Spring.
4. **`Gallery.App/Mappers/MultipartReader.cs`** — the one spot ASP.NET is *less* ergonomic than Spring (no auto-deserialize of JSON multipart parts).

## Known shortcuts taken (worth fixing as you learn)

- **No global exception handler.** Spring has `@ControllerAdvice`; in ASP.NET you'd add an `IExceptionHandler` or `UseExceptionHandler` middleware.
- **Auto-migrate on startup.** Convenient for learning, not what you'd do in production.
- **Validation is partly manual.** A more idiomatic next step is FluentValidation, or move per-action checks into `IValidatableObject` / model binders.
- **No tests.** Add a `Gallery.Tests` project with xUnit + `WebApplicationFactory<Program>` for integration tests — same role as Spring's `@SpringBootTest`.
