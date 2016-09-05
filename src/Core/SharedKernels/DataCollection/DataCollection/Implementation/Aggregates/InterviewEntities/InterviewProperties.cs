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
            bool wasCompleted, bool wasHardDeleted, bool receivedByInterviewer)
        {
            this.Id = id;
            this.InterviewerId = interviewerId != Guid.Empty ? interviewerId : null as Guid?;
            this.Status = status;
            this.WasCompleted = wasCompleted;
            this.WasHardDeleted = wasHardDeleted;
            this.ReceivedByInterviewer = receivedByInterviewer;
        }

        public Guid Id { get; }
        public Guid? InterviewerId { get; }
        public InterviewStatus Status { get; }
        public bool WasCompleted { get; }
        public bool WasHardDeleted { get; }
        public bool ReceivedByInterviewer { get; }
    }
}
