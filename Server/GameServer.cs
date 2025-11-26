using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using SemestrovkaSockets;

namespace Server;

public class GameServer
{
    private readonly TcpListener _listener;
    private readonly ServerContext _context = new ServerContext();
    private readonly ConcurrentDictionary<GameCommand, ICommandHandler> _handlers;

    public GameServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
        _handlers = LoadHandlers();
    }
    
    public async Task StartAsync(CancellationToken ct = default)
    {
        _listener.Start();
        Console.WriteLine("Сервер запущен на порту 8080...");

        while (!ct.IsCancellationRequested)
        {
            var client = await _listener.AcceptSocketAsync();
            _ = Task.Run(() => HandleClient(client, ct), ct);
        }
    }
    
    private ConcurrentDictionary<GameCommand, ICommandHandler> LoadHandlers()
    {
        var handlers = new ConcurrentDictionary<GameCommand, ICommandHandler>();
        var types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && typeof(ICommandHandler)
            .IsAssignableFrom(t));

        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<CommandAttribute>();
            if (attr != null)
            {
                var handler = (ICommandHandler)Activator.CreateInstance(type);
                handlers.TryAdd(attr.Command, handler);
            }
        }
        return handlers;
    }
    
    private async Task HandleClient(Socket client, CancellationToken ct)
    {
        var buffer = new byte[1024];

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var size = await client.ReceiveAsync(buffer, SocketFlags.None);
                if (size == 0) break;

                var command = (GameCommand)buffer[0];
                var payload = size > 1 ? buffer[1..size] : null;

                if (_handlers.TryGetValue(command, out var handler))
                {
                    await handler.Invoke(client, _context, payload, ct);
                }
                else
                {
                    await client.SendCommand(GameCommand.Error, new byte[] { 0xFF }); // Неизвестная команда
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки клиента: {ex.Message}");
                break;
            }
        }
        
        if (_context.Players.ContainsKey(client))
        {
            await new LeaveCommandHandler().Invoke(client, _context, null, ct);
        }
    }
}