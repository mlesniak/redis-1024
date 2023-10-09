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
    public void By_Default_No_Password_Required()
    {
        False(_sut.AuthenticationRequired);
    }

    [Fact]
    public void If_Password_Set_Authentication_Required()
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
    public void If_Password_Set_Error_On_Wrong_Password()
    {
        _configuration.Password = "foo";
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _clock);

        False(_sut.VerifyPassword("wrong-password"));
    }

    [Fact]
    public void If_Password_Set_Verify_Correct_Password()
    {
        _configuration.Password = "foo";
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _clock);

        True(_sut.VerifyPassword("foo"));
    }

    [Fact]
    public void If_Password_Not_Set_Verify_Is_Always_True()
    {
        True(_sut.VerifyPassword("anything-goes"));
    }
}
