using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof (Aggregates.Questionnaire), "CloneStaticText")]
    public class CloneStaticTextCommand : QuestionnaireEntityCloneCommand
    {
        public CloneStaticTextCommand(Guid questionnaireId, Guid entityId, string text, Guid responsibleId,
            Guid parentId, Guid sourceEntityId, int targetIndex)
            : base(
                responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId, parentId: parentId,
                sourceEntityId: sourceEntityId, targetIndex: targetIndex)
        {
            this.Text = text;
        }

        public string Text { get; set; }
    }
}