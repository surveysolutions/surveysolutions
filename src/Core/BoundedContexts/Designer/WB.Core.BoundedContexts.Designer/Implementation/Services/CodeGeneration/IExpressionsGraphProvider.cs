using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public interface IExpressionsGraphProvider
    {
        Dictionary<Guid, List<Guid>> BuildDependencyGraph(ReadOnlyQuestionnaireDocument questionnaire);
        Dictionary<Guid, List<Guid>> BuildConditionalDependencies(ReadOnlyQuestionnaireDocument questionnaireDocument);
    }
}