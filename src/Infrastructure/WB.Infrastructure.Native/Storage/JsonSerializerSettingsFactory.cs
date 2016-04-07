using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Infrastructure.Native.Storage
{
    public class JsonSerializerSettingsFactory : IJsonSerializerSettingsFactory
    {
        private readonly JsonSerializerSettings AllTypesJsonSerializerSettings;
        private readonly JsonSerializerSettings ObjectsOnlyJsonSerializerSettings;

        public JsonSerializerSettingsFactory()
        {
            OldToNewAssemblyRedirectSerializationBinder oldToNewBinder = new OldToNewAssemblyRedirectSerializationBinder();

            this.AllTypesJsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Binder = oldToNewBinder
            };

            this.ObjectsOnlyJsonSerializerSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.None,
                Binder = oldToNewBinder
            };
        }

        public JsonSerializerSettings GetAllTypesJsonSerializerSettings()
        {
            return AllTypesJsonSerializerSettings;
        }

        public JsonSerializerSettings GetObjectsJsonSerializerSettings()
        {
            return ObjectsOnlyJsonSerializerSettings;
        }
    }
}