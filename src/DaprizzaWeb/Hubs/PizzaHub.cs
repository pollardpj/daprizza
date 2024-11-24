using DaprizzaModels;
using Microsoft.AspNetCore.SignalR;

namespace DaprizzaWeb.Hubs;

public class PizzaHub : Hub
{
    public const string HubUrl = "/pizzahub";

    public async Task SendOrderStatusUpdate(OrderStatusUpdate orderStatusUpdate)
    {
        await Clients.All.SendAsync("OnOrderStatusUpdate", orderStatusUpdate);
    }

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} connected");
        return base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception e)
    {
        Console.WriteLine($"Disconnected {e?.Message} {Context.ConnectionId}");
        await base.OnDisconnectedAsync(e);
    }
}

