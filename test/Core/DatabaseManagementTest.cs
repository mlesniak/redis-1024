using Lesniak.Redis.Core;

namespace Lesniak.Redis.Test.Core;

public class DatabaseManagementTest
{
    
    private readonly TestConfiguration _configuration = new();
    private readonly TestDateTimeProvider _dateTimeProvider;
    private readonly IDatabaseManagement _sut;

    public DatabaseManagementTest()
    {
        _dateTimeProvider = new TestDateTimeProvider();
        _sut = new Database(TestLogger<Database>.Get(), _configuration, _dateTimeProvider);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        _sut.Set("demo-1", new byte[] {1, 2, 3});
        Assert.Single(_sut);

        _sut.Clear();
        Assert.Empty(_sut);
    }
}