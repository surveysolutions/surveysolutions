using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewPropertiesInvariants
    {
        public InterviewPropertiesInvariants(InterviewEntities.InterviewProperties interviewProperties)
        {
            this.InterviewProperties = interviewProperties;
        }

        public InterviewEntities.InterviewProperties InterviewProperties { get; }

        public void ThrowIfOtherInterviewerIsResponsible(Guid userId)
        {
            if (this.InterviewProperties.InterviewerId.HasValue &&
                userId != this.InterviewProperties.InterviewerId.Value)
                throw new InterviewException(
                    $"Interviewer with id {userId.FormatGuid()} is not responsible for the interview anymore, interviewer with id {this.InterviewProperties.InterviewerId} is.",
                    InterviewDomainExceptionType.OtherUserIsResponsible);
        }
    }
}