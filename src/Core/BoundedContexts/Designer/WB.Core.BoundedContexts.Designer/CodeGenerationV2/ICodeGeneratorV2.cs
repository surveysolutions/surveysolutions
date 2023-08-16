using System;
using System.Collections.Generic;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public interface ICodeGeneratorV2
    {
        Dictionary<string, string> Generate(QuestionnaireCodeGenerationPackage package, int targetVersion, bool inSingleFile);
    }
}
