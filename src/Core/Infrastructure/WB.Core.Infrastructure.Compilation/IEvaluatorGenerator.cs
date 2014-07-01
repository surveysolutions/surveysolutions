using Main.Core.Documents;

namespace WB.Core.Infrastructure.Compilation
{
    interface IEvaluatorGenerator
    {
        string GenerateEvaluator(QuestionnaireDocument questionnaire);
    }
}
