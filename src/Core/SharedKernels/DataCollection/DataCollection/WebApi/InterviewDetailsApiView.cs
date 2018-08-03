using System;
using System.Collections.Generic;
using Main.Core.Events;
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

    public class InterviewPackageApiView
    {
        public Guid InterviewId { get; set; }
        public InterviewMetaInfo MetaInfo { get; set; }
        public string Events { get; set; }
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

    public class InterviewAnswerApiView
    {
        public Guid QuestionId { get; set; }

        public decimal[] QuestionRosterVector { get; set; }

        public string JsonAnswer { get; set; }
        public string LastSupervisorOrInterviewerComment { get; set; }
    }

    public class AuditLogEntitiesApiView
    {
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
    }
}
