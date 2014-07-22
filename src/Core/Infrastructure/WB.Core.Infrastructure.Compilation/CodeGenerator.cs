using Main.Core.Documents;
using WB.Core.Infrastructure.Compilation.Templates;

namespace WB.Core.Infrastructure.Compilation
{
    public class CodeGenerator : ICodeGenerator {

        public string Generate(QuestionnaireDocument questionnaire)
        {
            var questionnaireTemplateStructure = new QuestionnaireExecutorTemplateModel(questionnaire);

            var template = new InterviewExpressionStateTemplate(questionnaireTemplateStructure);

            string result = template.TransformText();

            return result;
        }
    }
}