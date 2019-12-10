
using WB.UI.Headquarters.Models.ComponentModels;

namespace WB.UI.Headquarters.Models.Reports
{
    public class StatusDurationModel
    {
        public string DataUrl { get; set; }

        public string InterviewsBaseUrl { get; set; }

        public string AssignmentsBaseUrl { get; set; }

        public string QuestionnairesUrl { get; set; }

        public string QuestionnaireByIdUrl { get; set; }

        public bool IsSupervisorMode { set; get; }
    }
}
