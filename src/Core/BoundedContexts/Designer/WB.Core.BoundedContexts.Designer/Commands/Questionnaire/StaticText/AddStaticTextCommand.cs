using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "AddStaticText")]
    public class AddStaticTextCommand : UpdateStaticTextCommand
    {
        public AddStaticTextCommand(Guid questionnaireId, Guid entityId, string text, Guid responsibleId, Guid parentId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId, text: text)
        {
            this.ParentId = parentId;
        }

        public Guid ParentId { get; set; }
    }
}
