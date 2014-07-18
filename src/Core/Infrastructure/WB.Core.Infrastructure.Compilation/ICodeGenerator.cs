using Main.Core.Documents;

namespace WB.Core.Infrastructure.Compilation
{
    public interface ICodeGenerator
    {
        string Generate(QuestionnaireDocument questionnaire);
    }
}
