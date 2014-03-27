using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterRowsTitleChanged : InterviewPassiveEvent
    {
        public ChangedRosterRowTitleDto[] ChangedRows { get; private set; }

        public RosterRowsTitleChanged(ChangedRosterRowTitleDto[] changedRows)
        {
            this.ChangedRows = changedRows;
        }
    }
}