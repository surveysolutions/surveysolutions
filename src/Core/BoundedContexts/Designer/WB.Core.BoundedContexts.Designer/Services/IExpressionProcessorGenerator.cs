using Main.Core.Documents;
using Microsoft.CodeAnalysis.Emit;

namespace WB.Core.BoundedContexts.Designer.Services
{
    interface IExpressionProcessorGenerator
    {
        EmitResult GenerateProcessor(QuestionnaireDocument questionnaire, out string generatedAssembly);
    }
}
