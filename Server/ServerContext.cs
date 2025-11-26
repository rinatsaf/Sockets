using System.Collections.Concurrent;
using System.Net.Sockets;
using SemestrovkaSockets;

namespace Server;

public class ServerContext
{
    public Game Game { get; set; } = new Game();
    public ConcurrentDictionary<Socket, Player> Players { get; set; } = new ConcurrentDictionary<Socket, Player>();
    public ConcurrentDictionary<int, Socket> PlayerSockets { get; set; } = new ConcurrentDictionary<int, Socket>();
    public int NextPlayerId { get; set; } = 0;
}