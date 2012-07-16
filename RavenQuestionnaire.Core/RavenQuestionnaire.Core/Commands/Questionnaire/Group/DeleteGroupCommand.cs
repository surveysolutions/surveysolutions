using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "DeleteGroup")]
    public class DeleteGroupCommand : CommandBase
    {
        public Guid GroupPublicKey { get; set; }
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }

        public DeleteGroupCommand(Guid groupPublicKey, Guid questionnaireId)
        {
            this.GroupPublicKey = groupPublicKey;
            this.QuestionnaireId = questionnaireId;
        }
    }
}
