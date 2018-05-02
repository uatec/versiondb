using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VersionDb
{
    public class VersionMapper<T>
    {
        private readonly string currentVersion;
        private readonly List<KeyValuePair<string, Type>> versions;

        public VersionMapper(string currentVersion, List<KeyValuePair<string, Type>> versions)
        {
            this.currentVersion = currentVersion;
            this.versions = versions;
        }

        public object Parse(string json, string version)
        {
            // TODO: Handle unknown versions
            Type type = versions.Single(p => p.Key == version).Value;

            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });
        }

        public T ToCurrentVersion(VersionedDocument versionedDocument)
        {
            return (T) this.ToVersion(versionedDocument, this.currentVersion);
        }

        public object ToVersion(VersionedDocument versionedDocument, string version)
        {

            // TODO: Handle unknown versions
            Type requestedType = versions.Single(p => p.Key == version).Value;
            Type dataType = versions.Single(p => p.Key == versionedDocument.Version).Value;

            // TODO: do some iterative mapping up or down the version chain until we get the one we want.
            return AutoMapper.Mapper.Map(this.Parse(versionedDocument.Document, versionedDocument.Version), dataType, requestedType);
        }
    }
}
