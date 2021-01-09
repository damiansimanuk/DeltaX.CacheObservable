using System;
using NUnit.Framework;

namespace DeltaX.CacheObservable.UnitTest
{
    public class UnitTest1
    {
        [SetUp]
        public void Setup()
        {
        }

        class User
        {
            public int Id;
            public int Age;
            public string Name;
        }

        [Test]
        public void test_match_all_added_values()
        {
            var cache = new CacheObservable<User, int>(u => u.Id);

            var now = DateTime.Now;
            cache.Add(new User { Id = 1, Name = "User 1", Age = 21 });            
            cache.Add(new User { Id = 2, Name = "User 2", Age = 22 });

            var result = cache.WaitResultsAsunc(u => u.Updated > now).Result;

            Assert.AreEqual(result.Count, 2);
        }

        [Test]
        public void test_match_by_updated()
        {
            var cache = new CacheObservable<User, int>(u => u.Id);
             
            cache.Add(new User { Id = 1, Name = "User 1", Age = 21 });
            
            var now = DateTime.Now;
            cache.Add(new User { Id = 2, Name = "User 2", Age = 22 });
            var result = cache.WaitResultsAsunc(u => u.Updated > now).Result;
            Assert.AreEqual(result.Count, 1);
        }

        [Test]
        public void test_match_by_updated_without_result()
        {
            var cache = new CacheObservable<User, int>(u => u.Id);

            cache.Add(new User { Id = 1, Name = "User 1", Age = 21 });            
            cache.Add(new User { Id = 2, Name = "User 2", Age = 22 });

            var now = DateTime.Now;
            var result = cache.WaitResultsAsunc(u => u.Updated > now);
            var notResult = result.Wait(1);

            Assert.AreEqual(notResult, false);
            Assert.AreEqual(result.Result.Count, 0);
        }


        [Test]
        public void test_match_by_updated_wait_result()
        {
            var cache = new CacheObservable<User, int>(u => u.Id);

            cache.Add(new User { Id = 1, Name = "User 1", Age = 21 });
            cache.Add(new User { Id = 2, Name = "User 2", Age = 22 });

            var now = DateTime.Now;
            var result = cache.WaitResultsAsunc(u => u.Updated > now);
            
            cache.Add(new User { Id = 3, Name = "User 3", Age = 23 });
            var hasResult = result.Wait(1);

            Assert.AreEqual(hasResult, true);
            Assert.AreEqual(result.Result.Count, 1);
        }

        [Test]
        public void test_match_by_removed()
        {
            var cache = new CacheObservable<User, int>(u => u.Id);
            var now = DateTime.Now;

            cache.Add(new User { Id = 1, Name = "User 1", Age = 21 });
            cache.Add(new User { Id = 2, Name = "User 2", Age = 22 });
             
            var result = cache.WaitResultsAsunc(u => u.Action == DataTrackerChange.Remove);

            cache.Remove(new User { Id = 1});
            var hasResult = result.Wait(1);

            Assert.AreEqual(hasResult, true);
            Assert.AreEqual(result.Result.Count, 1);
            Assert.AreEqual(result.Result[0].Entity.Id, 1);
            Assert.AreEqual(result.Result[0].Action, DataTrackerChange.Remove);
        }
    }
}
