using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewProperties
    {
        private Guid? interviewerId;

        public string Id { get; set; }

        public Guid? InterviewerId
        {
            get { return this.interviewerId; }
            set { this.interviewerId = value.NullIfEmpty(); }
        }

        public InterviewStatus Status { get; set; }
        public bool IsReceivedByInterviewer { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsHardDeleted { get; set; }
    }
}
