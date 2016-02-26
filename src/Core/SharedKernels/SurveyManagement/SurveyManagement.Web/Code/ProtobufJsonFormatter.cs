using System;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code
{
    [Obsolete("Since v. 5.7")]
    public class ProtobufJsonFormatter : MediaTypeFormatter
    {
        private IProtobufSerializer serializer
        {
            get { return ServiceLocator.Current.GetInstance<IProtobufSerializer>(); }
        }

        public ProtobufJsonFormatter()
        {
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            this.SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            this.SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
        }
        public override bool CanReadType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override bool CanWriteType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return true;
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            return await Task.Run(() =>
            {
                var bytesOfStream = Convert.FromBase64String(new StreamReader(readStream).ReadToEnd());
                return this.serializer.DeserializeFromStream(stream: new MemoryStream(bytesOfStream), type: type);
            });
        }
        
        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            return Task.Run(() => this.serializer.SerializeToStream(value: value, type: type, stream: writeStream));
        }
    }
}