using Newtonsoft.Json;

namespace WB.Infrastructure.Native.Storage
{
    internal static class KeyValueSerializerSettings
    {
        internal static readonly JsonSerializerSettings BackwardCompatibleJsonSerializerSettings = new JsonSerializerSettings
        {
                 TypeNameHandling = TypeNameHandling.Auto,
                 DefaultValueHandling = DefaultValueHandling.Ignore,
                 MissingMemberHandling = MissingMemberHandling.Ignore,
                 NullValueHandling = NullValueHandling.Ignore,
                 Binder = new OldToNewAssemblyRedirectSerializationBinder()
            };
    }
}
