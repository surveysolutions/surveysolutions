using Main.Core.Documents;
using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class PasteAfter : QuestionnaireEntityCommand
    {
        public PasteAfter(Guid questionnaireId, Guid entityId, Guid itemToPasteAfterId, Guid sourceQuestionnaireId, Guid sourceItemId, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.SourceQuestionnaireId = sourceQuestionnaireId;
            this.SourceItemId = sourceItemId;
            this.ItemToPasteAfterId = itemToPasteAfterId;
        }

        public Guid SourceQuestionnaireId { get; private set; }
        public Guid SourceItemId { get; private set; }

        public Guid ItemToPasteAfterId { get; private set; }

        public QuestionnaireDocument? SourceDocument { get; set; }
    }
}
