namespace VersionDb
{
    public interface IDatabaseFactory
    {
        IDatabase<T> Build<T>(string typeName);
    }
}
