using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.CodeGenerationV3;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration
{
    public class QuestionnaireExpressionProcessorGenerator : IExpressionProcessorGenerator
    {
        private readonly IDynamicCompiler codeCompiler;
        private readonly ICodeGenerator codeGenerator;
        private readonly ICodeGeneratorV2 codeGeneratorV2;
        private readonly ICodeGeneratorV3 codeGeneratorV3;
        private readonly IDynamicCompilerSettingsProvider compilerSettingsProvider;

        public QuestionnaireExpressionProcessorGenerator(
            IDynamicCompiler codeCompiler, 
            ICodeGenerator codeGenerator,
            ICodeGeneratorV2 codeGeneratorV2,
            ICodeGeneratorV3 codeGeneratorV3,
            IDynamicCompilerSettingsProvider compilerSettingsProvider)
        {
            this.codeCompiler =  codeCompiler;
            this.codeGenerator = codeGenerator;
            this.compilerSettingsProvider = compilerSettingsProvider;
            this.codeGeneratorV2 = codeGeneratorV2;
            this.codeGeneratorV3 = codeGeneratorV3;
        }

        public GenerationResult GenerateProcessorStateAssembly(QuestionnaireDocument questionnaire, int targetVersion, out string generatedAssembly)
        {
            var generatedEvaluator = this.GenerateProcessorStateClasses(questionnaire, targetVersion);
            List<PortableExecutableReference> referencedPortableAssemblies = this.compilerSettingsProvider.GetAssembliesToReference();

            EmitResult emitedResult = this.codeCompiler.TryGenerateAssemblyAsStringAndEmitResult(
                questionnaire.PublicKey, 
                generatedEvaluator, 
                referencedPortableAssemblies.ToArray(),
                out generatedAssembly);

            return new GenerationResult(emitedResult.Success, emitedResult.Diagnostics);
        }

        public Dictionary<string, string> GenerateProcessorStateClasses(QuestionnaireDocument questionnaire, int targetVersion, bool inSingleFile = false)
        {
            if (targetVersion >= 22)
            {
                return this.codeGeneratorV3.Generate(questionnaire, targetVersion, inSingleFile);
            }
            return targetVersion >= 20 
                ? this.codeGeneratorV2.Generate(questionnaire, targetVersion, inSingleFile) 
                : this.codeGenerator.Generate(questionnaire, targetVersion);
        }
    }
}