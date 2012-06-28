using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(CompleteQuestionnaireAR), "AddPropagatableGroup")]
    public class AddPropagatableGroupCommand : CommandBase
    {
        [AggregateRootId]
        public Guid CompleteQuestionnaireId { get; set; }

        public Guid PublicKey { get; set; }
        public Guid PropagationKey { get; set; }


        public AddPropagatableGroupCommand(Guid completeQuestionnaireId, Guid propagationKey, Guid publicKey)
        {
            CompleteQuestionnaireId = completeQuestionnaireId;
            PublicKey = publicKey;
            PropagationKey = propagationKey;
        }

    }
}
