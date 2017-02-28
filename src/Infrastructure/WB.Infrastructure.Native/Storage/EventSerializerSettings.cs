using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Infrastructure.Native.Storage
{
    internal static class EventSerializerSettings
    {
        internal static readonly JsonSerializerSettings BackwardCompatibleJsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter(), new IdentityJsonConverter() },
            Binder = new OldToNewAssemblyRedirectSerializationBinder()
        };
    }
}