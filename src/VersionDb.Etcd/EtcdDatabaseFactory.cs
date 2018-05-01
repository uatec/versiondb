namespace VersionDb.Etcd
{
    public class EtcdDatabaseFactory : IDatabaseFactory
    {
        public IDatabase<T> Build<T>(string typeName)
        {
            return new EtcDatabase<T>(typeName);
        }
    }
}
