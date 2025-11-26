namespace SemestrovkaSockets;

public class Game
{
    public List<Player> Players { get; set; } = new List<Player>();
    public List<Card> Deck { get; set; } = new List<Card>();
    public List<Move> MoveHistory { get; set; } = new List<Move>();
    public int CurrentPlayerIndex { get; set; } = 0;

    public void StartRound()
    {
        Deck = DeckCreator.CreateDeck().OrderBy(x => Guid.NewGuid()).ToList();
        DealCards();
        PlaceFirstCardOnTable();
    }

    private void DealCards()
    {
        foreach (var player in Players.Where(p => p.IsAlive))
        {
            for (int i = 0; i < 4; i++)
            {
                if (Deck.Count > 0)
                {
                    player.Hand.Add(Deck[0]);
                    Deck.RemoveAt(0);
                }
            }
        }
    }

    private void PlaceFirstCardOnTable()
    {
        Card card;
        do
        {
            card = Deck[0];
            Deck.RemoveAt(0);
        } while (card.Type == CardType.Joker);

        MoveHistory.Add(new Move
        {
            PlayerId = -1, // Условный "ход стола"
            CardsPlayed = new List<Card> { card },
            DeclaredNominal = card.Type.ToString(),
            DeclaredCount = 1
        });
    }

    public void PlayTurn(int playerId, List<Card> cards, string declaredNominal, int declaredCount)
    {
        var player = Players.First(p => p.Id == playerId);
        
        foreach (var card in cards)
        {
            player.Hand.Remove(card);
        }

        MoveHistory.Add(new Move
        {
            PlayerId = playerId,
            CardsPlayed = cards,
            DeclaredNominal = declaredNominal,
            DeclaredCount = declaredCount
        });

        CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Players.Count(p => p.IsAlive);
    }

    public bool IsLastMoveLie()
    {
        if (MoveHistory.Count == 0) return false;

        var lastMove = MoveHistory[MoveHistory.Count - 1];
        var realCards = lastMove.CardsPlayed.Where(c => c.Type != CardType.Joker).ToList();
        var realCount = realCards.Count(c => c.Type.ToString() == lastMove.DeclaredNominal);
        var jokerCount = lastMove.CardsPlayed.Count(c => c.Type == CardType.Joker);

        return (realCount + jokerCount) < lastMove.DeclaredCount;
    }

    public bool RussianRoulette()
    {
        var rand = new Random();
        return rand.Next(0, 6) != 0;
    }

    public Player? CheckWinner()
    {
        var alivePlayers = Players.Where(p => p.IsAlive).ToList();
        return alivePlayers.Count == 1 ? alivePlayers[0] : null;
    }

    public void AccuseOfLying(int accuserId)
    {
        if (MoveHistory.Count == 0) return;

        var lastMove = MoveHistory[MoveHistory.Count - 1];
        var lastPlayer = Players.First(p => p.Id == lastMove.PlayerId);

        if (IsLastMoveLie())
        {
            if (!RussianRoulette())
            {
                lastPlayer.IsAlive = false;
            }
        }
        else
        {
            var accuser = Players.First(p => p.Id == accuserId);
            if (!RussianRoulette())
            {
                accuser.IsAlive = false;
            }
        }

        StartRound();
    }

    public bool EndGameIfWinner()
    {
        var winner = CheckWinner();
        if (winner != null)
        {
            Console.WriteLine($"Игрок {winner.Name} победил!");
            return true;
        }
        return false;
    }
}