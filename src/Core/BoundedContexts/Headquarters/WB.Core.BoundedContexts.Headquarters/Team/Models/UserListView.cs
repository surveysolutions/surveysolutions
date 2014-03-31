using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Team.Models
{
    /// <summary>
    /// The user list view.
    /// </summary>
    public class UserListView
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<UserListItem> Items { get; set; }
        public UserListItem ItemsSummary { get; set; }
    }
}