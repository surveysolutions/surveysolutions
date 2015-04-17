using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IExpressionProcessorGenerator
    {
        GenerationResult GenerateProcessorStateAssemblyForVersion(QuestionnaireDocument questionnaire, ExpressionsEngineVersion version,
          out string generatedAssembly);

        Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireDocument questionnaire);
        string GenerateProcessorStateSingleClass(QuestionnaireDocument questionnaire);
    }
}
