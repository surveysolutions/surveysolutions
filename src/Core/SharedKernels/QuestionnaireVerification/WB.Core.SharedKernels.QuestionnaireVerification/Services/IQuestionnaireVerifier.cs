using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Services
{
    public interface IQuestionnaireVerifier
    {
        IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire);
    }
}