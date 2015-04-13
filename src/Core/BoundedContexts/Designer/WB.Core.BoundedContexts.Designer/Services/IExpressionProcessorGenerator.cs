using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IExpressionProcessorGenerator
    {
        GenerationResult GenerateProcessorStateAssembly(QuestionnaireDocument questionnaire,
            out string generatedAssembly);

        GenerationResult GenerateProcessorStateAssemblyForVersion(QuestionnaireDocument questionnaire, QuestionnaireVersion version,
          out string generatedAssembly);

        Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireDocument questionnaire);
        string GenerateProcessorStateSingleClass(QuestionnaireDocument questionnaire);
    }
}
