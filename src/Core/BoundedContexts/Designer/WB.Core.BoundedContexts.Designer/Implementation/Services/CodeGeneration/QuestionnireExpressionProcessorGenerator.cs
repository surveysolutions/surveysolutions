using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnireExpressionProcessorGenerator : IExpressionProcessorGenerator
    {
        private readonly IDynamicCompiler codeCompiler;
        private readonly ICodeGenerator codeGenerator;

        public QuestionnireExpressionProcessorGenerator(IDynamicCompiler codeCompiler, ICodeGenerator codeGenerator)
        {
            this.codeCompiler =  codeCompiler;
            this.codeGenerator = codeGenerator;
        }

        public GenerationResult GenerateProcessorStateAssembly(QuestionnaireDocument questionnaire, out string generatedAssembly)
        {
            Dictionary<string, string> genertedEvaluator = this.codeGenerator.GenerateEvaluator(questionnaire);

            var emmitResult = this.codeCompiler.GenerateAssemblyAsString(questionnaire.PublicKey, genertedEvaluator, new string[] { },
                out generatedAssembly);

            return new GenerationResult(emmitResult.Success, emmitResult.Diagnostics);
        }
    }
}