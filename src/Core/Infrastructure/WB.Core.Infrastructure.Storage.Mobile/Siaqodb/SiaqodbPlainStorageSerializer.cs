using System;
using System.IO;
using Sqo;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.Infrastructure.Storage.Mobile.Siaqodb
{
    public class SiaqodbPlainStorageSerializer : IDocumentSerializer
    {
        private readonly IJsonUtils jsonUtils;

        public SiaqodbPlainStorageSerializer(IJsonUtils jsonUtils)
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