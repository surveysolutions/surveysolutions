using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICodeGenerator
    {
        string Generate(QuestionnaireDocument questionnaire);
        Dictionary<string, string> GenerateEvaluator(QuestionnaireDocument questionnaire, Version targetVersion);
    }
}
