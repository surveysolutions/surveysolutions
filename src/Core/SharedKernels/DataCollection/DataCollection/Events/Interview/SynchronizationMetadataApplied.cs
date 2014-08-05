using System;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class SynchronizationMetadataApplied : InterviewActiveEvent
    {
        public SynchronizationMetadataApplied(Guid userId, Guid questionnaireId, InterviewStatus status,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, bool createdOnClient, string comments)
            : base(userId)
        {
            this.Comments = comments;
            this.QuestionnaireId = questionnaireId;
            this.Status = status;
            this.FeaturedQuestionsMeta = featuredQuestionsMeta;
            this.CreatedOnClient = createdOnClient;
        }

        public Guid QuestionnaireId { get; private set; }

        public InterviewStatus Status { get; private set; }

        public AnsweredQuestionSynchronizationDto[] FeaturedQuestionsMeta { get; private set; }

        public bool CreatedOnClient { get; private set; }

        public string Comments { get; private set; }
    }
}
