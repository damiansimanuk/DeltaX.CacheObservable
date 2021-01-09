using System;
using System.Collections.Generic;

namespace DeltaX.CacheObservable.Example1
{
    public interface IUserRepository
    {
        UserDto Get(int id);
        IEnumerable<UserDto> GetAll();
        UserDto Insert(UserDto item);
        UserDto Remove(int id);
        UserDto Update(UserDto item);
    }
}
