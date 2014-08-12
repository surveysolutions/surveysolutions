using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "UpdateStaticText")]
    public class UpdateStaticTextCommand : QuestionnaireEntityCommand
    {
        public UpdateStaticTextCommand(Guid questionnaireId, Guid entityId, string text, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.Text = CommandUtils.SanitizeHtml(text);
        }

        public string Text { get; set; }
    }
}
