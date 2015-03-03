using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviewer
{
    public class InterviewersView : IListView<InterviewersItem>
    {
        public int TotalCount { get; set; }

        public IEnumerable<InterviewersItem> Items { get; set; }

        public InterviewersItem ItemsSummary { get; set; }
    }
}