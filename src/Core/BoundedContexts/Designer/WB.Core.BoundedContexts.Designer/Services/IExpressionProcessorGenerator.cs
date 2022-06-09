using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface IExpressionProcessorGenerator
    {
        GenerationResult GenerateProcessorStateAssembly(QuestionnaireCodeGenerationPackage package, int targetVersion,
          out string generatedAssembly);
        Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireCodeGenerationPackage package, int targetVersion, bool inSingleFile = false);
    }
}
