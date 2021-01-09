using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DeltaX.CacheObservable.Example1
{
    public class UserCacheRepository : UserRepository, IUserCacheRepository, IUserRepository
    {
        private CacheObservable<UserDto, int> cache;

        public UserCacheRepository() : base()
        {
            cache = new CacheObservable<UserDto, int>(u => u.Id);
            cache.Add(base.GetAll());
        }

        public new UserDto Get(int id)
        {
            return base.Get(id);
        }

        public new UserDto Insert(UserDto item)
        {
            var inserted = base.Insert(item);
            cache.Add(inserted);
            return inserted;
        }

        public new UserDto Remove(int id)
        {
            var item = base.Remove(id);
            if (item != null)
            {
                cache.Remove(item);
            }
            return item;
        }

        public new UserDto Update(UserDto item)
        {
            var updated = base.Update(item);
            cache.Update(updated);
            return updated; throw new NotImplementedException();
        }

        public Task<List<DataTracker<UserDto>>> WaitResultsAsunc(
          Func<DataTracker<UserDto>, bool> filter,
          TimeSpan? timeout = null,
          CancellationToken? token = null)
        {
            return cache.WaitResultsAsunc(filter, timeout, token);
        }
    }
}
