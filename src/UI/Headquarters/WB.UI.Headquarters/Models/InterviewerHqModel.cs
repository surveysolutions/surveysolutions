using Newtonsoft.Json;
using WB.UI.Headquarters.Models.ComponentModels;
using WB.UI.Headquarters.Utils;

namespace WB.UI.Headquarters.Models
{
    public class InterviewerHqModel
    {
        public string AllInterviews { get; set; }
        public string InterviewerHqEndpoint { get; set; }
        public string[] Statuses { get; set; }
        public TranslationModel Resources { get; set; }

        public string Title { get; set; }
        public ComboboxOptionModel[] Questionnaires { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}