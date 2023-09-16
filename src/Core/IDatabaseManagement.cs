using System.Collections;

namespace Lesniak.Redis.Core;

public interface IDatabaseManagement: IDatabase,  IEnumerable<KeyValuePair<string, Database.DatabaseValue>>
{
    event DatabaseUpdated DatabaseUpdates;
    void Clear();
}