using System;
using System.Linq;
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
            var result = cache.WaitResultsAsunc(u => u.Updated > now, TimeSpan.FromMilliseconds(1));
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

            cache.Remove(new User { Id = 1 });
            var hasResult = result.Wait(5000);

            Assert.AreEqual(hasResult, true);
            Assert.AreEqual(result.Result.Count, 1);
            Assert.AreEqual(result.Result[0].Entity.Id, 1);
            Assert.AreEqual(result.Result[0].Action, DataTrackerChange.Remove);
        }

        [Test]
        public void test_match_by_remove_wait_rremove_not_match()
        {
            var cache = new CacheObservable<User, int>(u => u.Id);

            cache.Add(new User { Id = 1, Name = "User 1", Age = 21 });
            cache.Add(new User { Id = 2, Name = "User 2", Age = 22 });

            var now = DateTime.Now;
            var result = cache.WaitResultsAsunc(u => u.Action == DataTrackerChange.Remove, TimeSpan.FromSeconds(1));

            cache.Remove(new User { Id = 3 });
            var hasResult = result.Wait(1);

            Assert.AreEqual(hasResult, false);
            Assert.AreEqual(result.Result.Count, 0);
        }

        [Test]
        public void test_match_by_remove_batch_remove_force()
        {
            var cache = new CacheObservable<User, int>(u => u.Id);

            cache.Add(new User { Id = 1, Name = "User 1", Age = 21 });
            cache.Add(new User { Id = 2, Name = "User 2", Age = 22 });

            var now = DateTime.Now;
            var result = cache.WaitResultsAsunc(u => u.Action == DataTrackerChange.Remove, TimeSpan.FromSeconds(1));

            cache.Remove(new[] {
                new User { Id = 3 },
                new User { Id = 4 }}, true);

            var hasResult = result.Wait(1);

            Assert.AreEqual(hasResult, true);
            Assert.AreEqual(result.Result.Count, 2);
            
            var resOrdered = result.Result.OrderBy(e=>e.Entity.Id).ToList();

            Assert.AreEqual(resOrdered[0].Entity.Id, 3);
            Assert.AreEqual(resOrdered[1].Entity.Id, 4);
            Assert.AreEqual(resOrdered[0].Action, DataTrackerChange.Remove);
            Assert.AreEqual(resOrdered[1].Action, DataTrackerChange.Remove);
        }


        [Test]
        public void test_match_add_update_remove()
        {
            var cache = new CacheObservable<User, int>(u => u.Id);

            var now = DateTime.Now;

            cache.Add(new User { Id = 1, Name = "User 1", Age = 21 });
            var resultAdd = cache.WaitResultsAsunc(u => u.Updated >now);
            resultAdd.Wait();

            cache.Update(new User { Id = 1, Name = "Usuario 1", Age = 31 });
            var resultUpdate = cache.WaitResultsAsunc(u => u.Updated > now);
            resultUpdate.Wait();

            cache.Remove(new User { Id = 1 });
            var resultRemove = cache.WaitResultsAsunc(u => u.Updated > now);
            resultRemove.Wait();

            Assert.AreEqual(resultAdd.Result.Count, 1);
            Assert.AreEqual(resultUpdate.Result.Count, 1);
            Assert.AreEqual(resultRemove.Result.Count, 1);  

            Assert.AreEqual(resultAdd.Result[0].Action, DataTrackerChange.Add);
            Assert.AreEqual(resultAdd.Result[0].Entity.Id, 1);
            Assert.AreEqual(resultAdd.Result[0].Entity.Name, "User 1");
            
            Assert.AreEqual(resultUpdate.Result[0].Action, DataTrackerChange.Update); 
            Assert.AreEqual(resultUpdate.Result[0].Entity.Id, 1);
            Assert.AreEqual(resultUpdate.Result[0].Entity.Name, "Usuario 1");
            
            Assert.AreEqual(resultRemove.Result[0].Action, DataTrackerChange.Remove); 
            Assert.AreEqual(resultRemove.Result[0].Entity.Id, 1);
            Assert.AreEqual(resultRemove.Result[0].Entity.Name, "Usuario 1");
        }
    }
}
