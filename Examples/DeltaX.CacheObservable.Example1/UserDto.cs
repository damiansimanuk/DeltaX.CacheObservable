namespace DeltaX.CacheObservable.Example1
{
    public class UserDto
    {
        public int Id;
        public int Age;
        public string Name;

        public override string ToString()
        {
            return $"{nameof(UserDto)}=> Id:{Id} Name:{Name}, Age:{Age}";
        }
    }
}
