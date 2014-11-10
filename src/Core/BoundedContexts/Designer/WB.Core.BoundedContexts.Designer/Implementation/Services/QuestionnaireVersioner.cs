using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVersioner : IQuestionnaireVersioner
    {
        public QuestionnaireVersion GetVersion(QuestionnaireDocument questionnaire)
        {
            return QuestionnaireVersionProvider.GetCurrentEngineVersion();
        }
    }
}