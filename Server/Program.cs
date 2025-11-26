using Server;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new GameServer(8080);
        await server.StartAsync();
    }
}