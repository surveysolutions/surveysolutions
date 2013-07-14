using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "DeleteQuestionnaire")]
    public class DeleteQuestionnaireCommand : CommandBase
    {
        public DeleteQuestionnaireCommand(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }

        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
    }
}