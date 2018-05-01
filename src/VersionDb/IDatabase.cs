using System.Collections.Generic;

namespace VersionDb
{
    // TODO : Async
    public interface IDatabase<T>
    {
        T Get(string id);
        void Put(string id, T value);
        void Delete(string id);
        IEnumerable<Change<T>> Watch(string id);
    }
}
