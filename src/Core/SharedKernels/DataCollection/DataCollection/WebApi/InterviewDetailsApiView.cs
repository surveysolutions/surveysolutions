using System;
using System.Collections.Generic;
using Main.Core.Events;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Views.AuditLog;

namespace WB.Core.SharedKernels.DataCollection.WebApi
{
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

    public class InterviewDetailsApiView
    {
        public List<InterviewAnswerOnPrefilledQuestionApiView> AnswersOnPrefilledQuestions { get; set; }
        public string LastSupervisorOrInterviewerComment { get; set; }
        public DateTime? RejectedDateTime { get; set; }
        public DateTime? InterviewerAssignedDateTime { get; set; }

        public List<InterviewAnswerApiView> Answers { get; set; }

        public List<IdentityApiView> DisabledGroups { get; set; }
        public List<IdentityApiView> DisabledQuestions { get; set; }
        public List<IdentityApiView> ValidAnsweredQuestions { get; set; }
        public List<IdentityApiView> InvalidAnsweredQuestions { get; set; }

        public List<RosterApiView> RosterGroupInstances { get; set; }

        public bool WasCompleted { get; set; }

        public string FailedValidationConditions { get; set; }
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

    public class AuditLogEntityApiView
    {
        public int Id { get; set; }

        public AuditLogEntityType Type { get; set; }

        public DateTime Time { get; set; }
        public DateTime TimeUtc { get; set; }

        public Guid? ResponsibleId { get; set; }
        public string ResponsibleName { get; set; }

        public IAuditLogEntity Payload { get; set; }
    }
}
