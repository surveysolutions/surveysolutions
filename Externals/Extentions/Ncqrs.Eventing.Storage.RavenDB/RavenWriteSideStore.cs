using System.Linq;
using Raven.Abstractions.Json;
using Raven.Client.Document;
using Raven.Imports.Newtonsoft.Json;

namespace Ncqrs.Eventing.Storage.RavenDB
{
    public class RavenWriteSideStore
    {
        protected internal static DocumentConvention CreateStoreConventions(string ravenCollectionName)
        {
            return new DocumentConvention
            {
                JsonContractResolver = new PropertiesOnlyContractResolver(),
                FindTypeTagName = x => ravenCollectionName,
                CustomizeJsonSerializer = CustomizeJsonSerializer,
            };
        }

        private static void CustomizeJsonSerializer(JsonSerializer serializer)
        {
            serializer.Binder = new TypeNameSerializationBinder();

            // if we want to perform serialized type name substitution
            // then JsonDynamicConverter should be removed
            // that is because JsonDynamicConverter handles System.Object types
            // and it by itself does not recognized substituted type
            // and does not allow our custom serialization binder to work
            RemoveJsonDynamicConverter(serializer.Converters);
        }

        private static void RemoveJsonDynamicConverter(JsonConverterCollection converters)
        {
            JsonConverter jsonDynamicConverter = converters.SingleOrDefault(converter => converter is JsonDynamicConverter);

            if (jsonDynamicConverter != null)
            {
                converters.Remove(jsonDynamicConverter);
            }
        }
    }
}