using Main.Core.Documents;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireVersioner
    {
        QuestionnaireVersion GetVersion(QuestionnaireDocument questionnaire);
    }
}