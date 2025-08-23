using Microsoft.AspNetCore.Mvc;

namespace ReactAspNetAvalonia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new { message = "Hello from in-process API!" });
    }
}