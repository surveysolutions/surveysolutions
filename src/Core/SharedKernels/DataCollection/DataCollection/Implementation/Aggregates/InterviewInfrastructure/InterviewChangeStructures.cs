using System.Collections.Generic;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates
{
    internal class InterviewChangeStructures
    {
        public InterviewChangeStructures()
        {
            this.State = new InterviewStateDependentOnAnswers();
            this.Changes = new List<InterviewChanges>();
        }

        public InterviewStateDependentOnAnswers State { private set; get; }
        public List<InterviewChanges> Changes { private set; get; }
    }
}