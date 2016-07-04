using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UsersView
    {
        public IEnumerable<UsersViewItem> Users { get; set; }
        public int TotalCountByQuery { get; set; }
    }
}