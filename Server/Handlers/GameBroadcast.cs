using System.Text;
using System.Text.Json;

namespace Server;

public static class GameBroadcast
{
    public static async Task BroadcastGameState(ServerContext context)
    {
        foreach (var kvp in context.Players)
        {
            var socket = kvp.Key;
            var player = kvp.Value;

            var currentPlayer = context.Game.Players[context.Game.CurrentPlayerIndex];
            var state = new
            {
                Players = context.Game.Players.Select(p => new { p.Id, p.Name, p.IsAlive, CardsCount = p.Hand.Count }).ToList(),
                Moves = context.Game.MoveHistory.Select(m => new { m.PlayerId, m.DeclaredNominal, m.DeclaredCount }).ToList(),
                CurrentPlayerId = currentPlayer.Id, // Отправляем ID текущего игрока
                CurrentPlayerName = currentPlayer.Name, // Отправляем имя текущего игрока
                YourHand = player.Hand.Select(c => new { Type = c.Type }).ToList()
            };

            var json = JsonSerializer.Serialize(state);
            var payload = Encoding.UTF8.GetBytes(json);

            await socket.SendCommand(GameCommand.GameState, payload);
        }
    }

    public static async Task BroadcastGameOver(ServerContext context)
    {
        var winner = context.Game.CheckWinner();
        var result = new { WinnerId = winner?.Id, WinnerName = winner?.Name };

        var json = JsonSerializer.Serialize(result);
        var payload = Encoding.UTF8.GetBytes(json);

        foreach (var socket in context.Players.Keys)
        {
            await socket.SendCommand(GameCommand.GameOver, payload);
        }
    }
}