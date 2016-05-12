using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class VariablesValuesChanged: InterviewPassiveEvent
    {
        public ChangedVariable[] ChangedVariables { get; private set; }

        public VariablesValuesChanged(ChangedVariable[] changedVariables)
        {
            this.ChangedVariables = changedVariables;
        }
    }
}