using System.Net.Sockets;
using System.Text;

TcpListener server = new(6379);
server.Start();
Console.WriteLine("Server started");
while (true)
{
    TcpClient client = server.AcceptTcpClient();
    Console.WriteLine("Client connected");
    var stream = client.GetStream();

    Console.WriteLine("Answering client");
    while (true)
    {
        byte[] buffer = new byte[1024];
        int read = stream.Read(buffer);
        foreach (byte b in buffer.Take(read))
        {
            Console.Write((char)b);
        }

        Console.WriteLine();

        var responseBytes = "+OK\r\n"u8.ToArray();
        stream.Write(responseBytes, 0, responseBytes.Length);
    }
    client.Close();
}


// var client = new TcpClient("localhost", 6379);
// using (NetworkStream stream = client.GetStream())
// {
//     // Initial message sent by the reference client.
//     var message = "*2$7COMMAND$4DOCS";
//     var bytes = Encoding.ASCII.GetBytes(message);
//
//     stream.Write(bytes, 0, bytes.Length);
//     Console.WriteLine("Message sent");
//
//     var buffer = new byte[1024];
//     int read = stream.Read(buffer, 0, buffer.Length);
//     var response = Encoding.ASCII.GetString(buffer, 0, read);
//     Console.WriteLine($"Received: '{response}'");
// }