using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeltaX.CacheObservable
{
    public interface ICacheObservable<T>
    {
        TimeSpan LiveTime { get; set; }
        int Count { get; }
        void Add(IEnumerable<T> items);
        void Add(T item);
        void Clear();
        IEnumerable<T> Remove(IEnumerable<T> items, bool force = false);
        void Remove(T item, bool force = false);
        IEnumerable<T> Update(IEnumerable<T> items, bool force = false);
        void Update(T item, bool force = false);
        Task<List<DataTracker<T>>> WaitResultsAsunc(Func<DataTracker<T>, bool> filter, TimeSpan? timeout = null, CancellationToken? token = null);
    }
}