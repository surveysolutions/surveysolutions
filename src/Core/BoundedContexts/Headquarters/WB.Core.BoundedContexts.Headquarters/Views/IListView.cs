using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    interface IListView<T>
    {
        int TotalCount { get; set; }
        IEnumerable<T> Items { get; set; }
    }
}
