using System;
using System.IO;
using Sqo;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;

namespace WB.Core.Infrastructure.Android.Implementation.Services.Storage
{
    internal class PlainStorageSerializer : IDocumentSerializer
    {
        private readonly IJsonUtils jsonUtils;

        public PlainStorageSerializer(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
        }

        public object Deserialize(Type type, byte[] objectBytes)
        {
            return this.jsonUtils.DeserializeFromStreamAsync(new MemoryStream(objectBytes), type).Result;
        }

        public byte[] Serialize(object obj)
        {
            return this.jsonUtils.SerializeToByteArrayAsync(obj).Result;
        }
    }
}