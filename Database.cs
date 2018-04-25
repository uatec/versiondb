using System.Collections.Generic;

namespace VersionDb
{
    // TODO: Data Store IOC
    public static class Database<T>
    {
        static Dictionary<string, T> data = new Dictionary<string, T>();

        public static T Get(string id)
        {
            return data[id];
        }

        public static void Put(string id, T document)
        {
            data[id] = document;
        }

        // TODO: Delete functionality
    }
}
