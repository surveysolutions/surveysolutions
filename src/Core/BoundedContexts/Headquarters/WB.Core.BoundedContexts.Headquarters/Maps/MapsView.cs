using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Maps;

namespace WB.Core.BoundedContexts.Headquarters.Maps
{
    public class MapsView : IListView<MapBrowseItem>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }

        public int TotalCount { get; set; }
        public IEnumerable<MapBrowseItem> Items { get; set; }
    }
}
