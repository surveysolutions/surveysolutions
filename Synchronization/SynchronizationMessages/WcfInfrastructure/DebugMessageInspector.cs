using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SynchronizationMessages.WcfInfrastructure
{
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    public class DebugMessageInspector : IClientMessageInspector
    {
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            return null;
        }


        public void AfterReceiveReply(ref Message reply, object correlationState)
        {

            var xdr = reply.GetReaderAtBodyContents();
            var XMLDoc = new XmlDocument();
            XMLDoc.Load(xdr);

            var memStream = new MemoryStream();
            var writer = XmlDictionaryWriter.CreateBinaryWriter(memStream);
            XMLDoc.WriteTo(writer);
            writer.Flush();
            memStream.Position = 0;

            var reader = XmlDictionaryReader.CreateBinaryReader(memStream, XmlDictionaryReaderQuotas.Max);
            var newReply = Message.CreateMessage(reply.Version, null, reader);
            for (int i = 0; i < reply.Headers.Count; i++)
            {
                newReply.Headers.CopyHeaderFrom(reply,i);    
            }

            newReply.Properties.CopyProperties(reply.Properties);


            reply = newReply;



        }
    } 
}
