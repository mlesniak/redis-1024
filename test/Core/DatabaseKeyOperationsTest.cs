using Lesniak.Redis.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Core;

public class DatabaseKeyOperationsTest
{
    private readonly IConfiguration _configuration = new TestConfiguration();
    private readonly TestClock _clock;
    private readonly IDatabase _sut;

    public DatabaseKeyOperationsTest()
    {
        _clock = new TestClock();
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _clock);
    }

    [Fact]
    public void Set_Defines_Key()
    {
        _sut.Set("key", new byte[]
        {
            1,
            2,
            3
        });

        Equal(new byte[]
        {
            1,
            2,
            3
        }, _sut.Get("key"));
    }

    [Fact]
    public void Set_For_Expiration_Date_In_Far_Future_Returns_Key()
    {
        _sut.Set("key", new byte[]
        {
            1,
            2,
            3
        }, TimeSpan.FromMilliseconds(1_000));
        // Less than 1000ms have passed, key is available.
        _clock.Add(999);
        Equal(new byte[]
        {
            1,
            2,
            3
        }, _sut.Get("key"));
    }

    [Fact]
    public void Set_For_Expiration_Date_Passed_Returns_Null()
    {
        _sut.Set("key", new byte[]
        {
            1,
            2,
            3
        }, TimeSpan.FromMilliseconds(1_000));
        // 1000ms have passed, key is not available any more.
        _clock.Add(1_000);
        Null(_sut.Get("key"));
    }

    [Fact]
    public void Get_Returns_Null_On_Undefined_Key()
    {
        Null(_sut.Get("key"));
    }

    [Fact]
    public void Remove_Does_Nothing_For_Non_Existing_Key()
    {
        _sut.Remove("not-existing");
    }

    [Fact]
    public void Remove_Removes_Existing_Key()
    {
        _sut.Set("key", new byte[]
        {
            1,
            2,
            3
        });
        _sut.Remove("key");
        Null(_sut.Get("key"));
    }
}
