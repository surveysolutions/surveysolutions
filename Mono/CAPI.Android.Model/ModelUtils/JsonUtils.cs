using System;
using System.Collections.Generic;
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

        public static T GetObject<T>(string json) where T : class
        {
            return JsonConvert.DeserializeObject<T>(json,
                new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Objects, NullValueHandling = NullValueHandling.Ignore
                });
        }
    }
}