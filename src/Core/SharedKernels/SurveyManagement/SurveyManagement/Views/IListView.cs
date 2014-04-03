using System.Collections.Generic;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    interface IListView<T>
    {
        int TotalCount { get; set; }
        IEnumerable<T> Items { get; set; }
    }
}
