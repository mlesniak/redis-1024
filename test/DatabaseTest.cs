using System.Text;

using Lesniak.Redis.Storage;
using Lesniak.Redis.Test.Utils;

using Xunit.Abstractions;

namespace Lesniak.Redis.Test;

public class DatabaseTest
{
    private readonly byte[] _dummy = "dummy"u8.ToArray();
    private readonly ITestOutputHelper _output;
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly Database _sut;

    public DatabaseTest(ITestOutputHelper output)
    {
        _output = output;
        _dateTimeProvider = new TestDateTimeProvider();
        _sut = new Database(_dateTimeProvider);
    }

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
}