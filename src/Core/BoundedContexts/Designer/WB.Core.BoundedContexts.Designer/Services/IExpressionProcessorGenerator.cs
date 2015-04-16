using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IExpressionProcessorGenerator
    {
        GenerationResult GenerateProcessorStateAssemblyForVersion(QuestionnaireDocument questionnaire, EngineVersion version,
          out string generatedAssembly);

        Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireDocument questionnaire);
        string GenerateProcessorStateSingleClass(QuestionnaireDocument questionnaire);
    }
}
