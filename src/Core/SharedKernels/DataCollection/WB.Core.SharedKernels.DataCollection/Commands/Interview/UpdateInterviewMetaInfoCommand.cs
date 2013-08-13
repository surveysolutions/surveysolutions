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
    [MapsToAggregateRootMethodOrConstructor(typeof(Implementation.Aggregates.Interview), "UpdateInterviewMetaInfo")]
    public class UpdateInterviewMetaInfoCommand : InterviewCommand
    {
        public UpdateInterviewMetaInfoCommand(Guid interviewId, Guid questionnarieId, Guid userId,
                                              InterviewStatus status, List<AnsweredQuestionSynchronizationDto> featuredQuestionsMeta)
            : base(interviewId, userId)
        {
            QuestionnarieId = questionnarieId;
            InterviewStatus = status;
            FeaturedQuestionsMeta = featuredQuestionsMeta;
        }

        public Guid QuestionnarieId { get; set; }

        public InterviewStatus InterviewStatus { get; set; }

        public List<AnsweredQuestionSynchronizationDto> FeaturedQuestionsMeta { get; set; }
    }
}
