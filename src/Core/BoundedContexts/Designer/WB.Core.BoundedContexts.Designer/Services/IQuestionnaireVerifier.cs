using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireVerifier
    {
        IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire);
    }
}