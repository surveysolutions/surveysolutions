using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SynchronizationMetadataApplied : InterviewActiveEvent
    {
        public SynchronizationMetadataApplied(Guid userId, Guid questionnaireId, long questionnaireVersion, InterviewStatus status,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, bool createdOnClient, string comments,
            DateTime? rejectedDateTime, DateTime? interviewerAssignedDateTime,
            DateTimeOffset originDate,
            bool usesExpressionStorage = false)
            : base(userId, originDate)
        {
            this.InterviewerAssignedDateTime = interviewerAssignedDateTime;
            this.Comments = comments;
            this.RejectedDateTime = rejectedDateTime;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.Status = status;
            this.FeaturedQuestionsMeta = featuredQuestionsMeta;
            this.CreatedOnClient = createdOnClient;
            this.UsesExpressionStorage = usesExpressionStorage;
        }

        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; set; }
        public InterviewStatus Status { get; private set; }

        public AnsweredQuestionSynchronizationDto[] FeaturedQuestionsMeta { get; private set; }

        public bool CreatedOnClient { get; private set; }

        public string Comments { get; private set; }
        public DateTime? RejectedDateTime { get; private set; }
        public DateTime? InterviewerAssignedDateTime { get; private set; }
        public bool UsesExpressionStorage { get; set; }
    }
}
