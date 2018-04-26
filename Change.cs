namespace VersionDb
{
    public class Change<T>
    {
        public Change(ChangeType changeType, string id, T value)
        {
            ChangeType = changeType;
            Id = id;
            Value = value;
        }

        public ChangeType ChangeType { get; }
        public string Id { get; }
        public T Value { get; }
    }
}
