using System;
using System.IO;
using Sqo;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    class StorageSerializer : IDocumentSerializer
    {
        private readonly IJsonUtils jsonUtils;

        public StorageSerializer(IJsonUtils jsonUtils)
        {
            this.jsonUtils = jsonUtils;
        }

        public object Deserialize(Type type, byte[] objectBytes)
        {
            return this.jsonUtils.DeserializeFromStream(new MemoryStream(objectBytes), type);
        }

        public byte[] Serialize(object obj)
        {
            return this.jsonUtils.SerializeToByteArray(obj);
        }
    }
}