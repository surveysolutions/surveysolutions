using System.Collections.Generic;
using WB.Core.BoundedContexts.Headquarters.Views;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentsWithoutIdentifingData : IListView<AssignmentRow>
    {
        public int TotalCount { get; set; }
        public IEnumerable<AssignmentRow> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}