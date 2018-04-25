namespace VersionDb
{
    public class VersionedDocument
    {
        public string Version { get; set; }
        public object Document { get; set; }
    }
}
