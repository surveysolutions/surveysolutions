using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Interview
{
    public class AllInterviewsView : IListView<AllInterviewsViewItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<AllInterviewsViewItem> Items { get; set; }
    }
}