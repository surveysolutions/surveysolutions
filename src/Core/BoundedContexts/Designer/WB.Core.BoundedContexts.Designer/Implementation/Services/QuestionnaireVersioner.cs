using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVersioner : IQuestionnaireVersioner
    {
        public QuestionnaireVersion GetVersion(QuestionnaireDocument questionnaire)
        {
            return new QuestionnaireVersion(1, 6, 0);
        }
    }
}