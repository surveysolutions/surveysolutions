using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.CodeGenerationV2
{
    public interface IQuestionTypeToCSharpTypeMapper
    {
        string GetQuestionType(IQuestion question, ReadOnlyQuestionnaireDocument questionnaire);
        string GetVariableType(VariableType variableType);
        string GetQuestionMethodName(string questionModelTypeName, int settingsTargetVersion);
    }
}