using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewProperties
    {
        public InterviewProperties(Guid id, Guid interviewerId, InterviewStatus status,
            bool isReceivedByInterviewer, bool isCompleted, bool isHardDeleted)
        {
            this.Id = id.FormatGuid();
            this.InterviewerId = interviewerId != Guid.Empty ? interviewerId : null as Guid?;
            this.Status = status;
            this.IsReceivedByInterviewer = isReceivedByInterviewer;
            this.IsCompleted = isCompleted;
            this.IsHardDeleted = isHardDeleted;
        }

        public string Id { get; }
        public Guid? InterviewerId { get; }
        public InterviewStatus Status { get; }
        public bool IsReceivedByInterviewer { get; }
        public bool IsCompleted { get; }
        public bool IsHardDeleted { get; }
    }
}
