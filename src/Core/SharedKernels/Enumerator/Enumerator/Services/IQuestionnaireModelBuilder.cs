using Main.Core.Documents;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface IQuestionnaireModelBuilder
    {
        QuestionnaireModel BuildQuestionnaireModel(QuestionnaireDocument questionnaireDocument);
    }
}