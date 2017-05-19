using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2.Models;
using WB.Core.BoundedContexts.Designer.Implementation.Services;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public interface ICodeGenerationModelsFactory
    {
        CodeGenerationModel CreateModel(ReadOnlyQuestionnaireDocument questionnaire);
        IEnumerable<LinkedFilterMethodModel> CreateLinkedFilterModels(ReadOnlyQuestionnaireDocument questionnaire, CodeGenerationModel model);
        IEnumerable<OptionsFilterMethodModel> CreateCategoricalOptionsFilterModels(ReadOnlyQuestionnaireDocument questionnaire, CodeGenerationModel model);
        IEnumerable<ConditionMethodModel> CreateMethodModels(ReadOnlyQuestionnaireDocument questionnaire, CodeGenerationModel model);
    }
}