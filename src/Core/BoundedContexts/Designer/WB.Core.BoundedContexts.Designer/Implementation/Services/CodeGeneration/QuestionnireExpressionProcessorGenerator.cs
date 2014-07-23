using Main.Core.Documents;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.Infrastructure.Compilation;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnireExpressionProcessorGenerator : IExpressionProcessorGenerator {

        public QuestionnireExpressionProcessorGenerator(IDynamicCompiler interviewCompiler = null, ICodeGenerator codeGenerator = null)
        {
            this.codeCompiler = interviewCompiler ?? new RoslynCompiler();
            this.codeGenerator = codeGenerator ?? new CodeGenerator();
        }

        private IDynamicCompiler codeCompiler;
        private ICodeGenerator codeGenerator;

        public EmitResult GenerateProcessor(QuestionnaireDocument questionnaire, out string generatedAssembly)
        {
            this.codeGenerator = new CodeGenerator();
            this.codeCompiler = new RoslynCompiler();

            string genertedEvaluator = this.codeGenerator.Generate(questionnaire);

            return this.codeCompiler.GenerateAssemblyAsString(questionnaire.PublicKey, genertedEvaluator, new string[] { }, out generatedAssembly);
        }
    }
}