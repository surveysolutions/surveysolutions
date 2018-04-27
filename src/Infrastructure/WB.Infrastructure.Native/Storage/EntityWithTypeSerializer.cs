using System.Collections.Generic;
using Newtonsoft.Json;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Infrastructure.Native.Storage
{
    public class EntityWithTypeSerializer<TEntity> : IEntityWithTypeSerializer<TEntity> where TEntity : class
    {
        public string Serialize(TEntity entity)
        {
            var serializedValue = JsonConvert.SerializeObject(entity, Formatting.None, JsonSerializerSettings);
            return serializedValue;
        }

        public TEntity Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<TEntity>(json, JsonSerializerSettings);
        }

        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
            Converters = new List<JsonConverter> { new IdentityJsonConverter(), new RosterVectorConverter() },
        };
    }
}
