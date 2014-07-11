using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "CloneStaticText")]
    public class CloneStaticTextCommand : AddStaticTextCommand
    {
        public CloneStaticTextCommand(Guid questionnaireId, Guid entityId, string text, Guid responsibleId,
            Guid parentId, Guid sourceEntityId, int targetIndex)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId, text: text,
                parentId: parentId)
        {
            this.SourceEntityId = sourceEntityId;
            this.TargetIndex = targetIndex;
        }

        public int TargetIndex { get; set; }
        public Guid SourceEntityId { get; set; }
    }
}
