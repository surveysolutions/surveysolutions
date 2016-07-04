using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views.Supervisor
{
    public class SupervisorsView : IListView<SupervisorsItem>
    {
        public int TotalCount { get; set; }

        public IEnumerable<SupervisorsItem> Items { get; set; }
    }
}