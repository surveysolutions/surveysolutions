using System.Collections.Generic;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICodeGenerator
    {
        string Generate(QuestionnaireDocument questionnaire);
        Dictionary<string, string> GenerateEvaluator(QuestionnaireDocument questionnaire);
        
    }
}
