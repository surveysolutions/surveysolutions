using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities
{
    public class InterviewProperties
    {
        private Guid? interviewerId;

        public InterviewProperties()
        {
            this.IsValid = true;
        }

        public string Id { get; set; }

        public Guid? InterviewerId
        {
            get { return this.interviewerId; }
            set { this.interviewerId = value.NullIfEmpty(); }
        }

        public InterviewStatus Status { get; set; }
        public bool IsReceivedByInterviewer { get; set; }
        public bool WasCompleted { get; set; }
        public bool WasRejected { get; set; }
        public bool IsHardDeleted { get; set; }
        public DateTime? StartedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? RejectDateTime { get; set; }
        public DateTime? InterviewerAssignedDateTime { get; set; }
        public int? AssignmentId { get; set; }

        private Guid? supervisorId;
        private DateTime? lastPausedUtc;
        private DateTime? lastResumedUtc;
        private DateTime? lastOpenedBySupervisor;
        private DateTime? lastClosedBySupervisor;
        private DateTime? lastAnswerDateUtc;

        public Guid? SupervisorId
        {
            get => this.supervisorId;
            set => this.supervisorId = value.NullIfEmpty();
        }

        public bool IsValid { get; set; }

        public bool? IsAudioRecordingEnabled { get; set; }
        public bool WasCreated { get; set; }

        public DateTime? LastPausedUtc
        {
            get => lastPausedUtc;
            set
            {
                if (value != null && value.Value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("Cannot assign non utc date in utc field");
                }
                
                lastPausedUtc = value;
            }
        }

        public DateTime? LastResumedUtc
        {
            get => lastResumedUtc;
            set
            {
                if (value != null && value.Value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("Cannot assign non utc date in utc field");
                }
                
                lastResumedUtc = value;
            }
        }

        public DateTime? LastOpenedBySupervisor
        {
            get => lastOpenedBySupervisor;
            set
            {
                if (value != null && value.Value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("Cannot assign non utc date in utc field");
                }
                
                lastOpenedBySupervisor = value;
            }
        }

        public DateTime? LastClosedBySupervisor
        {
            get => lastClosedBySupervisor;
            set
            {
                if (value != null && value.Value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("Cannot assign non utc date in utc field");
                }
                
                lastClosedBySupervisor = value;
            }
        }

        public DateTime? LastAnswerDateUtc
        {
            get => lastAnswerDateUtc;
            set
            {
                if (value != null && value.Value.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("Cannot assign non utc date in utc field");
                }
                lastAnswerDateUtc = value;
            }
        }
    }
}
