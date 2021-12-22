using Main.Core.Documents;
using System;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IDesignerQuestionnaireStorage
    {
        QuestionnaireDocument? Get(QuestionnaireRevision questionnaire);
        QuestionnaireDocument? Get(Guid questionnaireId);
    }
}
