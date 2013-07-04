namespace SynchronizationMessages.CompleteQuestionnaire
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization;
    using System.Xml;

    using SynchronizationMessages.Synchronization;

    /// <summary>
    /// The json serrializer.
    /// </summary>
    public class JsonSerrializer : XmlObjectSerializer
    {
        #region Constants

        /// <summary>
        /// The my prefix.
        /// </summary>
        private const string MyPrefix = "new";

        #endregion

        #region Fields

        /// <summary>
        /// The fallback serializer.
        /// </summary>
        private readonly XmlObjectSerializer fallbackSerializer;

        /// <summary>
        /// The is custom serialization.
        /// </summary>
        private readonly bool isCustomSerialization;

        /// <summary>
        /// The type.
        /// </summary>
        private readonly Type type;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSerrializer"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="fallbackSerializer">
        /// The fallback serializer.
        /// </param>
        public JsonSerrializer(Type type, XmlObjectSerializer fallbackSerializer)
        {
            this.type = type;
            this.isCustomSerialization = typeof(ICustomSerializable).IsAssignableFrom(type);
            this.fallbackSerializer = fallbackSerializer;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The is start object.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
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

        /// <summary>
        /// The read object.
        /// </summary>
        /// <param name="reader">
        /// The reader.
        /// </param>
        /// <param name="verifyObjectName">
        /// The verify object name.
        /// </param>
        /// <returns>
        /// The System.Object.
        /// </returns>
        public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
        {
            if (this.isCustomSerialization)
            {
                object result = Activator.CreateInstance(this.type);

                using (var memory = new MemoryStream())
                {
                    byte[] compressedData = reader.ReadElementContentAsBase64();
                    memory.Write(compressedData, 0, compressedData.Length);
                    memory.Position = 0L;

                    using (var zip = new GZipStream(memory, CompressionMode.Decompress, true))
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

        /// <summary>
        /// The write end object.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
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

        /// <summary>
        /// The write object content.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="graph">
        /// The graph.
        /// </param>
        public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
        {
            if (this.isCustomSerialization)
            {
                byte[] result = null;
                using (var memory = new MemoryStream())
                {
                    using (var zip = new GZipStream(memory, CompressionMode.Compress, true))
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

        /// <summary>
        /// The write start object.
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        /// <param name="graph">
        /// The graph.
        /// </param>
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

        #endregion
    }
}