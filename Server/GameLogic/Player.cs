namespace SemestrovkaSockets;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Card> Hand { get; set; } = new List<Card>();
    public bool IsAlive { get; set; } = true;
    public bool Connected { get; set; } = true;
}