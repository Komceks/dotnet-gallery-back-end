using Gallery.Model;
using Microsoft.AspNetCore.Mvc;

namespace Gallery.App.Controllers;

// [ApiController] adds automatic model validation, auto-400 on validation errors,
// and binding-source inference. Closest Spring analog: @RestController.
// [Route("greeting")] is your @RequestMapping("greeting"). The "/api" prefix
// is added globally via app.UsePathBase("/api") in Program.cs.
[ApiController]
[Route("greeting")]
public class GreetingController : ControllerBase
{
    private const string Template = "Hello, {0}!";
    private static readonly string Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
    private static long _counter;

    [HttpGet]
    public Greeting Greet([FromQuery] string name = "World")
    {
        var id = Interlocked.Increment(ref _counter);  // thread-safe equivalent of AtomicLong
        return new Greeting(id, string.Format(Template, name), Timestamp);
    }
}
