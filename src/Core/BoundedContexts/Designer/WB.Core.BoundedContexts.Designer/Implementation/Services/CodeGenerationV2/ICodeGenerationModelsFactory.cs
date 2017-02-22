using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGenerationV2
{
    public interface ICodeGenerationModelsFactory
    {
        CodeGenerationModel CreateModel(ReadOnlyQuestionnaireDocument questionnaire);
    }
}