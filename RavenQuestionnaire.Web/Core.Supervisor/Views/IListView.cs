using System.Collections.Generic;

namespace Core.Supervisor.Views
{
    interface IListView<T>
    {
        int TotalCount { get; set; }
        IEnumerable<T> Items { get; set; }
    }
}
