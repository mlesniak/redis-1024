using System.Collections;

namespace Lesniak.Redis.Core;

public interface IDatabaseManagement: IDatabase
{
    event DatabaseUpdated DatabaseUpdates;
    void LockWrites(Action operation);
    IEnumerator GetEnumerator();
}