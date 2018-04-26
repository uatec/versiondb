using System.IO;

namespace VersionDb
{
    public static class StreamExtensions
    {
        public static string ReadAllText(this Stream stream)
        {
            using ( var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
