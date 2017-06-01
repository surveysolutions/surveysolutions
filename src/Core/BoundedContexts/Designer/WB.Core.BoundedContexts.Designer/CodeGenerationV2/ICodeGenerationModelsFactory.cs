using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public interface ICodeGenerationModelsFactory
    {
        ExpressionStorageModel CreateModel(ReadOnlyQuestionnaireDocument questionnaire);
        IEnumerable<LinkedFilterMethodModel> CreateLinkedFilterModels(ReadOnlyQuestionnaireDocument questionnaire, ExpressionStorageModel model);
        IEnumerable<OptionsFilterMethodModel> CreateCategoricalOptionsFilterModels(ReadOnlyQuestionnaireDocument questionnaire, ExpressionStorageModel model);
        IEnumerable<ConditionMethodModel> CreateMethodModels(ReadOnlyQuestionnaireDocument questionnaire, ExpressionStorageModel model);
        IEnumerable<LookupTableTemplateModel> CreateLookupModels(ReadOnlyQuestionnaireDocument questionnaire);
    }
}