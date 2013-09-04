using System;
using System.Collections.Generic;
using Main.Core.Domain;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethodOrConstructor(typeof(Implementation.Aggregates.Interview), "ApplySynchronizationMetadata")]
    public class ApplySynchronizationMetadata : InterviewCommand
    {
        public ApplySynchronizationMetadata(Guid interviewId, Guid questionnaireId, Guid userId,
                                              InterviewStatus status, AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            QuestionnaireId = questionnaireId;
            InterviewStatus = status;
            FeaturedQuestionsMeta = featuredQuestionsMeta;
        }
        public Guid Id { get; set; }

        public Guid QuestionnaireId { get; set; }

        public InterviewStatus InterviewStatus { get; set; }

        public AnsweredQuestionSynchronizationDto[] FeaturedQuestionsMeta { get; set; }
    }
}
