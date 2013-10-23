using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            return new[]
            {
                new QuestionnaireVerificationError("code", "message"),
            };
        }
    }
}