using System.Text;

using Lesniak.Redis.Communication.Network;
using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Core;
using Lesniak.Redis.Test.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network;

public class ClientHandlerTest
{
    private readonly ClientHandler _sut;
    private ClientContext _ctx;
    private Database _database;

    public ClientHandlerTest()
    {
        var configuration = new TestConfiguration();
        var clock = new TestClock();
        _database = new Database(TestLogger<Database>.Get(), configuration, clock);
        _ctx = new ClientContext();
        _sut = new ClientHandler(TestLogger<ClientHandler>.Get(), _database);
    }

    private byte[] CreateCommand(string command)
    {
        RedisValue[] elements = command
            .Split(" ")
            .Select(RedisBulkString.From)
            .ToArray();
        return RedisArray.From(elements).Serialize();
    }

    [Fact]
    public void Echo_returns_send_string()
    {
        var response = _sut.Handle(_ctx, CreateCommand("ECHO Hello World!"));
        Equal("$5\r\nHello\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Echo_without_arguments_returns_error()
    {
        // Assumption: superfluous spaces are trimmed by the client
        // and is nothing we have to handle.
        var response = _sut.Handle(_ctx, CreateCommand("ECHO"));
        Equal("-Not enough arguments\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Set_is_parsed_correctly()
    {
        var response = _sut.Handle(_ctx, CreateCommand("set foo bar"));
        Equal("+OK\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Set_with_expiration_in_seconds_is_parsed_correctly()
    {
        var response = _sut.Handle(_ctx, CreateCommand("set foo bar ex 1000"));
        Equal("+OK\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Set_with_expiration_in_milliseconds_is_parsed_correctly()
    {
        var response = _sut.Handle(_ctx, CreateCommand("set foo bar px 1000"));
        Equal("+OK\r\n"u8.ToArray(), response);
    }


    [Fact]
    public void Set_without_enough_arguments_returns_error()
    {
        // Assumption: superfluous spaces are trimmed by the client
        // and is nothing we have to handle.
        var response = _sut.Handle(_ctx, CreateCommand("set foo bar ex"));
        Equal("-Not enough arguments\r\n"u8.ToArray(), response);
    }
    
    [Fact]
    public void Set_with_invalid_timespan_argument_returns_error()
    {
        // Assumption: superfluous spaces are trimmed by the client
        // and is nothing we have to handle.
        var response = _sut.Handle(_ctx, CreateCommand("set foo bar ?? 1000"));
        Equal("-Invalid expiration type\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Get_without_enough_arguments_returns_error()
    {
        var response = _sut.Handle(_ctx, CreateCommand("get"));
        Equal("-Not enough arguments\r\n"u8.ToArray(), response);
    }
    
    [Fact]
    public void Get_returns_stored_value()
    {
        _database.Set("foo", "bar"u8.ToArray());

        var response = _sut.Handle(_ctx, CreateCommand("get foo"));
        Equal("$3\r\nbar\r\n"u8.ToArray(), response);
    }
}
