using System;
using System.Collections.Generic;
using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICodeGenerator
    {
        Dictionary<string, string> Generate(QuestionnaireDocument questionnaire, Version targetVersion);
    }
}
