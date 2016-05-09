using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class VariablesValuesChanged: InterviewPassiveEvent
    {
        public ChangedVariableValueDto[] ChangedVariables { get; private set; }

        public VariablesValuesChanged(ChangedVariableValueDto[] changedVariables)
        {
            this.ChangedVariables = changedVariables;
        }
    }
}