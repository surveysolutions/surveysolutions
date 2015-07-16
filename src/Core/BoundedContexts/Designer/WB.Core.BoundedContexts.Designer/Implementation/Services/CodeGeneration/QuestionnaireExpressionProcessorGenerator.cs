using System;
using System.Collections.Generic;
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
            var dynamicCompillerSettings = this.compilerSettingsProvider.GetSettings(targetVersion);
            var referencedPortableAssemblies = GetReferencedPortableAssemblies(targetVersion);

            EmitResult emitedResult = this.codeCompiler.TryGenerateAssemblyAsStringAndEmitResult(
                questionnaire.PublicKey, 
                generatedEvaluator, 
                referencedPortableAssemblies.ToArray(),
                dynamicCompillerSettings,
                out generatedAssembly);

            return new GenerationResult(emitedResult.Success, emitedResult.Diagnostics);
        }

        static List<PortableExecutableReference> GetReferencedPortableAssemblies(Version targetVersion)
        {
            var referencedPortableAssemblies = new List<PortableExecutableReference>();
            if (targetVersion.Major < 8)
            {
                var refProfile24 = AssemblyMetadata.CreateFromImage(
                    RoslynCompilerResources.WB_Core_SharedKernels_DataCollection_Portable_Profile24
                    ).GetReference();
                referencedPortableAssemblies.Add(refProfile24);
            }
            else
            {
                referencedPortableAssemblies.Add(
                    AssemblyMetadata.CreateFromFile(typeof (Identity).Assembly.Location).GetReference());
            }
            return referencedPortableAssemblies;
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