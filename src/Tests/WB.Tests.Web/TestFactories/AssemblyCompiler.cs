using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Web.TestFactories
{
    public class AssemblyCompiler
    {
        public static string CompileAssembly(QuestionnaireDocument questionnaireDocument)
        {
            var expressionProcessorGenerator =
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(), 
                    null,
                    new CodeGeneratorV2(CodeGenerationModelsFactory()),
                    new DynamicCompilerSettingsProvider());

            var latestSupportedVersion = new DesignerEngineVersionService(Mock.Of<IAttachmentService>()).LatestSupportedVersion;
            var emitResult = 
                expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument,  
                    latestSupportedVersion, 
                    out var resultAssembly);

            if (!emitResult.Success || string.IsNullOrEmpty(resultAssembly))
                throw new Exception(
                    $"Errors on IInterviewExpressionState generation:{Environment.NewLine}"
                    + string.Join(Environment.NewLine, emitResult.Diagnostics.Select((d, i) => $"{i + 1}. {d.Message}")));
            return resultAssembly;
        }

        private static CodeGenerationModelsFactory CodeGenerationModelsFactory()
        {
            return new CodeGenerationModelsFactory(
                DefaultMacrosSubstitutionService(),
                new LookupTableService(new TestInMemoryKeyValueStorage<LookupTableContent>(), null),
                new QuestionTypeToCSharpTypeMapper());
        }

        private static IMacrosSubstitutionService DefaultMacrosSubstitutionService()
        {
            var macrosSubstitutionServiceMock = new Mock<IMacrosSubstitutionService>();
            macrosSubstitutionServiceMock.Setup(
                    x => x.InlineMacros(It.IsAny<string>(), It.IsAny<IEnumerable<Macro>>()))
                .Returns((string e, IEnumerable<Macro> macros) => e);

            return macrosSubstitutionServiceMock.Object;
        }
    }
}
