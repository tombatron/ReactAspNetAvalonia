using Microsoft.AspNetCore.Mvc;
using ReactAspNetAvalonia.Services;

namespace ReactAspNetAvalonia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HelloController(ITimeService timeService) : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return Ok(new
        {
            message = "Hello from in-process API!",
            time = timeService.GetEpochTime()
        });
    }
}