namespace DeltaX.CacheObservable
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class CacheObservable<T, K>
       where K : notnull
    {
        private ObservableCollection<List<DataTracker<T>>> cache;
        private Func<T, K> keySelector;
        private System.Timers.Timer timerAutoClean;
        private TimeSpan liveTime;

        public TimeSpan LiveTime
        {
            get { return liveTime; }
            set
            {
                liveTime = value;
                StartTimerAutoClean();
            }
        }

        public CacheObservable(Func<T, K> keySelector)
        {
            this.keySelector = keySelector;
            this.cache = new ObservableCollection<List<DataTracker<T>>>(); 
        }

        private void StartTimerAutoClean()
        {
            timerAutoClean ??= new System.Timers.Timer();
            timerAutoClean.Elapsed -= TimerAutoClean_Elapsed;
            timerAutoClean.Elapsed += TimerAutoClean_Elapsed;
            timerAutoClean.Interval = LiveTime.TotalMilliseconds / 10;
            timerAutoClean.Enabled = true; 
        }

        private void TimerAutoClean_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var since = DateTime.Now - LiveTime;
            RemoveAll(t => t.Updated < since);
        } 

        protected void AddCache(IEnumerable<T> items, DataTrackerChange action = DataTrackerChange.Add)
        {
            cache.Add(items.Select(item => new DataTracker<T>(item, action)).ToList());
        }

        protected int RemoveAll(Predicate<DataTracker<T>> match)
        {
            var count = 0;
            foreach (var itms in cache.ToList())
            {
                count += itms.RemoveAll(match);
                if (!itms.Any())
                {
                    cache.Remove(itms);
                }
            }
            return count;
        }

        protected int RemoveAll(T item)
        {
            return RemoveAll(i => keySelector(i.Entity)?.Equals(keySelector(item)) == true);
        }

        public void Add(T item)
        {
            RemoveAll(item);
            AddCache(new[] { item }, DataTrackerChange.Add);
        }

        public void Remove(T item, bool force = false)
        {
            var count = RemoveAll(item);
            if (count > 0 || force)
            {
                AddCache(new[] { item }, DataTrackerChange.Remove);
            }
        }

        public void Replace(T item, bool force = false)
        {
            var count = RemoveAll(item);
            if (count > 0 || force)
            {
                AddCache(new[] { item }, DataTrackerChange.Replace);
            }
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
                var results = cache.SelectMany(e => e.Where(filter)).ToList();
                results ??= new List<DataTracker<T>>();

                if (results.Any())
                {
                    return results;
                }

                void Cache_CollectionChanged(object s, NotifyCollectionChangedEventArgs e)
                {
                    if (e.Action == NotifyCollectionChangedAction.Add)
                    {
                        foreach (List<DataTracker<T>> items in e.NewItems)
                        {
                            if (items.Any(filter))
                            {
                                results.AddRange(items.Where(filter));
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