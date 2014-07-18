using WB.Core.Infrastructure.Compilation.Templates;

namespace WB.Core.Infrastructure.Compilation
{
    public class CodeGenerator : ICodeGenerator {
        public string Generate()
        {
            var template = new InterviewExpressionStateTemplate();
            string result = template.TransformText();

            return result;
        }
    }
}