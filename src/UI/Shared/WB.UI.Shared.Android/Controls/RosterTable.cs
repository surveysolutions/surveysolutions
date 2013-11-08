using System.Collections.Generic;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails.GridItems;

namespace WB.UI.Shared.Android.Controls
{
    public class RosterTable
    {
        public RosterTable(List<HeaderItem> header, IEnumerable<QuestionnairePropagatedScreenViewModel> rows)
        {
            this.Header = header;
            this.Rows = rows;
        }

        public List<HeaderItem> Header { get; private set; }
        public IEnumerable<QuestionnairePropagatedScreenViewModel> Rows { get; private set; }
    }
}