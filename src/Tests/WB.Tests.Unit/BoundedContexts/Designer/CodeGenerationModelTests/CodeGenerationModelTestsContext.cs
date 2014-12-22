using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.BoundedContexts.Designer.CodeGenerationModelTests
{
    internal class CodeGenerationModelTestsContext
    {
        protected static QuestionnaireExecutorTemplateModel CreateQuestionnaireExecutorTemplateModel(Dictionary<Guid, List<Guid>> conditionalDependencies, List<Guid> conditionsPlayOrder)
        {
            return new QuestionnaireExecutorTemplateModel()
            {
                ConditionalDependencies = conditionalDependencies,
                ConditionsPlayOrder = conditionsPlayOrder
            };
        }
    }
}
