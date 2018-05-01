using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EtcdNet;
using Newtonsoft.Json;

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

        public EtcDatabase()
        {
            etcdClient = new EtcdClient(new EtcdClientOpitions {
                Urls = new [] { "http://127.0.0.1:12379/" }
            });
        }
        public void Delete(string id)
        {
            etcdClient.DeleteNodeAsync($"/{id}").Wait();
        }

        public T Get(string id)
        {
            return FromDataRecord(etcdClient.GetNodeValueAsync($"/{id}", ignoreKeyNotFoundException: true).Result);
        }

        public void Put(string id, T value)
        {
            etcdClient.SetNodeAsync($"/{id}", ToDataRecord(value)).Wait();
        }

        public IEnumerable<Change<T>> Watch(string id)
        {
            string key = $"/{id}";
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
                            T value = resp.Node.Value != null ? FromDataRecord(resp.Node.Value) : default(T);

                            switch(resp.Action.ToLowerInvariant())
                            {
                                case EtcdResponse.ACTION_DELETE:
                                    nextChange = new Change<T>(ChangeType.Delete, id, value);
                                    break;
                                case EtcdResponse.ACTION_EXPIRE:
                                    nextChange = new Change<T>(ChangeType.Delete, id, value);                             
                                    break;
                                case EtcdResponse.ACTION_COMPARE_AND_DELETE:
                                    nextChange = new Change<T>(ChangeType.Delete, id, default(T));                                
                                    break;

                                case EtcdResponse.ACTION_SET:
                                    nextChange = new Change<T>(ChangeType.Update, id, value);                                
                                    break;
                                case EtcdResponse.ACTION_CREATE:
                                    nextChange = new Change<T>(ChangeType.Create, id, value);                               
                                    break;
                                case EtcdResponse.ACTION_COMPARE_AND_SWAP:
                                    nextChange = new Change<T>(ChangeType.Update, id, value);                                
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

                continue;
                // if (!_running) return;
                // something went wrong, delay 1 second and try again
                // await Task.Delay(1000);
                Thread.Sleep(1000);
            }

        }
    }
}
