using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.BoundedContexts.Designer.ImportExport.Models;

namespace WB.Core.BoundedContexts.Designer.ImportExport
{
    public class QuestionnaireSerializer : IQuestionnaireSerializer
    {
        private static readonly JsonSerializerSettings jsonSerializerSettings = 
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                FloatParseHandling = FloatParseHandling.Decimal,
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>()
                {
                    new Newtonsoft.Json.Converters.StringEnumConverter()
                },
                ContractResolver = new ShouldSerializeContractResolver()
            };
        
        public class ShouldSerializeContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) 
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                if (property.PropertyType == typeof(string)) 
                {
                    property.ShouldSerialize = instance => !string.IsNullOrEmpty(instance.GetType().GetProperty(property.PropertyName).GetValue(instance) as string);
                }
                else if (property.PropertyType?.GetInterface(nameof(IEnumerable)) != null)
                {
                    property.ShouldSerialize =
                        instance => (instance?.GetType().GetProperty(property.PropertyName).GetValue(instance) as IEnumerable<object>)?.Count() > 0;
                }
                return property;
            }
        }

        public string Serialize(Questionnaire questionnaire)
        {
            var json = JsonConvert.SerializeObject(questionnaire, jsonSerializerSettings);
            return json;
        }

        public Questionnaire? Deserialize(string json)
        {
            var questionnaire = JsonConvert.DeserializeObject<Questionnaire>(json);
            return questionnaire;
        }

        public string Serialize<T>(List<T> items)
        {
            var json = JsonConvert.SerializeObject(items, jsonSerializerSettings);
            return json;
        }
        
        public List<T>? Deserialize<T>(string json)
        {
            var list = JsonConvert.DeserializeObject<List<T>>(json);
            return list;
        }
    }
}
