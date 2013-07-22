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
        }
    }
}