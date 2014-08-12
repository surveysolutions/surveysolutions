using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Templates;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class CodeGenerator : ICodeGenerator
    {
        public string Generate(QuestionnaireDocument questionnaire)
        {
            var questionnaireTemplateStructure = new QuestionnaireExecutorTemplateModel(questionnaire);
            
            var template = new InterviewExpressionStateTemplate(questionnaireTemplateStructure);

            return template.TransformText();
        }
    }
}