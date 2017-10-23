using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class MapUsersView : IListView<string>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<string> Items { get; set; }
    }
}
