using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireVersioner
    {
        QuestionnaireVersion GetVersion(QuestionnaireDocument questionnaire);
    }
}