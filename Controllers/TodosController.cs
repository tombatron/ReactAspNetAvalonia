using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ReactAspNetAvalonia.Services;

namespace ReactAspNetAvalonia.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodosController(ITodoStorage todoStorage) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var results = await todoStorage.GetAll();
        
        return Ok(results);
    }

    [HttpPost] // Create a todo.
    public async Task<IActionResult> Create([FromBody] Todo todo)
    {
        var result = await todoStorage.Insert(todo);
        return Ok(result);
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var todo = (await todoStorage.GetAll()).FirstOrDefault(x => x.Id == id);

        if (todo is null)
        {
            return NotFound();
        }

        var toggledTodo = todo with
        {
            Completed = !todo.Completed
        };

        await todoStorage.Update(toggledTodo);

        return Ok(toggledTodo);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await todoStorage.Delete(id);
        
        return NoContent();
    }
}