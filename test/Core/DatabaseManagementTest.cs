using Lesniak.Redis.Core;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Core;

public class DatabaseManagementTest
{
    private readonly IDatabaseManagement _sut;

    public DatabaseManagementTest()
    {
        TestConfiguration configuration = new();
        TestClock clock = new();
        _sut = new Database(TestLogger<Database>.Get(), configuration, clock);
    }

    [Fact]
    public void Clear_RemovesAllEntries()
    {
        _sut.Set("demo-1", new byte[]
        {
            1,
            2,
            3
        });
        Single(_sut);

        _sut.Clear();
        Empty(_sut);
    }
}