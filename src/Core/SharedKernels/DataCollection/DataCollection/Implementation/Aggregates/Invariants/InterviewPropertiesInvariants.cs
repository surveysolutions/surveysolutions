using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewPropertiesInvariants
    {
        public InterviewPropertiesInvariants(InterviewEntities.InterviewProperties interviewProperties)
        {
            this.InterviewProperties = interviewProperties;
        }

        private InterviewEntities.InterviewProperties InterviewProperties { get; }

        public void RequireAnswerCanBeChanged()
        {
            this.ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewApprovedByHQ();
            this.ThrowIfInterviewReceivedByInterviewer();
        }

        public void ThrowIfOtherInterviewerIsResponsible(Guid userId)
        {
            if (userId != this.InterviewProperties.InterviewerId)
                throw new InterviewException(
                    $"Interviewer with id {userId.FormatGuid()} is not responsible for the interview anymore, interviewer with id {this.InterviewProperties.InterviewerId} is.",
                    InterviewDomainExceptionType.OtherUserIsResponsible);
        }

        public void ThrowIfInterviewApprovedByHQ()
        {
            if (this.InterviewProperties.Status == InterviewStatus.ApprovedByHeadquarters)
                throw new InterviewException(
                    $"Interview was approved by Headquarters and cannot be edited. InterviewId: {this.InterviewProperties.Id}");
        }

        public void ThrowIfInterviewWasCompleted()
        {
            if (this.InterviewProperties.WasCompleted)
                throw new InterviewException(
                    $"Interview was completed by interviewer and cannot be deleted. InterviewId: {this.InterviewProperties.Id}");
        }

        public void ThrowIfInterviewStatusIsNotOneOfExpected(params InterviewStatus[] expectedStatuses)
        {
            if (!expectedStatuses.Contains(this.InterviewProperties.Status))
                throw new InterviewException(
                    $"Interview status is {this.InterviewProperties.Status}. But one of the following statuses was expected: {string.Join(", ", expectedStatuses.Select(expectedStatus => expectedStatus.ToString()))}. InterviewId: {this.InterviewProperties.Id}", InterviewDomainExceptionType.StatusIsNotOneOfExpected);
        }

        public void ThrowIfTryAssignToSameInterviewer(Guid interviewerIdToAssign)
        {
            if (this.InterviewProperties.Status == InterviewStatus.InterviewerAssigned && this.InterviewProperties.InterviewerId == interviewerIdToAssign)
                throw new InterviewException(
                    $"Interview has assigned on this interviewer already. InterviewId: {this.InterviewProperties.Id}, InterviewerId: {this.InterviewProperties.InterviewerId}");
        }

        public void ThrowIfInterviewHardDeleted()
        {
            if (this.InterviewProperties.IsHardDeleted)
                throw new InterviewException(
                    $"Interview {this.InterviewProperties.Id} status is hard deleted.",
                    InterviewDomainExceptionType.InterviewHardDeleted);
        }

        public void ThrowIfInterviewReceivedByInterviewer()
        {
            if (this.InterviewProperties.IsReceivedByInterviewer)
                throw new InterviewException(
                    $"Can't modify Interview {this.InterviewProperties.Id} on server, because it received by interviewer.");
        }

        public void ThrowIfStatusNotAllowedToBeChangedWithMetadata(InterviewStatus interviewStatus)
        {
            this.ThrowIfInterviewHardDeleted();
            switch (interviewStatus)
            {
                case InterviewStatus.Completed:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(InterviewStatus.InterviewerAssigned,
                        InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor,
                        InterviewStatus.Restarted);
                    return;
                case InterviewStatus.RejectedBySupervisor:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor,
                        InterviewStatus.Restarted,
                        InterviewStatus.Completed,
                        InterviewStatus.ApprovedBySupervisor);
                    return;
                case InterviewStatus.InterviewerAssigned:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.Restored,
                        InterviewStatus.RejectedBySupervisor,
                        InterviewStatus.SupervisorAssigned,
                        InterviewStatus.Restarted,
                        InterviewStatus.Completed);
                    return;
                case InterviewStatus.ApprovedBySupervisor:
                    this.ThrowIfInterviewStatusIsNotOneOfExpected(
                        InterviewStatus.InterviewerAssigned,
                        InterviewStatus.RejectedByHeadquarters,
                        InterviewStatus.SupervisorAssigned);
                    return;
                default:
                    throw new InterviewException(
                        $"Status {interviewStatus} not allowed to be changed with ApplySynchronizationMetadata command. InterviewId: {this.InterviewProperties.Id}",
                        InterviewDomainExceptionType.StatusIsNotOneOfExpected);
            }
        }
    }
}