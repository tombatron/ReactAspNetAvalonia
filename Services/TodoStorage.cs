using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReactAspNetAvalonia.Services;

public class TodoStorage : ITodoStorage
{
    private int _id = 0;
    private readonly ConcurrentDictionary<int, Todo> _store = new();
    
    public Task<List<Todo>> GetAll()
    {
        return Task.FromResult(_store.Values.ToList());
    }

    public Task<Todo> Insert(Todo todo)
    {
        var newTodo = todo with
        {
            Id = ++_id,
        };

        _store[newTodo.Id] = newTodo;

        return Task.FromResult(newTodo);
    }

    public Task Update(Todo todo)
    {
        Delete(todo.Id);

        _store[todo.Id] = todo;
        
        return Task.CompletedTask;
    }

    public Task Delete(int id)
    {
        _store.Remove(id, out _);
        
        return Task.CompletedTask;
    }
}