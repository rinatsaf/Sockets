class Program
{
    static async Task Main(string[] args)
    {
        var client = new ConsoleGameClient();
        await client.StartAsync("127.0.0.1", 8080);
    }
}