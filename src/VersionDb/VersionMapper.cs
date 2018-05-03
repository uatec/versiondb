using System;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VersionDb
{
    public class VersionMapper
    {
        private readonly VersionRegistration[] versions;

        public VersionMapper(params VersionRegistration[] versions)
        {
            this.versions = versions;
        }

        public object Parse(string json, string version)
        {
            // TODO: Handle unknown versions
            Type type = versions.Single(p => p.VersionCode == version).Type;

            return JsonConvert.DeserializeObject(json, type, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error
            });
        }

        public object ToVersion(VersionedDocument versionedDocument, string version)
        {
            // TODO: Handle unknown versions
            Type requestedType = versions.Single(p => p.VersionCode == version).Type;
            Type dataType = versions.Single(p => p.VersionCode == versionedDocument.Version).Type;

            // TODO: do some iterative mapping up or down the version chain until we get the one we want.
            return AutoMapper.Mapper.Map(this.Parse(versionedDocument.Document, versionedDocument.Version), dataType, requestedType);
        }
    }
}
