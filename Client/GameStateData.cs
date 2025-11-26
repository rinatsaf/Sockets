using SemestrovkaSockets;

namespace Client;

public class GameStateData
{
    public List<PlayerData> Players { get; set; } = new List<PlayerData>();
    public List<MoveData> Moves { get; set; } = new List<MoveData>();
    public int CurrentPlayerId { get; set; } 
    public string CurrentPlayerName { get; set; } = "";
    public List<CardData> YourHand { get; set; } = new List<CardData>();
}

public class CardData
{
    public CardType Type { get; set; }
}