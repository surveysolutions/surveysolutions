using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootConstructor(typeof (Implementation.Aggregates.Interview))]
    public class CreateInterviewCreatedOnClientCommand : InterviewCommand
    {
        public Guid Id { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        public long QuestionnaireVersion { get; private set; }

        public InterviewStatus InterviewStatus { get; set; }
        public AnsweredQuestionSynchronizationDto[] FeaturedQuestionsMeta { get; set; }
        public bool IsValid { get; set; }

        public CreateInterviewCreatedOnClientCommand(Guid interviewId, Guid userId, Guid questionnaireId,
            long questionnaireVersion, InterviewStatus status, AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, bool isValid)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.FeaturedQuestionsMeta = featuredQuestionsMeta;
            this.InterviewStatus = status;
            this.IsValid = isValid;
        }
    }
}
