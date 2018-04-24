using System.Collections.Generic;

namespace VersionDb
{
    // TODO: Data Store IOC
    public static class Database
    {
        static Dictionary<string, (string, object)> data = new Dictionary<string, (string, object)>();

        public static (string, object) Get(string id)
        {
            return data[id];
        }

        public static void Put(string id, string version, object value)
        {
            data[id] = (version, value);
        }

        // TODO: Delete functionality
    }
}
