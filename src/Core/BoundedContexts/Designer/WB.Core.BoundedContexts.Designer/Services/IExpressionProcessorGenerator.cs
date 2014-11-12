using System.Collections.Generic;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IExpressionProcessorGenerator
    {
        GenerationResult GenerateProcessorStateAssembly(QuestionnaireDocument questionnaire, out string generatedAssembly);
        Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireDocument questionnaire);
        string GenerateProcessorStateSingleClass(QuestionnaireDocument questionnaire);

    }
}
