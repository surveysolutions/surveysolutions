using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireExpressionProcessorGenerator : IExpressionProcessorGenerator
    {
        private readonly IDynamicCompiler codeCompiler;
        private readonly ICodeGenerator codeGenerator;

        public QuestionnaireExpressionProcessorGenerator(IDynamicCompiler codeCompiler, ICodeGenerator codeGenerator)
        {
            this.codeCompiler =  codeCompiler;
            this.codeGenerator = codeGenerator;
        }

        public GenerationResult GenerateProcessorStateAssembly(QuestionnaireDocument questionnaire,
            Version targetVersion, out string generatedAssembly)
        {
            var generatedEvaluator = this.codeGenerator.GenerateEvaluator(questionnaire, targetVersion);

            EmitResult emitedResult = this.codeCompiler.TryGenerateAssemblyAsStringAndEmitResult(questionnaire.PublicKey, generatedEvaluator, new string[] { },
                out generatedAssembly);

            return new GenerationResult(emitedResult.Success, emitedResult.Diagnostics);
        }

        public Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireDocument questionnaire, Version targetVersion)
        {
            return this.codeGenerator.GenerateEvaluator(questionnaire, targetVersion);
        }

        public string GenerateProcessorStateSingleClass(QuestionnaireDocument questionnaire, Version targetVersion)
        {
            return this.codeGenerator.Generate(questionnaire, targetVersion);
        }
    }
}