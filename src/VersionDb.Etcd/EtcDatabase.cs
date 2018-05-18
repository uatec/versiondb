using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EtcdNet;
using Newtonsoft.Json;
using VersionDb.Client;

namespace VersionDb.Etcd
{
    public class EtcDatabase<T> : IDatabase<T>
    {
        private static string ToDataRecord(T o)
        {
            return JsonConvert.SerializeObject(o);
        }

        private static T FromDataRecord(string s)
        {
            return JsonConvert.DeserializeObject<T>(s);
        }

        private readonly EtcdClient etcdClient;
        private readonly string typeName;

        public EtcDatabase(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("message", nameof(typeName));
            }

            this.typeName = typeName;

            etcdClient = new EtcdClient(new EtcdClientOpitions {
                Urls = new [] { "http://127.0.0.1:12379/" }
            });
        }
        
        public void Delete(string id)
        {
            etcdClient.DeleteNodeAsync(CreateKey(id)).Wait();
        }

        public T Get(string id)
        {
            return FromDataRecord(etcdClient.GetNodeValueAsync(CreateKey(id), ignoreKeyNotFoundException: true).Result);
        }

        public void Put(string id, T value)
        {
            etcdClient.SetNodeAsync(CreateKey(id), ToDataRecord(value)).Wait();
        }

        private string CreateKey(string id) => $"/{typeName}/{id}";
        private string ParseKey(string key) => key.Split('/').Last();

        public IEnumerable<Change<T>> Watch(string id)
        {
            string key = CreateKey(id);
            long? waitIndex = null;
            EtcdResponse resp;
            Change<T> nextChange = null;
            while (true) // TODO: Cancellation token
            {
                // try
                // {
                    // when waitIndex is null, get it from the ModifiedIndex
                    if( !waitIndex.HasValue )
                    {
                        resp = etcdClient.GetNodeAsync( key, ignoreKeyNotFoundException: true, recursive: true).Result;
                        if( resp != null && resp.Node != null )
                        {
                            waitIndex = resp.Node.ModifiedIndex + 1;

                            // and also check the children
                            if( resp.Node.Nodes != null )
                            {
                                foreach( var child in resp.Node.Nodes )
                                {
                                    if (child.ModifiedIndex >= waitIndex.Value)
                                        waitIndex = child.ModifiedIndex + 1;

                                    // child node
                                }
                            }
                        }
                    }

                    // watch the changes
                    resp = etcdClient.WatchNodeAsync(key, recursive: true, waitIndex: waitIndex).Result;
                    if (resp != null && resp.Node != null)
                    {
                        waitIndex = resp.Node.ModifiedIndex + 1;

                        if (resp.Node.Key.StartsWith(key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            string nodeId = ParseKey(resp.Node.Key);
                            T value = resp.Node.Value != null ? FromDataRecord(resp.Node.Value) : default(T);

                            switch(resp.Action.ToLowerInvariant())
                            {
                                case EtcdResponse.ACTION_DELETE:
                                    nextChange = new Change<T>(ChangeType.Delete, nodeId, value);
                                    break;
                                case EtcdResponse.ACTION_EXPIRE:
                                    nextChange = new Change<T>(ChangeType.Delete, nodeId, value);                             
                                    break;
                                case EtcdResponse.ACTION_COMPARE_AND_DELETE:
                                    nextChange = new Change<T>(ChangeType.Delete, nodeId, default(T));                                
                                    break;

                                case EtcdResponse.ACTION_SET:
                                    nextChange = new Change<T>(ChangeType.Update, nodeId, value);                                
                                    break;
                                case EtcdResponse.ACTION_CREATE:
                                    nextChange = new Change<T>(ChangeType.Create, nodeId, value);                               
                                    break;
                                case EtcdResponse.ACTION_COMPARE_AND_SWAP:
                                    nextChange = new Change<T>(ChangeType.Update, nodeId, value);                                
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                // }
                // catch(TaskCanceledException)
                // {
                //     // time out, try again
                // }
                // catch(EtcdException ee)
                // {
                //     // reset the waitIndex
                //     waitIndex = null;
                // }
                // catch (EtcdGenericException ege)
                // {
                //     // etcd returns an error
                // }
                // catch (Exception ex)
                // {
                //     // generic error
                // }
                
                if ( nextChange != null )
                {
                    yield return nextChange;
                    nextChange = null;
                }
            }
        }

        public IEnumerable<Change<T>> Watch()
        {
            return Watch(null);
        }

        public IEnumerable<T> GetAll()
        {
            return 
                etcdClient
                    .GetNodeAsync($"/{typeName}")
                    .Result
                    .Node
                    .Nodes
                    .Select(n => FromDataRecord(n.Value));
        }
    }
}
