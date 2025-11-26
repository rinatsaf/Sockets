using System.Net.Sockets;

namespace Server;

[Command(GameCommand.Leave)]
public class LeaveCommandHandler : ICommandHandler
{
    public async Task Invoke(
        Socket sender,
        ServerContext context,
        byte[]? payload = null,
        CancellationToken ct = default)
    {
        if (!context.Players.TryGetValue(sender, out var player))
        {
            await sender.SendCommand(GameCommand.Error, new byte[] { 0x02 }); // Игрок не найден
            return;
        }

        player.IsAlive = false; 
        player.Connected = false;

        context.Players.TryRemove(sender, out _);
        context.PlayerSockets.TryRemove(player.Id, out _);
        
        if (context.Game.EndGameIfWinner())
        {
            await GameBroadcast.BroadcastGameOver(context);
        }
        else
        {
            await GameBroadcast.BroadcastGameState(context);
        }

        sender.Shutdown(SocketShutdown.Both);
        sender.Close();
    }
}