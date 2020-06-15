﻿using System;
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

        public Guid? SupervisorId
        {
            get => this.supervisorId;
            set => this.supervisorId = value.NullIfEmpty();
        }

        public bool IsValid { get; set; }

        public bool? IsAudioRecordingEnabled { get; set; }
        public bool WasCreated { get; set; }

        public DateTimeOffset? LastPaused { get; set; }

        public DateTimeOffset? LastResumed { get; set; }

        public DateTimeOffset? LastOpenedBySupervisor { get; set; }

        public DateTimeOffset? LastClosedBySupervisor { get; set; }

        public DateTimeOffset? LastAnswerDateUtc { get; set; }
    }
}
