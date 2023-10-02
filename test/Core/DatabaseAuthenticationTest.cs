using Lesniak.Redis.Core;

namespace Lesniak.Redis.Test.Core;

public class DatabaseAuthenticationTest
{
    private readonly TestConfiguration _configuration = new();
    private readonly TestDateTimeProvider _dateTimeProvider;
    private Database _sut;

    public DatabaseAuthenticationTest()
    {
        _dateTimeProvider = new TestDateTimeProvider();
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _dateTimeProvider);
    }

    [Fact]
    public void ByDefault_NoPassword_Required()
    {
        Assert.False(_sut.AuthenticationRequired);
    }

    [Fact]
    public void If_PasswordSet_AuthenticationRequired()
    {
        // The password can only  be set when we initialize a
        // new database (since it's read from a configuration
        // file anyway). Therefore we need to manually create
        // new _sut instances outside the constructor.
        _configuration.Password = "foo";
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _dateTimeProvider);
        Assert.True(_sut.AuthenticationRequired);
    }
}