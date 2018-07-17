using System;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.Invariants
{
    internal class InterviewPropertiesInvariants
    {
        static class ExceptionKeys
        {
            public static readonly string UserId = "UserId";
            public static readonly string InterviewId = "InterviewId";
            public static readonly string SupervisorId = "SupervisorId";
            public static readonly string InterviewerId = "InterviewerId";
        }

        public InterviewPropertiesInvariants(InterviewEntities.InterviewProperties interviewProperties)
        {
            this.InterviewProperties = interviewProperties;
        }

        private InterviewEntities.InterviewProperties InterviewProperties { get; }

        public void RequireAnswerCanBeChanged()
        {
            this.ThrowIfInterviewHardDeleted();
            this.ThrowIfInterviewApprovedByHQ();
        }

        public void ThrowIfOtherInterviewerIsResponsible(Guid userId)
        {
            if (userId != this.InterviewProperties.InterviewerId)
                throw new InterviewException(
                    $"Interviewer is not responsible for the interview anymore",
                    InterviewDomainExceptionType.OtherUserIsResponsible) 
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewProperties.Id},
                        {ExceptionKeys.UserId, userId},
                        {ExceptionKeys.InterviewerId, this.InterviewProperties.InterviewerId}
                    }
                };
        }

        public void ThrowIfInterviewApprovedByHQ()
        {
            if (this.InterviewProperties.Status == InterviewStatus.ApprovedByHeadquarters)
                throw new InterviewException(
                    $"Interview was approved by Headquarters and cannot be edited")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewProperties.Id},
                    }
                };
        }

        public void ThrowIfInterviewWasCompleted()
        {
            if (this.InterviewProperties.WasCompleted)
                throw new InterviewException(
                    $"Interview was completed by interviewer and cannot be deleted")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewProperties.Id},
                    }
                };
        }

        public void ThrowIfInterviewStatusIsNotOneOfExpected(params InterviewStatus[] expectedStatuses)
        {
            if (!expectedStatuses.Contains(this.InterviewProperties.Status))
                throw new InterviewException(
                    $"Interview status is {this.InterviewProperties.Status}. But one of the following statuses was expected: {string.Join(", ", expectedStatuses.Select(expectedStatus => expectedStatus.ToString()))}", InterviewDomainExceptionType.StatusIsNotOneOfExpected)
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewProperties.Id},
                    }
                };
        }

        public void ThrowIfTryAssignToSameInterviewer(Guid interviewerIdToAssign)
        {
            if (this.InterviewProperties.Status == InterviewStatus.InterviewerAssigned && this.InterviewProperties.InterviewerId == interviewerIdToAssign)
                throw new InterviewException(
                    $"Interview has assigned on this interviewer already")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewProperties.Id},
                        {ExceptionKeys.InterviewerId, this.InterviewProperties.InterviewerId}
                    }
                };
        }

        public void ThrowIfTryAssignToSameSupervisor(Guid supervisorIdToAssign)
        {
            if ((this.InterviewProperties.Status == InterviewStatus.SupervisorAssigned ||
                 this.InterviewProperties.Status == InterviewStatus.RejectedBySupervisor)
                && this.InterviewProperties.SupervisorId == supervisorIdToAssign)
                throw new InterviewException(
                    $"Interview has assigned on this supervisor already")
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewProperties.Id},
                        {
                            ExceptionKeys.SupervisorId, this.InterviewProperties.SupervisorId
                        }
                    }
                };
        }

        public void ThrowIfInterviewHardDeleted()
        {
            if (this.InterviewProperties.IsHardDeleted)
                throw new InterviewException(
                    $"Interview status is hard deleted",
                    InterviewDomainExceptionType.InterviewHardDeleted)
                {
                    Data =
                    {
                        {ExceptionKeys.InterviewId, this.InterviewProperties.Id}
                    }
                };
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
                        $"Status {interviewStatus} not allowed to be changed with ApplySynchronizationMetadata command",
                        InterviewDomainExceptionType.StatusIsNotOneOfExpected){
                        Data =
                        {
                            {ExceptionKeys.InterviewId, this.InterviewProperties.Id},
                        }
                    };
            }
        }
    }
}
