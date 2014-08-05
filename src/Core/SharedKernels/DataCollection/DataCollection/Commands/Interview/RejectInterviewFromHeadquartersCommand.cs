using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "RejectInterviewFromHeadquarters")]
    public class RejectInterviewFromHeadquartersCommand : InterviewCommand
    {
        public Guid? InterviewerId { get; set; }
        public Guid SupervisorId { get; set; }
        public InterviewSynchronizationDto InterviewDto { get; set; }
        public DateTime SynchronizationTime { get; set; }

        public RejectInterviewFromHeadquartersCommand(Guid interviewId, 
            Guid userId, 
            Guid supervisorId, 
            Guid? interviewerId,
            InterviewSynchronizationDto interviewDto, 
            DateTime synchronizationTime)
            : base(interviewId, userId)
        {
            this.InterviewerId = interviewerId;
            this.SupervisorId = supervisorId;
            this.InterviewDto = interviewDto;
            this.SynchronizationTime = synchronizationTime;
        }
    }
}