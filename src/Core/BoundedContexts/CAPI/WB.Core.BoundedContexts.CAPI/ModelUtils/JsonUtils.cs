using System.IO;
using Newtonsoft.Json;

namespace WB.Core.BoundedContexts.Capi.ModelUtils
{
    public static class JsonUtils
    {
        public static string GetJsonData(object payload)
        {
            var data = JsonConvert.SerializeObject(payload, Formatting.None,
                                                   new JsonSerializerSettings
                                                   {
                                                       TypeNameHandling = TypeNameHandling.Objects,
                                                       NullValueHandling = NullValueHandling.Ignore
                                                   });
            return data;
        }

        public static T GetObject<T>(string json)
        {
            var type = typeof (T);
            if (type.IsValueType)
                return JSONDeserialize<T>(json);
            return JsonConvert.DeserializeObject<T>(json,
                                                    new JsonSerializerSettings
                                                        {
                                                            TypeNameHandling = TypeNameHandling.Objects,
                                                            NullValueHandling = NullValueHandling.Ignore,

                                                            Error = delegate(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
                                                            {
                                                                args.ErrorContext.Handled = true;
                                                            }
                                                        });
        }

        private static T JSONDeserialize<T>(string jsonText)
        {
            var jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var sr = new StringReader(jsonText);
            var reader = new JsonTextReader(sr);
            var result = jsonSerializer.Deserialize(reader, typeof (T));
            reader.Close();

            return (T) result;
        }
    }
}