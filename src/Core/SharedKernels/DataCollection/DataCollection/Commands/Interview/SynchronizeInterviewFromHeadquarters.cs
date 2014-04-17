using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethodOrConstructor(typeof (Implementation.Aggregates.Interview), "SynchronizeInterviewFromHeadquarters")]
    public class SynchronizeInterviewFromHeadquarters : InterviewCommand
    {
        public Guid Id { get; set; }
        public InterviewSynchronizationDto InterviewDto { get; set; }
        public DateTime SynchronizationTime { get; set; }

        public SynchronizeInterviewFromHeadquarters(Guid interviewId, Guid userId, InterviewSynchronizationDto interviewDto, DateTime synchronizationTime)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.InterviewDto = interviewDto;
            this.SynchronizationTime = synchronizationTime;
        }
    }
}