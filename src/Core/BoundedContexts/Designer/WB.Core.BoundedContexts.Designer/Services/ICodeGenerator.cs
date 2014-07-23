using Main.Core.Documents;

namespace WB.Core.BoundedContexts.Designer.Services
{
    public interface ICodeGenerator
    {
        string Generate(QuestionnaireDocument questionnaire);
    }
}
