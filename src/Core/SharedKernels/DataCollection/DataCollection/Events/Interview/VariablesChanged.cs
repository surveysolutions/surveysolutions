using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class VariablesChanged: InterviewPassiveEvent
    {
        public ChangedVariable[] ChangedVariables { get; private set; }

        public VariablesChanged(ChangedVariable[] changedVariables)
        {
            this.ChangedVariables = changedVariables;
        }
    }
}