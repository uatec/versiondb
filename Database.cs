using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Newtonsoft.Json;

namespace VersionDb
{
    // TODO: Data Store IOC
    public class Database<T>
    {
        private static string ToDataRecord(T o)
        {
            return JsonConvert.SerializeObject(o);
        }

        private static T FromDataRecord(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        Subject<Change<T>> changes = new Subject<Change<T>>();
        Dictionary<string, string> data = new Dictionary<string, string>();

        public T Get(string id)
        {
            return FromDataRecord(data[id]);
        }

        public void Put(string id, T value)
        {
            ChangeType changeType = ChangeType.Create;

            if ( data.ContainsKey(id) )
            {
                changeType = ChangeType.Update;
            }

            data[id] = ToDataRecord(value);
            changes.OnNext(new Change<T>(changeType, id, value));
        }

        public T Delete(string id)
        {
            var value = FromDataRecord(data[id]);
            data.Remove(id);
            changes.OnNext(new Change<T>(ChangeType.Delete, id, value));            
            return value;
        }

        public IEnumerable<Change<T>> Watch(string id)
        {
            foreach ( var next in changes.Next() )
            {
                if ( next.Id == id ) 
                {
                    yield return next;
                }
            }
        }
    }
}
