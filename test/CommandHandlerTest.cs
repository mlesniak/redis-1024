using System.Text;

using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Model;
using Lesniak.Redis.Storage;

using static Xunit.Assert;

namespace Lesniak.Redis.Test;

public class CommandHandlerTest
{
    private readonly Database _database;
    private readonly CommandHandler _sut;

    // Will be recreated every time we instantiate
    // a new test.
    public CommandHandlerTest()
    {
        _database = new Database(new DefaultDateTimeProvider());
        _sut = new(_database);
    }

    [Fact]
    public void Execute_InvalidCommand_ReturnsError()
    {
        var result = _sut.Execute(ToCommandLine("not-a-command"));

        Equal("-UNKNOWN COMMAND\r\n", Encoding.ASCII.GetString(result));
    }
    
    [Fact]
    public void Execute_SetCommand_Succeeds()
    {
        var result = _sut.Execute(ToCommandLine("set name value"));

        Equal("+OK\r\n", Encoding.ASCII.GetString(result));
        Equal("value"u8.ToArray(), _database.Get("name"));
    }
    
    [Fact]
    public void Execute_GetCommand_Succeeds()
    {
        _database.Set("name", "value"u8.ToArray(), null);

        var result = _sut.Execute(ToCommandLine("get name"));

        Equal("$5\r\nvalue\r\n"u8.ToArray(), result);
    }
    
    private static RedisArray ToCommandLine(string s) =>
        RedisArray.From(s.Split(" ").Select(elem => RedisString.From(elem)).ToArray());
}