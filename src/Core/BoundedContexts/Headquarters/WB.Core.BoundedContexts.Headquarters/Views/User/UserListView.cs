using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserListView : IListView<InterviewersItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<InterviewersItem> Items { get; set; }
        public InterviewersItem ItemsSummary { get; set; }
    }
}