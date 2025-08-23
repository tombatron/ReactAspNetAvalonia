using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReactAspNetAvalonia.Services;

public interface ITodoStorage
{
    Task<List<Todo>> GetAll();
    Task<Todo> Insert(Todo todo);
    Task Update(Todo todo);
    Task Delete(int id);
}