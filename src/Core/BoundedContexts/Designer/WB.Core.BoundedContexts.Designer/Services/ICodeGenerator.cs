using System;
using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICodeGenerator
    {
        Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, Version targetVersion);
    }
}
