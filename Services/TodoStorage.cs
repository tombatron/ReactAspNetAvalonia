using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ReactAspNetAvalonia.EventHubs;

namespace ReactAspNetAvalonia.Services;

public class TodoStorage(IHubContext<TodoStatusesHub> statusHubContext) : ITodoStorage
{
    private int _id = 0;
    private readonly ConcurrentDictionary<int, Todo> _store = new();
    
    public async Task<List<Todo>> GetAll()
    {
        await statusHubContext.Clients.All.SendAsync("ReceiveStatus", "Returning all messages.");
        
        return _store.Values.ToList();
    }

    public async Task<Todo> Insert(Todo todo)
    {
        var newTodo = todo with
        {
            Id = ++_id,
        };

        _store[newTodo.Id] = newTodo;
        
        await statusHubContext.Clients.All.SendAsync("ReceiveStatus", $"Inserted Todo #{newTodo.Id}");

        return newTodo;
    }

    public async Task Update(Todo todo)
    {
        await Delete(todo.Id);

        _store[todo.Id] = todo;
        
        await statusHubContext.Clients.All.SendAsync("ReceiveStatus", $"Updated Todo #{todo.Id}.");
    }

    public async Task Delete(int id)
    {
        _store.Remove(id, out _);
        
        await statusHubContext.Clients.All.SendAsync("ReceiveStatus", $"Deleted Todo #{id}");
    }
}