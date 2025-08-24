using Microsoft.AspNetCore.Mvc;

namespace ReactAspNetAvalonia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppShellController : Controller
{
    public class NewWindowRequest
    {
        public string ViewName { get; set; } = "";
    }
    
    [HttpPost]
    [Route("new-window")]
    public IActionResult NewWindow([FromBody] NewWindowRequest request)
    {
        BrowserWindowLauncher.OpenBrowserWindow(request.ViewName);
        return Ok();
    }
}