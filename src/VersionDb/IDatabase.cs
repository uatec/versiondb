using System.Collections.Generic;
using VersionDb.Client;

namespace VersionDb
{
    // TODO : Async
    public interface IDatabase<T>
    {
        T Get(string id);
        void Put(string id, T value);
        void Delete(string id);
        IEnumerable<Change<T>> Watch();
        IEnumerable<Change<T>> Watch(string id);
        IEnumerable<T> GetAll();
    }
}
