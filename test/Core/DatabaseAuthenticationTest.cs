using Lesniak.Redis.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Core;

public class DatabaseAuthenticationTest
{
    private readonly TestConfiguration _configuration = new();
    private readonly TestClock _clock;
    private IDatabase _sut;

    public DatabaseAuthenticationTest()
    {
        _clock = new TestClock();
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _clock);
    }

    [Fact]
    public void ByDefault_NoPassword_Required()
    {
        False(_sut.AuthenticationRequired);
    }

    [Fact]
    public void If_PasswordSet_AuthenticationRequired()
    {
        // The password can only  be set when we initialize a
        // new database (since it's read from a configuration
        // file anyway). Therefore we need to manually create
        // new _sut instances outside the constructor.
        _configuration.Password = "foo";
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _clock);
        True(_sut.AuthenticationRequired);
    }

    [Fact]
    public void If_PasswordSet_ErrorOnWrongPassword()
    {
        _configuration.Password = "foo";
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _clock);

        False(_sut.VerifyPassword("wrong-password"));
    }

    [Fact]
    public void If_PasswordSet_VerifyCorrectPassword()
    {
        _configuration.Password = "foo";
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _clock);

        True(_sut.VerifyPassword("foo"));
    }

    [Fact]
    public void If_PasswordNotSet_VerifyIsAlwaysTrue()
    {
        True(_sut.VerifyPassword("anything-goes"));
    }
}