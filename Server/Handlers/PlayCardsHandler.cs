using System.Net.Sockets;
using SemestrovkaSockets;

namespace Server;

[Command(GameCommand.PlayCards)]
public class PlayCardsHandler : ICommandHandler
{
    public async Task Invoke(Socket sender, ServerContext context, byte[]? payload = null, CancellationToken ct = default)
    {
        if (payload == null || payload.Length < 2) return;
        
        var player = context.Players[sender];
        var count = payload[0];
        var cards = new List<Card>();
        var offset = 0;

        for (int i = 0; i < count; i++)
        {
            if (offset > payload.Length - 1) break;
            cards.Add(new Card
            {
                Type = (CardType)payload[offset++]
            });
        }
        
        if (offset >=  payload.Length) return;
        
        var declaredNominal = ((CardType)payload[offset]).ToString();
        var declaredCount = count;
        
        context.Game.PlayTurn(player.Id, cards, declaredNominal, declaredCount);

        await GameBroadcast.BroadcastGameState(context);
    }
}