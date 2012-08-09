using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Completed
{
    /// <summary>
    /// Command to change responsible person
    /// </summary>
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "ChangeAssignment")]
    public class ChangeAssignmentCommand : CommandBase
    {
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        public UserLight UserPublicKey { get; set; }

        public ChangeAssignmentCommand(Guid completeQuestionnaireId, UserLight userPublicKey)
        {
            this.CompleteQuestionnaireId = completeQuestionnaireId;
            this.UserPublicKey = userPublicKey;
        }
    }
}
