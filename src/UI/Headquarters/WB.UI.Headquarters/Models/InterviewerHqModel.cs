using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.UI.Headquarters.Models.ComponentModels;

namespace WB.UI.Headquarters.Models
{
    public class InterviewerHqModel
    {
        public string AllInterviews { get; set; }
        public string InterviewerHqEndpoint { get; set; }
        public string[] Statuses { get; set; }
        public string Title { get; set; }
        public ComboboxOptionModel[] Questionnaires { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, ModelSerializationSettings);
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
