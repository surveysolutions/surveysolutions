using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireExpressionProcessorGenerator : IExpressionProcessorGenerator
    {
        private readonly IDynamicCompiler codeCompiler;
        private readonly ICodeGenerator codeGenerator;
        private readonly IExpressionsEngineVersionService expressionsEngineVersionService;

        public QuestionnaireExpressionProcessorGenerator(IDynamicCompiler codeCompiler, ICodeGenerator codeGenerator, IExpressionsEngineVersionService expressionsEngineVersionService)
        {
            this.codeCompiler =  codeCompiler;
            this.codeGenerator = codeGenerator;
            this.expressionsEngineVersionService = expressionsEngineVersionService;
        }

        public GenerationResult GenerateProcessorStateAssembly(QuestionnaireDocument questionnaire,
            Version targetVersion, out string generatedAssembly)
        {
            var generatedEvaluator = this.codeGenerator.GenerateEvaluator(questionnaire, targetVersion);

            EmitResult emitedResult = this.codeCompiler.TryGenerateAssemblyAsStringAndEmitResult(questionnaire.PublicKey, generatedEvaluator, new string[] { },
                out generatedAssembly);

            return new GenerationResult(emitedResult.Success, emitedResult.Diagnostics);
        }

        public Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireDocument questionnaire)
        {
            return this.codeGenerator.GenerateEvaluator(questionnaire, expressionsEngineVersionService.GetLatestSupportedVersion());
        }

        public string GenerateProcessorStateSingleClass(QuestionnaireDocument questionnaire)
        {
            return this.codeGenerator.Generate(questionnaire);
        }
    }
}