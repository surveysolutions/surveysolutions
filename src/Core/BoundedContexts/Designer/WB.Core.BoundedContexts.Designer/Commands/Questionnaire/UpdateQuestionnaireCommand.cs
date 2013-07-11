namespace Main.Core.Commands.Questionnaire
{
    using System;

    using Main.Core.Domain;

    using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "UpdateQuestionnaire")]
    public class UpdateQuestionnaireCommand : QuestionnaireCommand
    {
        public UpdateQuestionnaireCommand(Guid questionnaireId, string title, bool isPublic)
            : base(questionnaireId)
        {
            this.Title = title;
            this.IsPublic = isPublic;
        }

        public string Title { get; set; }

        public bool IsPublic { get; set; }
    }
}