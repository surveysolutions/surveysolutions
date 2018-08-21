using System;
using System.IO;
using System.Net;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;

namespace WB.UI.Designer.Code
{
    public class JsonFormatter : MediaTypeFormatter
    {
        private readonly Func<Version> headquartersVersionGetter;

        public JsonFormatter(Func<Version> hqVersionGetter = null)
        {
            this.headquartersVersionGetter = hqVersionGetter ?? GetHeadquartersVersion;

            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/json"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));
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

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, System.Net.Http.HttpContent content, IFormatterLogger formatterLogger)
        {
            var headquartersVersion = this.headquartersVersionGetter.Invoke();
            return Task.FromResult(this.DeserializeFromStream(stream: readStream, type: type, headquartersVersion: headquartersVersion));
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, TransportContext transportContext)
        {
            var headquartersVersion = this.headquartersVersionGetter.Invoke();
            return Task.Run(() => this.SerializeToStream(value: value, type: type, stream: writeStream, headquartersVersion: headquartersVersion));
        }

        private object DeserializeFromStream(Stream stream, Type type, Version headquartersVersion)
        {
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return JsonSerializer.Create(CreateJsonSerializerSettings(headquartersVersion)).Deserialize(jsonTextReader, type);
            }
        }

        private void SerializeToStream(object value, Type type, Stream stream, Version headquartersVersion)
        {
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false, true), 1024, true))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer.Create(CreateJsonSerializerSettings(headquartersVersion)).Serialize(jsonWriter, value, type);
            }
        }

        private Version GetHeadquartersVersion()
        {
            var userAgent = HttpContext.Current.Request.UserAgent;
            var hqVersionWithFlags = userAgent?.Substring(@"WB.Headquarters/".Length);
            var startIndexOfFlags = hqVersionWithFlags?.IndexOf(" ");
            var stringVersion = !startIndexOfFlags.HasValue || startIndexOfFlags == -1 
                ? hqVersionWithFlags
                : hqVersionWithFlags?.Remove(startIndexOfFlags.Value);

            if (Version.TryParse(stringVersion, out Version version))
                return version;

            return null;
        }

        private JsonSerializerSettings CreateJsonSerializerSettings(Version headquartersVersion) =>
            new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            NullValueHandling = NullValueHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Decimal,
            Formatting = Formatting.None,
            Binder = new DesignerOldCompatibilityBinder(headquartersVersion)
        };
    }
}
