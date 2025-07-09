
using Microsoft.AspNetCore.SignalR.Client;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5192/hub")
            .WithAutomaticReconnect()
            .Build();

        connection.On<string>("ReceberPing", from =>
        {
            Console.WriteLine($"{from} {DateTime.Now.ToLongTimeString()}");
        });

        await connection.StartAsync();
        Console.WriteLine("Conectado ao hub");

        while (true)
        {
            await connection.InvokeAsync("EnviarPing", "ConsoleApp");
            await Task.Delay(5000);
        }
    }
}