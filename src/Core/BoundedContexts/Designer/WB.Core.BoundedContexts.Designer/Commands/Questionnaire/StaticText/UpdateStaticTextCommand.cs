using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "UpdateStaticText")]
    public class UpdateStaticTextCommand : QuestionnaireCommand
    {
        public UpdateStaticTextCommand(Guid questionnaireId, Guid entityId, string text, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId)
        {
            this.EntityId = entityId;
            this.Text = text;
        }

        public Guid EntityId { get; set; }
        public string Text { get; set; }
    }
}
