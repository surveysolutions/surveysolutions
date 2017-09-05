
using WB.UI.Headquarters.Models.ComponentModels;

namespace WB.UI.Headquarters.Models.Reports
{
    public class CountDaysOfInterviewInStatusModel
    {
        public string BasePath { get; set; }

        public string DataUrl { get; set; }

        public string InterviewsBaseUrl { get; set; }

        public string AssignmentsBaseUrl { get; set; }

        public ComboboxOptionModel[] Questionnaires { get; set; }

        public TranslationModel Resources { get; set; }
    }
}