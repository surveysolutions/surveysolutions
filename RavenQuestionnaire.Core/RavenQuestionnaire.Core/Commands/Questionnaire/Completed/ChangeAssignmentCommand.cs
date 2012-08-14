using System;
using Ncqrs.Commanding;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    /// <summary>
    /// Command to change responsible person
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "ChangeAssignment")]
    public class ChangeAssignmentCommand : CommandBase
    {
        public ChangeAssignmentCommand(Guid completeQuestionnaireId, UserLight responsible)
        {
            Responsible = responsible;
            CompleteQuestionnaireId = completeQuestionnaireId;
        }

        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        public UserLight Responsible { get; set; }

    }
}
