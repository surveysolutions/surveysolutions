using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class RosterRowsAdded : InterviewPassiveEvent
    {
        public AddedRosterRowDto[] AddedRows { get; private set; }

        public RosterRowsAdded(AddedRosterRowDto[] addedRows)
        {
            this.AddedRows = addedRows;
        }
    }
}