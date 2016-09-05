using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    internal class InterviewProperties
    {
        public InterviewProperties(Guid id, Guid interviewerId, InterviewStatus status,
            bool isReceivedByInterviewer, bool isCompleted, bool isHardDeleted)
        {
            this.Id = id;
            this.InterviewerId = interviewerId != Guid.Empty ? interviewerId : null as Guid?;
            this.Status = status;
            this.IsReceivedByInterviewer = isReceivedByInterviewer;
            this.IsCompleted = isCompleted;
            this.IsHardDeleted = isHardDeleted;
        }

        public Guid Id { get; }
        public Guid? InterviewerId { get; }
        public InterviewStatus Status { get; }
        public bool IsReceivedByInterviewer { get; }
        public bool IsCompleted { get; }
        public bool IsHardDeleted { get; }
    }
}
