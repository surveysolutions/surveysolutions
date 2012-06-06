using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    [Serializable]
    [MapsToAggregateRootConstructor(typeof(CompleteQuestionnaireAR))]
    public class CreateCompleteQuestionnaireCommand : CommandBase
    {
        public CreateCompleteQuestionnaireCommand(){}

        public Guid CompleteQuestionnaireId
        {
            get; set;
        }
        public string QuestionnaireId
        {
            get;
            set;
        }

        public CreateCompleteQuestionnaireCommand(Guid completedQuestionnaireId, string questionnaireId)
            : base(completedQuestionnaireId)
        {
            CompleteQuestionnaireId = completedQuestionnaireId;
            QuestionnaireId = questionnaireId;
        }

    }
}
