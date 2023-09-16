using System.Net.Sockets;

namespace Lesniak.Redis.Communication.Network;

public abstract class NetworkUtils
{
    public static async Task<byte[]?> ReadAsync(NetworkStream networkStream, int maxBuffer)
    {
        const int bufferSize = 1024;

        byte[] buffer = new byte[bufferSize];
        using MemoryStream memoryStream = new();

        int bytesRead;
        while ((bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            memoryStream.Write(buffer, 0, bytesRead);
            if (!networkStream.DataAvailable)
            {
                break;
            }

            // Prevent memory-flooding by simulating an EOF
            // if a client tries to send too much data. The
            // connection will be closed. This is the same
            // behavior as implemented by the standard redis
            // server.
            if (memoryStream.Length >= maxBuffer)
            {
                return null;
            }
        }

        if (memoryStream.Length == 0)
        {
            // Client send EOF.
            return null;
        }

        byte[] input = memoryStream.ToArray();
        return input;
    }
}