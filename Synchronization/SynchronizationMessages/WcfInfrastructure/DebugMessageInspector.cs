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

            var w = Encoding.UTF8.GetString("H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Iv7d5Hu/cZKmvxj/pOlHv1t7vco/epR+9EVWLMcnVZ2Pn1bT9SJfts34ZV1N86bJZ6eX9PfJfL18O0ptw49GCsN+O337cj0pi+nvlV8D5KcP9yc7D+/l2+f7u9n2/sH96fbB+c7+9r2D/fv7e5N792YHkxAIvdjQm9+Tz+jTyfnebPf+5NPtg4fnD7b3H+wRiJ18d/vBg2z2YHf//t7k/L4BQc3vn0+ynf17n25PZ3sH2/vns/PtycPd6fbew3x3cjA9z873bI/U/OF0J/v0fHKwvXPv0/vb+5P9h9sP8wf3t3em04f5dO+cPrCjpOY796cPs729B9uzPWq5vzs72H6Y7e5uP3yYf7qT72QP81nmQ9+dPtibEfR8ev9Tgn7+6XZ2fkBd7Jw/fLi7t3cv2/vUb07YZrOd2fY9Are9v/Mw287u7xxs7+3u7GR7+3v3p9Mdr/mns/zg008zGlt2MNven00fbh9kB/e3J+fUdHJv92Ca+cicz2YP9nbu723n59k+QZ9Mt7P982x7N8vu79/LZvt7032v+YMHuwezT4kUeY6hPpiebz+cTibb9x5k+c5edv9glj/wmu/tT3cfnOf59mz/IKfm93e3J1Mi6cHupw/v59nOp7N9H/qMGOLg09n97QcHoPsuzdDDvb0ZYXTv/t69e/vn0/zAR2a6/ykNcm+bsL63vX9v/2A7u5cRNQmXB/ce7N47fzj1mh98Ot3d26fm9++fTwmZ3T2a1XvT7dkuzUE+239w755PyAfne9n+/QfZ9t6D+0SZ7AEx28757vbu/s70/sOHs33iKB/3ew/Pd3ceEtqfPiS6731Kzff37m3foxl9cP/g/mR6vutzZPbwQb63s7e9f39GDHy+v0/I0MgPJp8SM3/66cOd2Z6PDDH0wXR3QuR7sLO9/3B6QMjQDBAfTR/MCEESIq/5lFhotn9vQnK1y3QnouzSeO9lu9P7WTZ7+HBv5jPBg+kkzw7u0TTNaKj7D3eoOQnXvf0Hn96n72aTnXs+9PPZg4PJ7r3tTyc0ofv3iMUe3n842d6hTg929h7s33sYsNj5dLq3tzPZfjijf/bv0axO7n/6cPs+jXXvwSzbyx/6/H5Orc4PSDwe7j0kFstnxJEPIIvTT6cHJNaT2YGPzOz+vcneLon+/vkuEX8/A4sRgz48IDKeTw8+3Zn4srr38NMsP6exnZ9PiO737hHP7N2/v7032b1/frD34MHkno87cf+UoJOsnk+p+d50QpTJaBry+7P7D/dJWPfPP5LW3zca69vZclbmM9JXO/jkl/zGyff/nwAAAP//Uw6Zbl0FAAA=".Select(e => (byte)e).ToArray());


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
