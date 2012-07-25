using System;
using Ncqrs.Commanding;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Utility;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "MoveQuestion")]
    public class MoveQuestionnaireItemCommand : CommandBase
    {
        [AggregateRootId]
        public Guid QuestionnaireId { get; set; }
        public Guid PublicKey { get; set; }
        public Guid? GroupKey { get; set; }
        public Guid? AfterItemKey { get; set; }

        public MoveQuestionnaireItemCommand(Guid questionnaireId, Guid publicKey, Guid? groupKey, Guid? afterItemKey)
        {
            this.QuestionnaireId = questionnaireId;
            this.PublicKey = publicKey;
            this.AfterItemKey = afterItemKey;
            this.GroupKey = groupKey;
        }
    }
}
