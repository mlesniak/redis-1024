using Lesniak.Redis.Core.Storage;
using Lesniak.Redis.Test.Utils;

namespace Lesniak.Redis.Test;

public class DatabaseTest
{
    private readonly byte[] _dummy = "dummy"u8.ToArray();
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly InMemoryStorage _sut;

    // TODO(mlesniak) Fix this.
    // public DatabaseTest()
    // {
    //     _dateTimeProvider = new TestDateTimeProvider();
    //     _sut = new InMemoryStorage(_dateTimeProvider);
    // }

    [Fact]
    public void Set_WithoutExpiration_NeverExpires()
    {
        _sut.Set("key", _dummy, null);

        // Let 1 years pass.
        _dateTimeProvider.AddMilliseconds(31556926000);

        Assert.Equal(_dummy, _sut.Get("key"));
    }

    [Fact]
    public void Set_WithExpirationDateAndAccessBefore_ReturnValue()
    {
        _sut.Set("key", _dummy, 1_000);

        // Let 500ms pass.
        _dateTimeProvider.AddMilliseconds(500);

        Assert.Equal(_dummy, _sut.Get("key"));
    }

    [Fact]
    public void Set_WithExpirationDateAndAccessAfter_ReturnNil()
    {
        _sut.Set("key", _dummy, 1_000);

        // Let 5000ms pass.
        _dateTimeProvider.AddMilliseconds(5000);

        Assert.Null(_sut.Get("key"));
    }

    // TODO(mlesniak) Add separate test for MemoryCleanupJob 
    // [Fact]
    // public void Cleanup_Called_RemovesExpiredItems()
    // {
    //     _sut.Set("key-not-removed", _dummy, null);
    //     _sut.Set("key", _dummy, 1_000);
    //     _dateTimeProvider.AddMilliseconds(5000);
    //
    //     _sut.RemoveExpiredKeys();
    //
    //     Assert.Equal(1, _sut.Count);
    // }
}