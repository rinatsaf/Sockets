using System.Net.Sockets;
using SemestrovkaSockets;

namespace Server;

[Command(GameCommand.Join)]
public class JoinCommandHandler : ICommandHandler
{
    public async Task Invoke(Socket sender, ServerContext context, byte[]? payload = null, CancellationToken ct = default)
    {
        if (context.Players.Count >= 4)
        {
            await sender.SendCommand(GameCommand.Error, new byte[] { 0x01 });
            return;
        }
        
        var player = new Player
        {
            Id = context.NextPlayerId++,
            Name = $"Player{context.NextPlayerId}",
            IsAlive = true,
            Connected = true
        };
        
        context.Players.TryAdd(sender, player);
        context.PlayerSockets.TryAdd(player.Id, sender);
        
        if (context.Players.Count == 2)
        {
            context.Game.Players = context.Players.Values.ToList();
            context.Game.StartRound();
            await GameBroadcast.BroadcastGameState(context);
        }
    }
}