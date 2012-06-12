using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "CreateCompletedQ")]
    public class CreateCompleteQuestionnaireCommand : CommandBase
    {
        [AggregateRootId]
        public Guid QuestionnaireId
        {
            get;
            set;
        }

        public Guid CompleteQuestionnaireId
        {
            get; 
            set;
        }

        public CreateCompleteQuestionnaireCommand(Guid completedQuestionnaireId, Guid questionnaireId)
        {
            CompleteQuestionnaireId = completedQuestionnaireId;
            QuestionnaireId = questionnaireId;
        }

    }
}
