using System;
using System.Threading.Tasks;

namespace DeltaX.CacheObservable.Example1
{

    class TestApplication
    {
        IUserRepository repository;
        IUserCacheRepository cache;


        public TestApplication()
        {
            // Configured with dependency injection...
            var cacheRepository = new UserCacheRepository();
            repository = cacheRepository;
            cache = cacheRepository;
        }



        public Task TestGetsync()
        {
            return Task.Run(() =>
            {
                var now = DateTime.Now;
                var result = cache.WaitResultsAsunc(u => u.Updated > now && u.Action == DataTrackerChange.Add);
                Console.WriteLine("TestInsert Result.Count:{0}", result.Result.Count);
                Console.WriteLine("TestInsert Result.Count:{0}", result.Result[0].Entity);
            });
        }


        public async Task TestInsertAsync()
        {
            await Task.Delay(2000);
            repository.Insert(new UserDto { Name = "User 21", Age = 21 });
            repository.Insert(new UserDto { Name = "User 22", Age = 22 });
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            TestApplication testApplication = new TestApplication();

            var t1 = testApplication.TestGetsync();
            var t2 = testApplication.TestInsertAsync();

            Task.WaitAll(t1, t2);

            Console.WriteLine($"t1:{t1}, t2:{t2}");
        }
    }
}
