using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.UI.Headquarters.Models;

namespace WB.UI.Headquarters.Utils
{
    public class TranslationModelConverter : JsonConverter
    {
        private static readonly CamelCasePropertyNamesContractResolver ContractResolver = new CamelCasePropertyNamesContractResolver
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
                serializer.ContractResolver = ContractResolver;

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
}