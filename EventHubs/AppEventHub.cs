using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ReactAspNetAvalonia.EventHubs;

public class AppEventHub : Hub
{
    public async Task SendMessage(string user, string message) => 
        await Clients.All.SendAsync("ReceiveMessage", user, message);
}