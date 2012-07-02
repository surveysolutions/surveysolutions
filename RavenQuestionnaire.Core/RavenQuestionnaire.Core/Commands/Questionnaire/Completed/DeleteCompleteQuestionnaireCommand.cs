using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "Delete")]
    public class DeleteCompleteQuestionnaireCommand : CommandBase
    {
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }
        public DeleteCompleteQuestionnaireCommand(Guid completeQuestionnaireId)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
        }
    }
}
