using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireExpressionProcessorGenerator : IExpressionProcessorGenerator
    {
        private readonly IDynamicCompiler codeCompiler;
        private readonly ICodeGeneratorV2 codeGeneratorV2;
        private readonly IDynamicCompilerSettingsProvider compilerSettingsProvider;

        public QuestionnaireExpressionProcessorGenerator(
            IDynamicCompiler codeCompiler, 
            ICodeGeneratorV2 codeGeneratorV2,
            IDynamicCompilerSettingsProvider compilerSettingsProvider)
        {
            this.codeCompiler =  codeCompiler;
            this.compilerSettingsProvider = compilerSettingsProvider;
            this.codeGeneratorV2 = codeGeneratorV2;
        }

        public GenerationResult GenerateProcessorStateAssembly(QuestionnaireCodeGenerationPackage package, int targetVersion, out string generatedAssembly)
        {
            var generatedEvaluator = this.GenerateProcessorStateClasses(package, targetVersion);
            List<MetadataReference> referencedPortableAssemblies = this.compilerSettingsProvider.GetAssembliesToReference();

            EmitResult emitedResult = this.codeCompiler.TryGenerateAssemblyAsStringAndEmitResult(
                package.QuestionnaireDocument.PublicKey, 
                generatedEvaluator, 
                referencedPortableAssemblies,
                out generatedAssembly);

            return new GenerationResult(emitedResult.Success, emitedResult.Diagnostics);
        }

        public Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireCodeGenerationPackage package, int targetVersion, bool inSingleFile = false)
        {
            return this.codeGeneratorV2.Generate(package, targetVersion, inSingleFile);
        }
    }
}
