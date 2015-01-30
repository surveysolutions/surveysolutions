using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    public class UserListView : IListView<UserListItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<UserListItem> Items { get; set; }
        public UserListItem ItemsSummary { get; set; }
    }
}