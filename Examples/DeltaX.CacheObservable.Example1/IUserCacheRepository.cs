using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeltaX.CacheObservable.Example1
{
    public interface IUserCacheRepository
    {
        Task<List<DataTracker<UserDto>>> WaitResultsAsunc(Func<DataTracker<UserDto>, bool> filter, TimeSpan? timeout = null, CancellationToken? token = null);
    }
}
