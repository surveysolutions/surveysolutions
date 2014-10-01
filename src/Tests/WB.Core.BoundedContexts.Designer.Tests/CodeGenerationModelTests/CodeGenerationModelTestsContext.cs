using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationModelTests
{
    internal class CodeGenerationModelTestsContext
    {
        protected static QuestionnaireExecutorTemplateModel CreateQuestionnaireExecutorTemplateModel(Dictionary<Guid, List<Guid>> conditionalDependencies)
        {
            return new QuestionnaireExecutorTemplateModel()
            {
                ConditionalDependencies = conditionalDependencies
            };
        }
    }
}
