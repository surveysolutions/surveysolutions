using Main.Core.Documents;
using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    public class PasteInto : QuestionnaireEntityAddCommand
    {
        public PasteInto(Guid questionnaireId, Guid entityId, Guid sourceQuestionnaireId, Guid sourceItemId, Guid parentId, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId, parentId: parentId)
        {
            this.SourceQuestionnaireId = sourceQuestionnaireId;
            this.SourceItemId = sourceItemId;
        }

        public Guid SourceQuestionnaireId { get; private set; }
        public Guid SourceItemId { get; private set; }

        public QuestionnaireDocument? SourceDocument { get; set; }
    }
}

