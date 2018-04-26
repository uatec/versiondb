using System.Collections.Generic;

namespace VersionDb
{
    public interface IDatabase<T>
    {
        T Get(string id);
        void Put(string id, T value);
        T Delete(string id);
        IEnumerable<Change<T>> Watch(string id);
    }
}
