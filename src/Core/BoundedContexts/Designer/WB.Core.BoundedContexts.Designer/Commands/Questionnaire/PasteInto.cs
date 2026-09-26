using Main.Core.Documents;
using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class PasteInto : QuestionnaireEntityAddCommand
    {
        public PasteInto(Guid questionnaireId, Guid entityId, Guid sourceQuestionnaireId, Guid sourceItemId, Guid parentId, Guid responsibleId, Guid? sourceQuestionnaireRevisionId = null)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId, parentId: parentId)
        {
            this.SourceQuestionnaireRevision = new QuestionnaireRevision(sourceQuestionnaireId, sourceQuestionnaireRevisionId);
            this.SourceItemId = sourceItemId;
        }

        public Guid SourceQuestionnaireId => this.SourceQuestionnaireRevision?.QuestionnaireId ?? Guid.Empty;

        private QuestionnaireRevision? _sourceQuestionnaireRevision;
        public QuestionnaireRevision? SourceQuestionnaireRevision
        {
            get => _sourceQuestionnaireRevision;
            set { if (value != null) _sourceQuestionnaireRevision = value; }
        }

        public Guid SourceItemId { get; private set; }

        public QuestionnaireDocument? SourceDocument { get; set; }
    }
}
