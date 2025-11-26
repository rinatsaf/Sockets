using System.Net.Sockets;

namespace Server;

public static class SocketExtensions
{
    public static async Task SendCommand(this Socket socket, GameCommand command, byte[]? payload = null)
    {
        var data = new byte[1 + (payload?.Length ?? 0)];
        data[0] = (byte)command;
        if (payload != null)
            Buffer.BlockCopy(payload, 0, data, 1, payload.Length);

        await socket.SendAsync(data, SocketFlags.None);
    }
}