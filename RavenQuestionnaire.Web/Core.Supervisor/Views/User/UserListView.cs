namespace Core.Supervisor.Views.User
{
    using System.Collections.Generic;

    /// <summary>
    /// The user list view.
    /// </summary>
    public class UserListView : IListView<UserListItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<UserListItem> Items { get; set; }
        public UserListItem ItemsSummary { get; set; }
    }
}