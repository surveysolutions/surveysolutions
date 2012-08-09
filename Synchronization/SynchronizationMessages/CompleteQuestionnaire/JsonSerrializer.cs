using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Runtime.Serialization;
using SynchronizationMessages.Synchronization;

namespace SynchronizationMessages.CompleteQuestionnaire
{
    public class JsonSerrializer: XmlObjectSerializer
    {
        const string MyPrefix = "new";
        Type type;
        bool isCustomSerialization;
        XmlObjectSerializer fallbackSerializer;
        public JsonSerrializer(Type type, XmlObjectSerializer fallbackSerializer)
        {
            this.type = type;
            this.isCustomSerialization = typeof(ICustomSerializable).IsAssignableFrom(type);
            this.fallbackSerializer = fallbackSerializer;
        }

        public override bool IsStartObject(XmlDictionaryReader reader)
        {
            if (this.isCustomSerialization)
            {
                return reader.LocalName == MyPrefix;
            }
            else
            {
                return this.fallbackSerializer.IsStartObject(reader);
            }
        }

        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            if (this.isCustomSerialization)
            {
                object result = Activator.CreateInstance(this.type);

                using (MemoryStream memory = new MemoryStream())
                {
                    var compressedData = reader.ReadElementContentAsBase64();
                    memory.Write(compressedData, 0, compressedData.Length);
                    memory.Position = 0L;

                    using (GZipStream zip = new GZipStream(memory, CompressionMode.Decompress, true))
                    {
                        zip.Flush();
                        ((ICustomSerializable)result).InitializeFrom(zip);
                    }
                }

                return result;
            }
            else
            {
                return this.fallbackSerializer.ReadObject(reader, verifyObjectName);
            }
        }

        public override void WriteEndObject(XmlDictionaryWriter writer)
        {
            if (this.isCustomSerialization)
            {
                writer.WriteEndElement();
            }
            else
            {
                this.fallbackSerializer.WriteEndObject(writer);
            }
        }

        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            if (this.isCustomSerialization)
            {
                byte[] result = null;
                using (MemoryStream memory = new MemoryStream())
                {
                    using (GZipStream zip = new GZipStream(memory, CompressionMode.Compress, true))
                    {
                        ((ICustomSerializable)graph).WriteTo(zip);
                    }

                    result = memory.ToArray();
                }
                writer.WriteBase64(result, 0, result.Length);
            }
            else
            {
                this.fallbackSerializer.WriteObjectContent(writer, graph);
            }
        }

        public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
        {
            if (this.isCustomSerialization)
            {
                writer.WriteStartElement(MyPrefix);
            }
            else
            {
                this.fallbackSerializer.WriteStartObject(writer, graph);
            }
        }
    }
}
