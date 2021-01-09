namespace DeltaX.CacheObservable
{
    using System;

    public class DataTracker<TEntity>
    {
        public TEntity Entity;
        public DateTimeOffset Updated;
        public DataTrackerChange Action;

        public DataTracker(TEntity entity, DataTrackerChange action = DataTrackerChange.Add)
        {
            Updated = new DateTimeOffset(DateTime.Now);
            Entity = entity;
            Action = action;
        }
    }
}