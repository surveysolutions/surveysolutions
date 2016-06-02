using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Headquarters.Views.Survey
{
    public class SurveyStatusViewItem
    {
        public InterviewStatus Status { get; set; }

        public string StatusName { get; set; }
    }
}