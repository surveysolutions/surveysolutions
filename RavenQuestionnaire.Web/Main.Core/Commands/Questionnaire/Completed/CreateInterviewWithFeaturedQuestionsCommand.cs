namespace Main.Core.Commands.Questionnaire.Completed
{
    using System;

    using Main.Core.Domain;
    using Main.Core.Entities.SubEntities;

    using Ncqrs.Commanding;
    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    /// <summary>
    /// The create complete questionnaire command.
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "CreateInterviewWithFeaturedQuestions")]
    public class CreateInterviewWithFeaturedQuestionsCommand : CommandBase
    {
        public CreateInterviewWithFeaturedQuestionsCommand(Guid interviewId, Guid questionnaireId, UserLight creator)
        {
            this.InterviewId = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.Creator = creator;
        }

        public Guid InterviewId { get; set; }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public UserLight Creator { get; set; }
    }
}