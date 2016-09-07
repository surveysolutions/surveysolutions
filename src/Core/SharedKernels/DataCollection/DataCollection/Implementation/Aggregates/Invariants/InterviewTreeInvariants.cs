using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewTreeInvariants
    {
        public InterviewTreeInvariants(InterviewTree interviewTree)
        {
            this.InterviewTree = interviewTree;
        }

        public InterviewTree InterviewTree { get; }
    }
}