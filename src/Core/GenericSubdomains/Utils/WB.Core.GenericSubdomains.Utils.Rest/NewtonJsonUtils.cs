using System.IO;
using Newtonsoft.Json;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Utils.Rest
{
    using System;

    public class NewtonJsonUtils : IJsonUtils
    {
        private readonly JsonSerializer jsonSerializer;
        private readonly JsonSerializerSettings jsonSerializerSetings;

        public NewtonJsonUtils()
        {
            this.jsonSerializerSetings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                NullValueHandling = NullValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                //Binder = new AssemblyNameRaplaceSerializationBinder()
            };
            this.jsonSerializer = JsonSerializer.Create(this.jsonSerializerSetings);
        }

        public string Serialize(object item)
        {
            return JsonConvert.SerializeObject(item, Formatting.None, this.jsonSerializerSetings);
        }

        public byte[] SerializeToByteArray(object payload)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
                jsonSerializer.Serialize(writer, payload);
            return output.ToArray();
        }

        public T Deserialize<T>(string payload)
        {
            return JsonConvert.DeserializeObject<T>(ReplaceOldAssemblyNames(payload), this.jsonSerializerSetings);
        }

        [Obsolete]
        private static string ReplaceOldAssemblyNames(string payload)
        {
            var replaceOldAssemblyNames = payload;
            replaceOldAssemblyNames = replaceOldAssemblyNames.Replace("Main.Core.Events.AggregateRootEvent, Main.Core", "Main.Core.Events.AggregateRootEvent, WB.Core.Infrastructure");
            replaceOldAssemblyNames = replaceOldAssemblyNames.Replace("Main.Core.Events.QuestionnaireDocument, Main.Core", "Main.Core.Events.AggregateRootEvent, WB.Core.SharedKernels.SurveySolutions");
            return replaceOldAssemblyNames;
        }

        public T Deserialize<T>(byte[] payload)
        {
            var input = new MemoryStream(payload);
            using (var reader = new StreamReader(input))
            {
                return jsonSerializer.Deserialize<T>(new JsonTextReader(reader));
            }
        }
    }

    //[Obsolete]
    //public class AssemblyNameRaplaceSerializationBinder : DefaultSerializationBinder
    //{
    //    public override Type BindToType(string assemblyName, string typeName)
    //    {
    //        if (assemblyName == "Main.Core") assemblyName = "WB.Core.Infrastructure";
    //        return base.BindToType(assemblyName, typeName);
    //    }
    //}
}
