using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class UserMapsView : IListView<UserMapsItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<UserMapsItem> Items { get; set; }
    }
}
