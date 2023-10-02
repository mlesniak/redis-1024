using Lesniak.Redis.Core;

namespace Lesniak.Redis.Test.Core;

public class DatabaseKeyOperationsTest
{
    private readonly IConfiguration _configuration = new TestConfiguration();
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly Database _sut;

    public DatabaseKeyOperationsTest()
    {
        _dateTimeProvider = new TestDateTimeProvider();
        _sut = new Database(_configuration, _dateTimeProvider);
    }

    [Fact]
    public void Set_Defines_Key()
    {
        _sut.Set("key", new byte[]{1, 2, 3});

        Assert.Equal(new byte[] {1, 2, 3}, _sut.Get("key"));
    }

    [Fact]
    public void Set_ForExpirationDateInFarFuture_ReturnsKey()
    {
        _sut.Set("key", new byte[]{1, 2, 3}, 1000);
        // Less than 1000ms have passed, key is available.
        _dateTimeProvider.Add(999);
        Assert.Equal(new byte[] {1, 2, 3}, _sut.Get("key"));
    }
    
    [Fact]
    public void Set_ForExpirationDatePassed_ReturnsNull()
    {
        _sut.Set("key", new byte[]{1, 2, 3}, 1000);
        // 1000ms have passed, key is not available any more.
        _dateTimeProvider.Add(1_000);
        Assert.Null(_sut.Get("key"));
    }

    
    [Fact]
    public void Get_ReturnsNull_OnUndefinedKey()
    {
        Assert.Null(_sut.Get("key"));
    }
}