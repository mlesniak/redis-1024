using System.Text;

using Lesniak.Redis.Communication.Network;
using Lesniak.Redis.Communication.Network.Types;
using Lesniak.Redis.Core;
using Lesniak.Redis.Test.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Communication.Network;

public class ClientHandlerTest
{
    private ClientContext _ctx;

    private Database _database;

    // Note that we are pragmatic here and override the field in case we want to
    // define a specific test configuration. For our current uses cases, this is
    // sufficient.
    private ClientHandler _sut;

    public ClientHandlerTest()
    {
        SetupSUT();
    }

    private byte[] CreateCommand(string command)
    {
        RedisValue[] elements = command
            .Split(" ")
            .Select(RedisBulkString.From)
            .ToArray();
        return RedisArray.From(elements).Serialize();
    }

    private void SetupSUT(string? password = null)
    {
        var configuration = new TestConfiguration() { Password = password };
        var clock = new TestClock();
        _database = new Database(TestLogger<Database>.Get(), configuration, clock);
        _ctx = new ClientContext();
        _sut = new ClientHandler(TestLogger<ClientHandler>.Get(), _database);
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

    [Fact]
    public void Unknown_commands_returns_error()
    {
        var response = _sut.Handle(_ctx, CreateCommand("xyzzy"));
        Equal("-UNKNOWN COMMAND\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Auth_on_unset_password_succeeds()
    {
        var response = _sut.Handle(_ctx, CreateCommand("auth anything-goes"));
        Equal("+OK\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Auth_on_set_password_with_correct_password_succeeds()
    {
        SetupSUT("password");

        var response = _sut.Handle(_ctx, CreateCommand("auth password"));
        Equal("+OK\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Auth_on_wrong_password_fails()
    {
        SetupSUT("password");

        var response = _sut.Handle(_ctx, CreateCommand("auth something else"));
        Equal("-invalid password\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Auth_without_enough_arguments_returns_error()
    {
        var response = _sut.Handle(_ctx, CreateCommand("auth"));
        Equal("-Not enough arguments\r\n"u8.ToArray(), response);
    }

    [Fact]
    public void Set_with_enabled_password_fails_without_authentication()
    {
        SetupSUT("password");

        var response = _sut.Handle(_ctx, CreateCommand("set key value"));
        Equal("-Authentication needed. Use AUTH command\r\n"u8.ToArray(), response);
    }
    
    [Fact]
    public void Set_with_enabled_password_succeeds_after_authentication()
    {
        SetupSUT("password");
        _sut.Handle(_ctx, CreateCommand("auth password"));

        var response = _sut.Handle(_ctx, CreateCommand("set key value"));
        Equal("+OK\r\n"u8.ToArray(), response);
    }
}
