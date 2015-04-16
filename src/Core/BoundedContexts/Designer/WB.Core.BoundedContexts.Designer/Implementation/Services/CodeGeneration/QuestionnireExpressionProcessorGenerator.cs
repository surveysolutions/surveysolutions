using System.Collections.Generic;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnireExpressionProcessorGenerator : IExpressionProcessorGenerator
    {
        private readonly IDynamicCompiler codeCompiler;
        private readonly ICodeGenerator codeGenerator;
        private readonly IEngineVersionService engineVersionService;

        public QuestionnireExpressionProcessorGenerator(IDynamicCompiler codeCompiler, ICodeGenerator codeGenerator, IEngineVersionService engineVersionService)
        {
            this.codeCompiler =  codeCompiler;
            this.codeGenerator = codeGenerator;
            this.engineVersionService = engineVersionService;
        }

        public GenerationResult GenerateProcessorStateAssemblyForVersion(QuestionnaireDocument questionnaire,
            EngineVersion version, out string generatedAssembly)
        {
            var generatedEvaluator = this.codeGenerator.GenerateEvaluatorForVersion(questionnaire, version);

            var emmitResult = this.codeCompiler.GenerateAssemblyAsString(questionnaire.PublicKey, generatedEvaluator, new string[] { },
                out generatedAssembly);

            return new GenerationResult(emmitResult.Success, emmitResult.Diagnostics);
        }

        public Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireDocument questionnaire)
        {
            return this.codeGenerator.GenerateEvaluatorForVersion(questionnaire, engineVersionService.GetCurrentEngineVersion());
        }

        public string GenerateProcessorStateSingleClass(QuestionnaireDocument questionnaire)
        {
            return this.codeGenerator.Generate(questionnaire);
        }
    }
}