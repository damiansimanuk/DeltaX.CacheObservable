using System;
using System.Collections.Generic;
using System.Linq;

namespace DeltaX.CacheObservable.Example1
{
    public class UserRepository : IUserRepository
    {
        List<UserDto> db;
        private static int identity = 1;

        public UserRepository()
        {
            db = new List<UserDto>();

            // Dumy data
            Insert(new UserDto { Age = 11, Name = "User 1" });
            Insert(new UserDto { Age = 12, Name = "User 2" });
        }

        public UserDto Get(int id)
        {
            return db.FirstOrDefault(i => i.Id == id);
        }

        public IEnumerable<UserDto> GetAll()
        {
            return db.ToArray();
        }

        public UserDto Insert(UserDto item)
        {
            var clone = new UserDto
            {
                Id = identity++,
                Name = item.Name,
                Age = item.Age
            };

            db.Add(clone);
            return clone;
        }

        public UserDto Remove(int id)
        {
            var item = Get(id);
            if (item != null)
            {
                db.Remove(item);
            }
            return item;
        }

        public UserDto Update(UserDto item)
        { 
            var prevItem = Remove(item.Id);
            if (prevItem != null)
            {
                var clone = new UserDto
                {
                    Id = prevItem.Id,
                    Name = item.Name,
                    Age = item.Age
                };

                db.Add(clone);
                return clone;
            }
            return null;
        }
    }
}
