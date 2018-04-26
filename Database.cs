using System.Collections.Concurrent;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VersionDb
{
    // TODO: Data Store IOC
    public static class Database<T>
    {
        private static string ToDataRecord(T o)
        {
            return JsonConvert.SerializeObject(o);
        }

        private static T FromDataRecord(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        static BlockingCollection<Change<T>> changes = new BlockingCollection<Change<T>>();
        static Dictionary<string, string> data = new Dictionary<string, string>();

        public static T Get(string id)
        {
            return FromDataRecord(data[id]);
        }

        public static void Put(string id, T value)
        {
            ChangeType changeType = ChangeType.Create;

            if ( data.ContainsKey(id) )
            {
                changeType = ChangeType.Update;
            }

            data[id] = ToDataRecord(value);
            changes.Add(new Change<T>(changeType, id, value));
        }

        public static T Delete(string id)
        {
            var value = FromDataRecord(data[id]);
            data.Remove(id);
            changes.Add(new Change<T>(ChangeType.Delete, id, value));            
            return value;
        }

        public static IEnumerable<Change<T>> Watch(string id)
        {
            while ( true )
            {
                var next = changes.Take();
                if ( next.Id == id ) 
                {
                    yield return next;
                }
            }
        }
    }
}
