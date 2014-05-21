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
        public long? QuestionnaireVersion { get; private set; }
        public Guid SupervisorId { get; private set; }
        public DateTime AnswersTime { get; private set; }

        public InterviewStatus InterviewStatus { get; set; }
        public AnsweredQuestionSynchronizationDto[] FeaturedQuestionsMeta { get; set; }
        public string Comments { get; set; }
        public bool Valid { get; set; }

        public CreateInterviewCreatedOnClientCommand(Guid interviewId, Guid userId, Guid questionnaireId, long? questionnaireVersion,
            DateTime answersTime, Guid supervisorId, InterviewStatus status,
            AnsweredQuestionSynchronizationDto[] featuredQuestionsMeta, string comments, bool valid)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.AnswersTime = answersTime;
            this.SupervisorId = supervisorId;
            this.QuestionnaireVersion = questionnaireVersion;
            this.FeaturedQuestionsMeta = featuredQuestionsMeta;
            this.InterviewStatus = status;
            this.Comments = comments;
            this.Valid = valid;
        }
    }
}
