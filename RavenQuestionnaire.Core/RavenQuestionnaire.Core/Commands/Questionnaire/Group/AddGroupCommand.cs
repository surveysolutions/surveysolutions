using System;
using Ncqrs.Commanding;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using RavenQuestionnaire.Core.Domain;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(QuestionnaireAR), "AddGroup")]
    public class AddGroupCommand : CommandBase 
    {
        [AggregateRootId]
        public Guid QuestionnaireId
        {
            get; set;
        }

        public Guid PublicKey { get; set; }

        public string Text { get; set; }

        public Propagate Propagateble { get; set; }

        public Guid? ParentGroupKey { get; set; }

        public AddGroupCommand(Guid questionnaireId, Guid publicKey,string text, Propagate propagate, Guid? parentGroupKey)
        {
            QuestionnaireId = questionnaireId;
            PublicKey = publicKey;
            Text = text;
            Propagateble = propagate;
            ParentGroupKey = parentGroupKey;
        }
    }
}
