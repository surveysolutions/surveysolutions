using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethodOrConstructor(typeof (Implementation.Aggregates.Interview), "ApplySynchronizationMetadata")]
    public class ApplySynchronizationMetadata : InterviewCommand
    {
        public ApplySynchronizationMetadata(Guid interviewId, Guid userId, Guid questionnaireId, InterviewStatus status,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, bool valid)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.InterviewStatus = status;
            this.FeaturedQuestionsMeta = featuredQuestionsMeta;
            this.Comments = comments;
            this.Valid = valid;
        }

        public Guid Id { get; set; }

        public Guid QuestionnaireId { get; set; }

        public InterviewStatus InterviewStatus { get; set; }

        public AnsweredQuestionSynchronizationDto[] FeaturedQuestionsMeta { get; set; }

        public string Comments { get; set; }

        public bool Valid { get; set; }
    }
}
