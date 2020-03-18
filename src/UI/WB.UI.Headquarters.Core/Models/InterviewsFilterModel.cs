using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.UI.Headquarters.Models
{
    public class InterviewsFilterModel
    {
        public string AllInterviews { get; set; }
        
        public ComboboxViewItem[] Statuses { get; set; }
        public string Title { get; set; }
        public List<QuestionnaireVersionsComboboxViewItem> Questionnaires { get; set; }

        public ApiEndpoints Api { get; set; }
        public bool IsSupervisor { get; set; }
        public string AssignmentsUrl { get; set; }
        public bool IsObserver { get; set; }
        public bool IsObserving { get; set; }
        public bool IsHeadquarter { get; set; }
        public string InterviewReviewUrl { get; set; }
        public string ProfileUrl { get; set; }
        public string CommandsUrl { get; set; }

        public class ApiEndpoints
        {
            public string Responsible { get; set; }
            public string InterviewStatuses { get; set; }
            public string QuestionnaireByIdUrl { get; internal set; }
        }


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
