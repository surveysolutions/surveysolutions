using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public interface ICodeGenerationModelsFactory
    {
        ExpressionStorageModel CreateModel(ReadOnlyQuestionnaireDocument questionnaire);
        IEnumerable<LinkedFilterMethodModel> CreateLinkedFilterModels(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames);
        IEnumerable<OptionsFilterMethodModel> CreateCategoricalOptionsFilterModels(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames);
        IEnumerable<ConditionMethodModel> CreateMethodModels(ReadOnlyQuestionnaireDocument questionnaire, Dictionary<RosterScope, string> levelClassNames);
        IEnumerable<LookupTableTemplateModel> CreateLookupModels(QuestionnaireCodeGenerationPackage package);
    }
}