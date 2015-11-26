using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Sqo;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    public class SiaqodbSerializer : IDocumentSerializer
    {
        static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Auto,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new JsonConverter[] { new StringEnumConverter() }
        };

        public object Deserialize(Type type, byte[] objectBytes)
        {
            JsonSerializer serializer = JsonSerializer.Create(JsonSerializerSettings);
            BsonReader readeer = new BsonReader(new MemoryStream(objectBytes));
            
            return serializer.Deserialize(readeer, type);
        }

        public byte[] Serialize(object obj)
        {
            JsonSerializer serializer = JsonSerializer.Create(JsonSerializerSettings);
            MemoryStream memoryStream = new MemoryStream();

            BsonWriter writer = new BsonWriter(memoryStream);
            serializer.Serialize(writer, obj);
            return memoryStream.ToArray();
        }



    }
}