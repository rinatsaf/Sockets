using System.Net.Sockets;

namespace Server;

public interface ICommandHandler
{
    Task Invoke(Socket sender, ServerContext context, byte[]? payload = null, CancellationToken ct = default);
}