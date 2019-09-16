using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Headquarters.Assignments
{
    public class AssignmentHistory
    {
        public AssignmentHistory()
        {
            this.History = new List<AssignmentHistoryItem>();
        }

        public List<AssignmentHistoryItem> History { get; set; }

        public int RecordsFiltered { get; set; }
    }
}