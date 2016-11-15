using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireVerifier
    {
        IEnumerable<QuestionnaireVerificationMessage> CheckForErrors(QuestionnaireView questionnaireView);
        IEnumerable<QuestionnaireVerificationMessage> Verify(QuestionnaireView questionnaireView);
    }
}