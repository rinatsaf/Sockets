using System.Net.Sockets;

namespace Server;

[Command(GameCommand.AccuseLie)]
public class AccuseLieCommandHandler : ICommandHandler
{
    public async Task Invoke(Socket sender, ServerContext context, byte[]? payload = null, CancellationToken ct = default)
    {
        var accuserId = context.Players[sender].Id;

        context.Game.AccuseOfLying(accuserId);

        if (context.Game.EndGameIfWinner())
        {
            await GameBroadcast.BroadcastGameOver(context);
        }
        else
        {
            await GameBroadcast.BroadcastGameState(context);
        }
    }
}

