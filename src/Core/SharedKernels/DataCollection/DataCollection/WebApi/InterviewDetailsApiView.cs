using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Events;
using Ncqrs.Eventing;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views.InterviewerAuditLog;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
    public class ObsoletePackageCheck
    {
        public Guid InterviewId { get; set; }

        public int SequenceOfLastReceivedEvent { get; set; }
    }

    public class EventStreamSignatureTag
    {
        public Guid FirstEventId { get; set; }
        public Guid LastEventId { get; set; }

        public DateTime FirstEventTimeStamp { get; set; }
        public DateTime LastEventTimeStamp { get; set; }
    }

    public class InterviewPackageApiView
    {
        public Guid InterviewId { get; set; }
        public InterviewMetaInfo MetaInfo { get; set; }
        public string Events { get; set; }
        public bool FullEventStreamRequested { get; set; }
    }

    public class InterviewSyncInfoPackage
    {
        public Guid FirstEventId { get; set; }
        public Guid? LastEventIdFromPreviousSync { get; set; }
    }

    public class SyncInfoPackageResponse
    {
        public bool HasInterview { get; set; } 
        public bool NeedSendFullStream { get; set; } 
    }

    public class InterviewPackageContainer
    {
        public InterviewPackageContainer(Guid interviewId, IReadOnlyCollection<CommittedEvent> events)
        {
            InterviewId = interviewId;
            Events = events;

            if (events.Count == 0)
                Tag = null;
            else
            {
                var first = events.First();
                var last = events.Last();

                Tag = new EventStreamSignatureTag
                {
                    FirstEventId = first.EventIdentifier,
                    LastEventId = last.EventIdentifier,

                    FirstEventTimeStamp = first.EventTimeStamp,
                    LastEventTimeStamp = last.EventTimeStamp
                };
            }
        }

        public Guid InterviewId { get; }
        public EventStreamSignatureTag Tag { get; }

        public IReadOnlyCollection<CommittedEvent> Events { get; }
    }


    public class InterviewerInterviewApiView
    {
        public AnsweredQuestionSynchronizationDto[] AnswersOnPrefilledQuestions { get; set; }
        public string Details { get; set; }
    }

    public class LogonInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    
    public class ChangePasswordInfo
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
    }

    public class InterviewAnswerApiView
    {
        public Guid QuestionId { get; set; }

        public decimal[] QuestionRosterVector { get; set; }

        public string JsonAnswer { get; set; }
        public string LastSupervisorOrInterviewerComment { get; set; }
    }

    public class AuditLogEntitiesApiView
    {
        public bool IsWorkspaceSupported { get; set; } 
        public AuditLogEntityApiView[] Entities { get; set; }
    }

    public class AuditLogEntityApiView
    {
        public int Id { get; set; }

        public AuditLogEntityType Type { get; set; }

        public DateTimeOffset Time { get; set; }
        public DateTimeOffset TimeUtc { get; set; }

        public Guid? ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }

        public string PayloadType { get; set; }
        public string Payload { get; set; }
        public string Workspace { get; set; }
    }
}
