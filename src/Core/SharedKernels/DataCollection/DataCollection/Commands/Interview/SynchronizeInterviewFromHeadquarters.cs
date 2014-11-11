using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class SynchronizeInterviewFromHeadquarters : InterviewCommand
    {
        public Guid Id { get; set; }
        public Guid SupervisorId { get; set; }
        public InterviewSynchronizationDto InterviewDto { get; set; }
        public DateTime SynchronizationTime { get; set; }

        public SynchronizeInterviewFromHeadquarters(Guid interviewId, Guid userId, Guid supervisorId, InterviewSynchronizationDto interviewDto, DateTime synchronizationTime)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.SupervisorId = supervisorId;
            this.InterviewDto = interviewDto;
            this.SynchronizationTime = synchronizationTime;
        }
    }
}