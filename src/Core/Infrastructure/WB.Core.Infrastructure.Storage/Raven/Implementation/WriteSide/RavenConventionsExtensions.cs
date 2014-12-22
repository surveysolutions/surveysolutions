using System.Linq;
using Raven.Abstractions.Json;
using Raven.Abstractions.Replication;
using Raven.Client;
using Raven.Imports.Newtonsoft.Json;

namespace WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide
{
    public static class RavenConventionsExtensions
    {
        public static void UpdateStoreConventions(this IDocumentStore store, string ravenCollectionName, FailoverBehavior failoverBehavior = FailoverBehavior.FailImmediately)
        {
            store.Conventions.FailoverBehavior = failoverBehavior;
            store.Conventions.JsonContractResolver = new PropertiesOnlyContractResolver();
            store.Conventions.FindTypeTagName = x => ravenCollectionName;
            store.Conventions.CustomizeJsonSerializer = CustomizeJsonSerializer;
        }

        private static void CustomizeJsonSerializer(JsonSerializer jsonSerializer)
        {
            SetupSerializerToIgnoreAssemblyNameForEvents(jsonSerializer);
        }


        private static void SetupSerializerToIgnoreAssemblyNameForEvents(JsonSerializer serializer)
        {
            serializer.Binder = new IgnoreAssemblyNameForEventsSerializationBinder();

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