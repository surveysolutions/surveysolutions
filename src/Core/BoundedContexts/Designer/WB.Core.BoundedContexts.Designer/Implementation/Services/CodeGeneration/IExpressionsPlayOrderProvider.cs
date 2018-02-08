using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public interface IExpressionsPlayOrderProvider
    {
        List<Guid> GetExpressionsPlayOrder(ReadOnlyQuestionnaireDocument questionnaire);
        Dictionary<Guid, Guid[]> GetDependencyGraph(ReadOnlyQuestionnaireDocument questionnaire);
        Dictionary<Guid, Guid[]> GetValidationDependencyGraph(ReadOnlyQuestionnaireDocument questionnaire);
    }
}