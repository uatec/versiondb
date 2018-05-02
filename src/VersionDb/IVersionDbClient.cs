using System.Collections.Generic;

namespace VersionDb
{
    public interface IVersionDbClient<T>
    {
        T Get(string id);
        void Put(string id, T value);
        void Delete(string id);
        IEnumerable<Change<T>> Watch(string id);
    }
}
