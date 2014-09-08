using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "AddStaticText")]
    public class AddStaticTextCommand : QuestionnaireEntityAddCommand
    {
        public AddStaticTextCommand(Guid questionnaireId, Guid entityId, string text, Guid responsibleId, Guid parentId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId, parentId: parentId)
        {
            this.Text = CommandUtils.SanitizeHtml(text);
        }

        public string Text { get; set; }
    }
}
