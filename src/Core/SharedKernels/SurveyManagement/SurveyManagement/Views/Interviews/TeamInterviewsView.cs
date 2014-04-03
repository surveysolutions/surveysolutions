using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interviews
{
    public class TeamInterviewsView 
    {
        public int TotalCount { get; set; }
        public IEnumerable<TeamInterviewsViewItem> Items { get; set; }
    }
}
