using System.Net.Sockets;

TcpListener server = new(6379);
server.Start();
Console.WriteLine("Server started");
while (true)
{
    TcpClient client = server.AcceptTcpClient();
    Console.WriteLine("Client connected");
    var stream = client.GetStream();

    byte[] buffer = new byte[1024];
    int read = stream.Read(buffer);
    foreach (byte b in buffer.Take(read))
    {
        Console.Write((char)b);
    }
    Console.WriteLine();
    client.Close();
}