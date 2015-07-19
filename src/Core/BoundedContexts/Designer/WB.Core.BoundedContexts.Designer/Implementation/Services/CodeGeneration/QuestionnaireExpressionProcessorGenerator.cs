using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireExpressionProcessorGenerator : IExpressionProcessorGenerator
    {
        private readonly IDynamicCompiler codeCompiler;
        private readonly ICodeGenerator codeGenerator;
        private readonly IDynamicCompilerSettingsProvider compilerSettingsProvider;

        public QuestionnaireExpressionProcessorGenerator(
            IDynamicCompiler codeCompiler, 
            ICodeGenerator codeGenerator,
            IDynamicCompilerSettingsProvider compilerSettingsProvider)
        {
            this.codeCompiler =  codeCompiler;
            this.codeGenerator = codeGenerator;
            this.compilerSettingsProvider = compilerSettingsProvider;
        }

        public GenerationResult GenerateProcessorStateAssembly(QuestionnaireDocument questionnaire,
            Version targetVersion, out string generatedAssembly)
        {
            var generatedEvaluator = this.codeGenerator.GenerateEvaluator(questionnaire, targetVersion);
            var referencedPortableAssemblies = this.compilerSettingsProvider.GetAssembliesToRoslyn(targetVersion);

            EmitResult emitedResult = this.codeCompiler.TryGenerateAssemblyAsStringAndEmitResult(
                questionnaire.PublicKey, 
                generatedEvaluator, 
                referencedPortableAssemblies.ToArray(),
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