using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireVerifier
    {
        IEnumerable<QuestionnaireVerificationMessage> CheckForErrors(QuestionnaireDocument questionnaire);
        IEnumerable<QuestionnaireVerificationMessage> Verify(QuestionnaireDocument questionnaire);
    }
}