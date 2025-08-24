using Microsoft.AspNetCore.Mvc;

namespace ReactAspNetAvalonia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppShellController : Controller
{
    public class NewWindowRequest
    {
        public string FileName { get; set; } = "";
    }
    
    [HttpPost]
    [Route("new-window")]
    public IActionResult NewWindow([FromBody] NewWindowRequest request)
    {
        BrowserWindowLauncher.OpenBrowserWindow(request.FileName);
        return Ok();
    }
}