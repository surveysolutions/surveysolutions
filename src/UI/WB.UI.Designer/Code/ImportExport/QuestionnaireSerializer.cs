using System.Collections.Generic;
using Newtonsoft.Json;
using WB.UI.Designer.Code.ImportExport.Models;

namespace WB.UI.Designer.Code.ImportExport
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
    }
}