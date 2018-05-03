using System;

namespace VersionDb
{
    public class VersionRegistration
    {
        public VersionRegistration(string versionCode, Type type, Func<object, object> upgrade = null, Func<object, object> downgrade = null)
        {
            VersionCode = versionCode;
            Type = type;
            Upgrade = upgrade;
            Downgrade = downgrade;
        }

        public string VersionCode { get; }
        public Type Type { get; }
        public Func<object, object> Upgrade { get; }
        public Func<object, object> Downgrade { get; }
    }
}
