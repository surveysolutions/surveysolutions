using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.Models.Interview
{
    public class SurveyStatusViewItem
    {
        public InterviewStatus Status { get; set; }

        public string StatusName { get; set; }
    }
}