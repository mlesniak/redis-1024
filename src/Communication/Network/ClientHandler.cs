using System.Text;

using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Core;
using Lesniak.Redis.Utils;

using Microsoft.Extensions.Logging;

namespace Lesniak.Redis.Communication.Network;

[AttributeUsage(AttributeTargets.Method)]
public class RequiresAuthentication : Attribute
{ }

public class ClientHandler
{
    private readonly ILogger<ClientHandler> _log;
    private readonly IDatabase _database;

    public ClientHandler(ILogger<ClientHandler> log, IDatabase database)
    {
        _log = log;
        _database = database;
    }

    public byte[] Handle(ClientContext ctx, byte[] stream)
    {
        int offset = 0;
        List<byte> responses = new();
        while (offset < stream.Length)
        {
            // Commands are send as serialized arrays.
            (RedisArray commands, int nextOffset) = RedisValue.Deserialize<RedisArray>(stream, offset);
            byte[] singleResponse = ExecuteCommand(ctx, commands);
            responses.AddRange(singleResponse);
            offset = nextOffset;
        }

        return responses.ToArray();
    }

    private byte[] ExecuteCommand(ClientContext ctx, RedisArray commandline)
    {
        IList<RedisValue> parts = commandline.Values!;
        byte[] rs = ((RedisBulkString)parts[0]).Value!;
        string command = Encoding.ASCII.GetString(rs).ToLower();
        List<RedisValue> arguments = parts.Skip(1).ToList();

        Func<ClientContext, List<RedisValue>, RedisValue> method = command switch
        {
            // TODO(mlesniak) add del command
            "auth" => AuthHandler,
            "set" => SetHandler,
            "get" => GetHandler,
            "echo" => EchoHandler,
            "subscribe" => SubscribeHandler,
            "unsubscribe" => UnsubscribeHandler,
            "publish" => PublishHandler,
            _ => UnknownCommandHandler
        };

        object[] attributes = method.Method.GetCustomAttributes(typeof(RequiresAuthentication), false);
        if (_database.AuthenticationRequired && attributes.Length > 0 && !ctx.Authenticated)
        {
            return RedisErrorString.From("Authentication needed. Use AUTH command").Serialize();
        }

        RedisValue result = method.Invoke(ctx, arguments);
        return result.Serialize();
    }

    [RequiresAuthentication]
    private RedisValue UnsubscribeHandler(ClientContext ctx, List<RedisValue> arguments)
    {
        List<string> unsubscribedFrom = new();

        if (arguments.Count > 0)
        {
            foreach (RedisValue ch in arguments)
            {
                string channel = ((RedisBulkString)ch).AsciiValue;
                _database.Unsubscribe(ctx.ClientId, channel);
                unsubscribedFrom.Add(channel);
            }
        }
        else
        {
            unsubscribedFrom.AddRange(_database.UnsubscribeAll(ctx.ClientId));
        }

        RedisBulkString[] response = unsubscribedFrom
            .Select(channel => RedisBulkString.From(channel))
            .Prepend(RedisBulkString.From("unsubscribe"))
            .ToArray();
        return RedisArray.From(response);
    }

    [RequiresAuthentication]
    private RedisValue PublishHandler(ClientContext ctx, List<RedisValue> arguments)
    {
        if (arguments.Count() < 2)
        {
            return RedisErrorString.From("Not enough arguments");
        }

        string channel = ((RedisBulkString)arguments[0]).AsciiValue;
        byte[] message = ((RedisBulkString)arguments[1]).Value!;

        int sendTo = _database.Publish(channel, message);
        return RedisNumber.From(sendTo);
    }

    [RequiresAuthentication]
    private RedisValue SubscribeHandler(ClientContext ctx, List<RedisValue> arguments)
    {
        if (!arguments.Any())
        {
            return RedisErrorString.From("Not enough arguments");
        }

        List<(string, int)> subscriberCounts = new();
        foreach (RedisValue ch in arguments)
        {
            string channel = ((RedisBulkString)ch).AsciiValue;
            int subscribers = _database.Subscribe(ctx.ClientId, channel, ResponseAction);
            subscriberCounts.Add((channel, subscribers));
        }

        RedisValue[] response = subscriberCounts
            .SelectMany(tuple =>
                new RedisValue[]
                {
                    RedisBulkString.From(tuple.Item1),
                    RedisNumber.From(tuple.Item2)
                })
            .Prepend(RedisBulkString.From("subscribe"))
            .ToArray();
        return RedisArray.From(response);

        void ResponseAction(string c, byte[] message)
        {
            _log.LogTrace("Received message on channel {Channel}: {S}", c, Encoding.UTF8.GetString(message));
            byte[] response = RedisArray.From(
                    RedisBulkString.From("message"),
                    RedisBulkString.From(c),
                    RedisBulkString.From(message))
                .Serialize();
            ctx.SendToClient(response);
        }
    }

    private RedisValue EchoHandler(ClientContext ctx, List<RedisValue> arguments)
    {
        if (!arguments.Any())
        {
            return RedisErrorString.From("Not enough arguments");
        }

        byte[] response = ((RedisBulkString)arguments[0]).Value!;
        return RedisBulkString.From(response);
    }

    // Remark: the password is submitted in cleartext. Implementing or adding
    // any form of transport encryption would blow up the whole project, though.
    private RedisValue AuthHandler(ClientContext ctx, List<RedisValue> arguments)
    {
        if (!arguments.Any())
        {
            return RedisErrorString.From("Not enough arguments");
        }

        string password = ((RedisBulkString)arguments[0]).AsciiValue;
        if (_database.VerifyPassword(password))
        {
            ctx.Authenticated = true;
            return RedisSimpleString.From("OK");
        }

        return RedisErrorString.From("invalid password");
    }

    [RequiresAuthentication]
    private RedisValue SetHandler(ClientContext ctx, IReadOnlyList<RedisValue> arguments)
    {
        if (arguments.Count() < 2)
        {
            return RedisErrorString.From("Not enough arguments");
        }

        string setKey = ((RedisBulkString)arguments[0]).AsciiValue;
        byte[] value = ((RedisBulkString)arguments[1]).Value!;

        TimeSpan? expiration = null;
        if (arguments.Count > 2)
        {
            string type = ((RedisBulkString)arguments[2]).AsciiValue;
            int num = Int32.Parse(((RedisBulkString)arguments[3]).AsciiValue);

            int expirationInMs = type.ToLower() switch
            {
                "ex" => num * 1_000,
                "px" => num,
                _ => throw new ArgumentOutOfRangeException()
            };
            expiration = TimeSpan.FromMilliseconds(expirationInMs);
        }

        _database.Set(setKey, value, expiration);
        return RedisSimpleString.From("OK");
    }

    [RequiresAuthentication]
    private RedisValue GetHandler(ClientContext ctx, IReadOnlyList<RedisValue> arguments)
    {
        if (!arguments.Any())
        {
            return RedisErrorString.From("Not enough arguments");
        }

        string getKey = ((RedisBulkString)arguments[0]).AsciiValue;
        byte[]? resultBytes = _database.Get(getKey);
        return resultBytes == null
            ? RedisBulkString.Nil()
            : RedisBulkString.From(resultBytes);
    }

    private RedisValue UnknownCommandHandler(ClientContext ctx, List<RedisValue> arguments)
    {
        string command = "";
        if (arguments.Count > 0)
        {
            command = ((RedisBulkString)arguments[0]).AsciiValue;
        }

        return RedisErrorString.From($"UNKNOWN COMMAND {command}");
    }
}
