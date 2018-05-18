using System.Collections.Generic;

namespace VersionDb.Client
{
    public interface IVersionDbClient<T>
    {
        IEnumerable<T> GetAll();
        T Get(string id);
        void Post(string id, T value);
        void Delete(string id);
        IEnumerable<Change<T>> Watch(string id);
        IEnumerable<Change<T>> Watch();
    }
}
