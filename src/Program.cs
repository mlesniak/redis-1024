using System.Net.Sockets;

using Lesniak.Redis;

new RedisServer().Start();

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