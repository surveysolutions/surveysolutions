using System.Collections.Generic;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public interface ICodeGeneratorV2
    {
        Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, int targetVersion);
    }
}
