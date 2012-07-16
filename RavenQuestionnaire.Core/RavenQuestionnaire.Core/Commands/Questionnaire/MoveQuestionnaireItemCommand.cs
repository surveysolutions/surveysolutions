using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "MoveQuestion")]
    public class MoveQuestionnaireItemCommand : CommandBase
    {
        [AggregateRootId]
        public string QuestionnaireId { get; set; }
        public Guid PublicKey { get; set; }
        public Guid? GroupKey { get; set; }
        public Guid? AfterItemKey { get; set; }

        public MoveQuestionnaireItemCommand(string questionnaireId, Guid publicKey, Guid? group, Guid? afterItem)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.PublicKey = publicKey;
            this.AfterItemKey = afterItem;
            this.GroupKey = group;
        }
    }
}
