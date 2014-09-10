using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnireExpressionProcessorGenerator : IExpressionProcessorGenerator
    {
        private readonly IDynamicCompiler codeCompiler;
        private readonly ICodeGenerator codeGenerator;

        public QuestionnireExpressionProcessorGenerator()
        {
            this.codeCompiler =  new RoslynCompiler();
            this.codeGenerator = new CodeGenerator();
        }

        public GenerationResult GenerateProcessorStateAssembly(QuestionnaireDocument questionnaire, out string generatedAssembly)
        {
            string genertedEvaluator = this.codeGenerator.Generate(questionnaire);

            var emmitResult = this.codeCompiler.GenerateAssemblyAsString(questionnaire.PublicKey, genertedEvaluator, new string[] { },
                out generatedAssembly);

            return new GenerationResult(emmitResult.Success, emmitResult.Diagnostics);
        }
    }
}