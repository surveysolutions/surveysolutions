using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using Newtonsoft.Json.Converters;

namespace WB.UI.Headquarters.Models
{
    public class InterviewerHqModel
    {
        public string AllInterviews { get; set; }
        public string InterviewerHqEndpoint { get; set; }
        public string[] Statuses { get; set; }
        public Dictionary<string, string> Resources { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MenuItem Title { get; set; }

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

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, ModelSerializationSettings);
        }
    }
}