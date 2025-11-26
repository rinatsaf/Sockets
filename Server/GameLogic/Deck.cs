namespace SemestrovkaSockets;

public static class DeckCreator
{
    public static List<Card> CreateDeck()
    {
        var deck = new List<Card>();
        var types = new[] { CardType.Ten, CardType.Jack, CardType.Queen, CardType.King, CardType.Ace };
        foreach (var t in types)
            for (int i = 0; i < 4; i++)
                deck.Add(new Card { Type = t });
        for (int i = 0; i < 4; i++)
            deck.Add(new Card { Type = CardType.Joker });
        return deck;
    }
}