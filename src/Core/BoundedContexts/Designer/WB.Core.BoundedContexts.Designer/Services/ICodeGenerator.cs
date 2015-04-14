using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICodeGenerator
    {
        string Generate(QuestionnaireDocument questionnaire);
        Dictionary<string, string> GenerateEvaluator(QuestionnaireDocument questionnaire);
        Dictionary<string, string> GenerateEvaluatorForVersion(QuestionnaireDocument questionnaire, QuestionnaireVersion version);
    }
}
