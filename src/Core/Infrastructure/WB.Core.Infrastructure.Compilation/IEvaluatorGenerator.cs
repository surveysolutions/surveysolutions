using Main.Core.Documents;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.Infrastructure.Compilation
{
    interface IEvaluatorGenerator
    {
        EmitResult GenerateEvaluator(QuestionnaireDocument questionnaire, out string generatedAssembly);
    }
}
