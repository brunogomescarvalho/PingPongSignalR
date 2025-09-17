using Microsoft.AspNetCore.SignalR.Client; 

namespace ClientSignalR;

internal class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Informe a quantidade de conexões");
        int totalConnections = Convert.ToInt16(Console.ReadLine());

        var cts = new CancellationTokenSource();

        // cria as conexões
        var connections = CreateConnections(totalConnections);

        // inicia todas as conexões
        await Task.WhenAll(connections.Select(async x =>
        {
            await x.StartAsync(cts.Token);
            WriteLineColor($"[{x.ConnectionId}] Conectado ao hub", ConsoleColor.DarkBlue);
        }));

        WriteLineColor(" - Agora aguarde alguns segundos e em seguida, encerre o servidor OU desconecte a rede.", ConsoleColor.DarkYellow);
        WriteLineColor(" - Observe que as conexões tentam se reconectar automaticamente.", ConsoleColor.DarkYellow);
        WriteLineColor(" - Execute o servidor novamente para que as conexões sejam restabelecidas automaticamente.", ConsoleColor.DarkYellow);
        
        // loop de envio de pings
        var sendPings = connections.Select(async x =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (x.State == HubConnectionState.Connected)
                {
                    await x.InvokeAsync("EnviarPing", "ConsoleApp", cts.Token);
                }
                await Task.Delay(5000, cts.Token);
            }
        });


        // mantém rodando até ENTER
        Console.WriteLine("\nPressione ENTER para encerrar...");
        Console.ReadLine();
        cts.Cancel();

        // encerra tudo 
        await Task.WhenAll(sendPings);
        await Task.WhenAll(connections.Select(c => c.StopAsync()));

        return;


        List<HubConnection> CreateConnections(int totalConnections)
        {
            List<HubConnection> connections = new();

            for (int i = 0; i < totalConnections; i++)
            {
                var index = i;

                var connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5192/hub")
                    .WithAutomaticReconnect(new RetryPolicy())
                    .Build();

                connection.On<string>("ReceberPing", (from) =>
                {
                    //aqui recebe o ping do servidor
                });

                connection.Reconnecting += (erro) =>
                {
                    WriteLineColor($"Conexão {index} Perdida: Motivo {erro?.Message}", ConsoleColor.DarkRed);
                    return Task.CompletedTask;
                };

                connection.Reconnected += (id) =>
                {
                    WriteLineColor($"Conexão {index} restaurada com id {id} às {DateTime.Now:HH:mm:ss}", ConsoleColor.DarkGreen);
                    return Task.CompletedTask;
                };

                connections.Add(connection);
            }

            return connections;
        }

        static void WriteLineColor(string texto, ConsoleColor cor)
        {
            Console.ForegroundColor = cor;
            Console.WriteLine(texto);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}

internal class RetryPolicy : IRetryPolicy
{
    private static readonly Random _random = new();

    public TimeSpan? NextRetryDelay(RetryContext context)
    {
        if (context.PreviousRetryCount >= 5)
            return null;

        var backoff = TimeSpan.FromSeconds(Math.Pow(2, context.PreviousRetryCount));

        var randomSeconds = _random.NextDouble();
        var delaySeconds = TimeSpan.FromSeconds(randomSeconds);

        var nextDelay = backoff + delaySeconds;

        Console.WriteLine($"Next retry delay: {nextDelay}");

        return nextDelay;
    }
}
