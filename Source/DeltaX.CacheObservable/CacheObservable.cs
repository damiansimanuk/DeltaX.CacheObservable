namespace DeltaX.CacheObservable
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class CacheObservable<T, K> : ICacheObservable<T> where K : notnull
    {
        private ObservableCollection<List<DataTracker<T>>> cache;
        private Func<T, K> keySelector;
        private System.Timers.Timer timerAutoClean;
        private TimeSpan liveTime;


        public CacheObservable(Func<T, K> keySelector)
        {
            this.keySelector = keySelector;
            this.cache = new ObservableCollection<List<DataTracker<T>>>();
        }

        public TimeSpan LiveTime
        {
            get { return liveTime; }
            set
            {
                liveTime = value;
                StartTimerAutoClean();
            }
        }

        public int Count
        {
            get { return cache.Count; }
        }

        public void Clear()
        {
            cache.Clear();
        }

        private void StartTimerAutoClean()
        {
            timerAutoClean ??= new System.Timers.Timer();
            timerAutoClean.Elapsed -= TimerAutoClean_Elapsed;
            timerAutoClean.Elapsed += TimerAutoClean_Elapsed;
            timerAutoClean.Interval = LiveTime.TotalMilliseconds / 10;
            timerAutoClean.Enabled = true;
            timerAutoClean.AutoReset = true;
        }

        private void TimerAutoClean_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var since = DateTime.Now - LiveTime;
            RemoveAll(t => t.Updated < since);
        }

        protected void AddCache(IEnumerable<T> items, DataTrackerChange action = DataTrackerChange.Add)
        {
            lock (cache)
            {
                cache.Add(items.Select(item => new DataTracker<T>(item, action)).ToList());
            }
        }

        protected IEnumerable<DataTracker<T>> RemoveAll(Func<DataTracker<T>, bool> match)
        {
            List<DataTracker<T>> removed = new List<DataTracker<T>>();
            lock (cache)
            {
                foreach (var itms in cache.ToList())
                {
                    lock (itms)
                    {
                        var toRemove = itms.Where(match);
                        if (toRemove.Any())
                        {
                            removed.AddRange(toRemove);
                            itms.RemoveAll(e => match(e));
                        }
                        if (!itms.Any())
                        {
                            cache.Remove(itms);
                        }
                    }
                }
            }
            return removed;
        }

        protected IEnumerable<T> RemoveAll(IEnumerable<T> items)
        {
            var toDelete = items.Select(item => keySelector(item)).ToArray();
            return RemoveAll(i => toDelete.Contains(keySelector(i.Entity)))
                .Select(r => r.Entity);
        }

        public void Add(T item)
        {
            Add(new[] { item });
        }

        public void Add(IEnumerable<T> items)
        {
            RemoveAll(items);
            AddCache(items, DataTrackerChange.Add);
        }

        public void Remove(T item, bool force = false)
        {
            Remove(new[] { item }, force);
        }

        public IEnumerable<T> Remove(IEnumerable<T> items, bool force = false)
        {
            var removed = RemoveAll(items);
            if (force)
            {
                AddCache(items, DataTrackerChange.Remove);
            }
            else if (removed.Any())
            {
                AddCache(removed, DataTrackerChange.Remove);
            }
            return removed;
        }

        public void Update(T item, bool force = false)
        {
            Update(new[] { item }, force);
        }

        public IEnumerable<T> Update(IEnumerable<T> items, bool force = false)
        {
            var removed = RemoveAll(items);
            if (force)
            {
                AddCache(items, DataTrackerChange.Update);
            }
            else if (removed.Any())
            {
                var keysRemoved = removed.Select(item => keySelector(item)).ToArray();
                AddCache(items.Where(item => keysRemoved.Contains(keySelector(item))), DataTrackerChange.Update);
            }
            return removed;
        }

        public Task<List<DataTracker<T>>> WaitResultsAsunc(
            Func<DataTracker<T>, bool> filter,
            TimeSpan? timeout = null,
            CancellationToken? token = null)
        {
            token ??= CancellationToken.None;

            return Task.Run(() =>
            {
                var resetEvent = new ManualResetEventSlim();
                List<DataTracker<T>> results;
                lock (cache)
                {
                    results = cache.SelectMany(e => e.Where(filter)).ToList();
                }
                results ??= new List<DataTracker<T>>();

                if (results.Any())
                {
                    return results;
                }

                void Cache_CollectionChanged(object s, NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (List<DataTracker<T>> itms in e.NewItems)
                        {
                            lock (itms) if (itms.Any(filter))
                                {
                                    results.AddRange(itms.Where(filter));
                                }
                        }

                        if (results.Any())
                        {
                            resetEvent.Set();
                        }
                    }
                }

                cache.CollectionChanged += Cache_CollectionChanged;
                if (timeout.HasValue)
                {
                    resetEvent.Wait(timeout.Value, token.Value);
                }
                else
                {
                    resetEvent.Wait(token.Value);
                }
                cache.CollectionChanged -= Cache_CollectionChanged;

                return results;
            }, token.Value);
        }
    }
}