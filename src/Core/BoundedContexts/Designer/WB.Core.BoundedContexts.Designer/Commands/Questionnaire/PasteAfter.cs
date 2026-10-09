using Main.Core.Documents;
using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class PasteAfter : QuestionnaireEntityCommand
    {
        public PasteAfter(Guid questionnaireId, Guid entityId, Guid itemToPasteAfterId, Guid sourceQuestionnaireId, Guid sourceItemId, Guid responsibleId, Guid? sourceQuestionnaireRevisionId = null)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            this.SourceQuestionnaireRevision = new QuestionnaireRevision(sourceQuestionnaireId, sourceQuestionnaireRevisionId);
            this.SourceItemId = sourceItemId;
            this.ItemToPasteAfterId = itemToPasteAfterId;
        }

        public Guid SourceQuestionnaireId => this.SourceQuestionnaireRevision?.QuestionnaireId ?? Guid.Empty;

        private QuestionnaireRevision? _sourceQuestionnaireRevision;
        public QuestionnaireRevision? SourceQuestionnaireRevision
        {
            get => _sourceQuestionnaireRevision;
            set { if (value != null) _sourceQuestionnaireRevision = value; }
        }

        public Guid SourceItemId { get; private set; }

        public Guid ItemToPasteAfterId { get; private set; }

        public QuestionnaireDocument? SourceDocument { get; set; }
    }
}
