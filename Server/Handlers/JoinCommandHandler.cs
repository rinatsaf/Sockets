using System.Net.Sockets;
using System.Text;
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

        var name = payload != null && payload.Length > 0
            ? Encoding.UTF8.GetString(payload)
            : $"Player{context.NextPlayerId + 1}";

        var player = new Player
        {
            Id = context.NextPlayerId++,
            Name = name,
            IsAlive = true,
            Connected = true
        };

        context.Players.TryAdd(sender, player);
        context.PlayerSockets.TryAdd(player.Id, sender);

        if (!context.Game.Players.Any(p => p.Id == player.Id))
        {
            context.Game.Players.Add(player);
        }

        if (context.Players.Count >= 2 && !context.Game.RoundInProgress)
        {
            context.Game.StartRound();
            await GameBroadcast.BroadcastGameState(context);
        }
    }
}
