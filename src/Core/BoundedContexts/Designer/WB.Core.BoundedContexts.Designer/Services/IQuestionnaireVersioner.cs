using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IQuestionnaireVersioner
    {
        QuestionnaireVersion GetVersion(QuestionnaireDocument questionnaire);
    }
}