using Newtonsoft.Json;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Infrastructure.Native.Storage
{
    public class EntitySerializer<TEntity> : IEntitySerializer<TEntity> where TEntity: class
    {
        public string Serialize(TEntity entity)
        {
            var serializedValue = JsonConvert.SerializeObject(entity, Formatting.None, BackwardCompatibleJsonSerializerSettings);
            return serializedValue;
        }

        public TEntity Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<TEntity>(json, BackwardCompatibleJsonSerializerSettings);
        }


        private static readonly JsonSerializerSettings BackwardCompatibleJsonSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            Binder = new OldToNewAssemblyRedirectSerializationBinder()
        };
    }
}