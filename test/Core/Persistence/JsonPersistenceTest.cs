using Lesniak.Redis.Core;
using Lesniak.Redis.Core.Persistence;

using static Xunit.Assert;

namespace Lesniak.Redis.Test.Core.Persistence;

public class JsonPersistenceTest : IDisposable
{
    readonly JsonPersistence _sut;
    readonly string _databaseName = Path.GetRandomFileName();
    private Database _database;
    private TestClock _clock;

    public JsonPersistenceTest()
    {
        TestConfiguration configuration = new() { DatabaseName = _databaseName };
        _clock = new("2023-10-01 12:34:56");
        _database = new(TestLogger<Database>.Get(), configuration, _clock);
        _sut = new JsonPersistence(TestLogger<JsonPersistence>.Get(), configuration, _clock, _database);
    }

    [Fact]
    public void Storing_Keys_Creates_Correct_File()
    {
        _database.Set("key1", new byte[]
        {
            1
        });
        _database.Set("key2", new byte[]
        {
            2
        }, TimeSpan.FromMilliseconds(1_000));

        _sut.Save();

        string json = File.ReadAllText(_databaseName);
        // Keys can be stored in any order, we have two choices, account for that.
        string[] expected =
        {
            """{"key1":{"Value":"AQ==","ExpirationDate":null},"key2":{"Value":"Ag==","ExpirationDate":"2023-10-01T12:34:57"}}""",
            """{"key2":{"Value":"Ag==","ExpirationDate":"2023-10-01T12:34:57"},"key1":{"Value":"AQ==","ExpirationDate":null}}"""
        };
        Contains(json, expected);
    }

    [Fact]
    public void Loading_Keys_Create_Correct_Database_Entries()
    {
        var json =
            """{"key1":{"Value":"AQ==","ExpirationDate":null},"key2":{"Value":"Ag==","ExpirationDate":"2023-10-01T12:34:57"}}""";
        File.WriteAllText(_databaseName, json);

        _sut.Load();

        // Let 1000ms pass to make key2 disappear,
        // thus checking that we correctly loaded
        // expiration date as well.
        _clock.Add(1000);

        Equal(new byte[]
        {
            1
        }, _database.Get("key1"));
        Null(_database.Get("key2"));
    }

    [Fact]
    public void Loading_Not_Existing_File_Throws_No_Error()
    {
        // We expect nothing to happen. The temporary files
        // does not exist anyway.
        _sut.Load();
    }

    public void Dispose()
    {
        File.Delete(_databaseName);
    }
}
