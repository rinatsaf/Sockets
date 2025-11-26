namespace SemestrovkaSockets;

public class Move
{
    public int PlayerId { get; set; }
    public List<Card> CardsPlayed { get; set; } = new List<Card>();
    public string DeclaredNominal { get; set; }
    public int DeclaredCount { get; set; }
}