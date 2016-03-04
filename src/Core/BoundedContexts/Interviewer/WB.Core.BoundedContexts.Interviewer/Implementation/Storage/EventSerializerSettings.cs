using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Storage
{
    internal static class EventSerializerSettings
    {
        internal static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto, //TypeNameHandling.All,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal
        };
    }
}
