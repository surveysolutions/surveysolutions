using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace CAPI.Android.Core.Model.ModelUtils
{
    public static class JsonUtils
    {
        public static string GetJsonData(object payload)
        {
            var data = JsonConvert.SerializeObject(payload, Formatting.None,
                                                   new JsonSerializerSettings
                                                   {
                                                       TypeNameHandling = TypeNameHandling.Objects, NullValueHandling = NullValueHandling.Ignore
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
                                                            NullValueHandling = NullValueHandling.Ignore
                                                        });
        }

        private static T JSONDeserialize<T>(string jsonText)
        {

            Newtonsoft.Json.JsonSerializer json = new Newtonsoft.Json.JsonSerializer();

            json.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            json.ObjectCreationHandling = Newtonsoft.Json.ObjectCreationHandling.Replace;
            json.MissingMemberHandling = Newtonsoft.Json.MissingMemberHandling.Ignore;
            json.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            StringReader sr = new StringReader(jsonText);
            Newtonsoft.Json.JsonTextReader reader = new JsonTextReader(sr);
            object result = json.Deserialize(reader, typeof (T));
            reader.Close();

            return (T) result;
        }
    }
}