using System.Collections.Generic;
using Newtonsoft.Json;
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
            };

        public string Serialize(Questionnaire questionnaire)
        {
            var json = JsonConvert.SerializeObject(questionnaire, jsonSerializerSettings);
            return json;
        }

        public Questionnaire Deserialize(string json)
        {
            var questionnaire = JsonConvert.DeserializeObject<Questionnaire>(json);
            return questionnaire;
        }

        public string Serialize<T>(List<T> items)
        {
            var json = JsonConvert.SerializeObject(items, jsonSerializerSettings);
            return json;
        }
        
        public List<T> Deserialize<T>(string json)
        {
            var list = JsonConvert.DeserializeObject<List<T>>(json);
            return list;
        }
    }
}