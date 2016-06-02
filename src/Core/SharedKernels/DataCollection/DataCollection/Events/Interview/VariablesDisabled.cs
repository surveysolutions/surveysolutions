using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class VariablesDisabled: InterviewPassiveEvent
    {
        public Identity[] Variables { get; private set; }

        public VariablesDisabled(Identity[] variables)
        {
            this.Variables = variables;
        }
    }
}