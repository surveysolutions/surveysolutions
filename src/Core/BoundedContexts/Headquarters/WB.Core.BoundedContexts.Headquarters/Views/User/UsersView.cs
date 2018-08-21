using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UsersView
    {
        public IEnumerable<UsersViewItem> Users { get; set; }
        public int TotalCountByQuery { get; set; }
    }
}
