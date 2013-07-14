using System.Collections.Generic;
using Main.Core.Documents;

namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "CreateInterviewWithFeaturedQuestions")]
    public class CreateInterviewWithFeaturedQuestionsCommand : CommandBase
    {
        public CreateInterviewWithFeaturedQuestionsCommand(Guid interviewId, Guid questionnaireId, UserLight creator, UserLight responsible, List<QuestionAnswer> featuredAnswers)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.Creator = creator;
            this.Responsible = responsible;
            this.FeaturedAnswers = featuredAnswers;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public Guid InterviewId { get; set; }

        public UserLight Creator { get; set; }

        public UserLight Responsible { get; set; }
        
        public List<QuestionAnswer> FeaturedAnswers { get; set; }
    }
}