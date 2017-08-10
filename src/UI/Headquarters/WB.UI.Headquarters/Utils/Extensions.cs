using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace WB.UI.Headquarters.Utils
{
    public static class Extensions
    {
        public static T GetActionArgumentOrDefault<T>(this HttpActionExecutedContext context, string argument, T defaultValue)
        {
            object value;
            if (!context.ActionContext.ActionArguments.TryGetValue(argument, out value))
                return defaultValue;

            if (value is T)
            {
                return (T)value;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                return defaultValue;
            }
        }

        [Localizable(false)]
        public static TranslationModel Translations(this ResourceManager[] resources)
        {
            return new TranslationModel(resources);
        }
    }

    public class TranslationModelConverter : JsonConverter
    {
        private CamelCasePropertyNamesContractResolver contractResolver = new CamelCasePropertyNamesContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = false
            }
        };

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is TranslationModel model)
            {
                var contract = serializer.ContractResolver;
                serializer.ContractResolver = contractResolver;

                try
                {
                    serializer.Serialize(writer, model.Object);
                }
                finally
                {
                    serializer.ContractResolver = contract;
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TranslationModel);
        }
    }

    [JsonConverter(typeof(TranslationModelConverter))]
    public class TranslationModel
    {
        public TranslationModel(ResourceManager[] resources)
        {
            Add(resources);
        }

        public Dictionary<string, Dictionary<string, string>> Object { get; } = new Dictionary<string, Dictionary<string, string>>();

        public void Add(string @namespace, string key, string value)
        {
            if (!Object.ContainsKey(@namespace))
            {
                Object.Add(@namespace, new Dictionary<string, string>());
            }

            Object[@namespace].Add(key, value);
        }

        public void Add(params ResourceManager[] resources)
        {
            foreach (var resource in resources)
            {
                IEnumerable<string> keys = resource
                    .GetResourceSet(CultureInfo.InvariantCulture, true, true)
                    .Cast<DictionaryEntry>()
                    .Select(entry => entry.Key)
                    .Cast<string>();

                var lastDot = resource.BaseName.LastIndexOf(".", StringComparison.Ordinal);
                var @namespace = resource.BaseName.Substring(lastDot > 0 ? lastDot + 1 : 0);

                foreach (var key in keys)
                {
                    Add(@namespace, key, resource.GetString(key, CultureInfo.CurrentUICulture));
                }
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(Object, ModelSerializationSettings);
        }

        private static readonly JsonSerializerSettings ModelSerializationSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            }
        };
    }
}