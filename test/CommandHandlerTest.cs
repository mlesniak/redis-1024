using System.Text;

namespace Lesniak.Redis.Test;

public class CommandHandlerTest
{
    private readonly CommandHandler sut;

    public CommandHandlerTest()
    {
        sut = new(new Memory());
    }

    [Fact]
    public void Execute_InvalidCommand_ReturnsError()
    {
        var commandLine = ToCommandLine("not-a-command");

        var result = sut.Execute(commandLine);

        Assert.Equal("-UNKNOWN COMMAND\r\n", Encoding.ASCII.GetString(result));
    }

    // TODO(mlesniak) write more tests... 
    
    private static RedisData ToCommandLine(string s) =>
        RedisData.of(s.Split(" ").Select(elem => RedisData.of(elem)).ToArray());
}