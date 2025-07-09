using Microsoft.AspNetCore.SignalR;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSignalR();

        var app = builder.Build();

        app.MapHub<PingPongHub>("/hub");

        app.Run();
    }
}

class PingPongHub : Hub
{
    public async Task EnviarPing(string from)
    {
        Console.WriteLine($"{from} {DateTime.Now.ToLongTimeString()}");

        await Clients.All.SendAsync("ReceberPing", $"Server");
    }

}
