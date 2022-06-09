using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.LookupTableService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Web.TestFactories
{
    public class AssemblyCompiler
    {
        public static IInterviewExpressionStorage GetInterviewExpressionStorage(QuestionnaireDocument questionnaireDocument)
        {
            var compiledAssembly = CompileAssembly(questionnaireDocument);

            var ass = Assembly.Load(Convert.FromBase64String(compiledAssembly));

            Type interviewExpressionStorageType =
                ass.GetTypes()
                    .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionStorage)));

            if (interviewExpressionStorageType == null)
                throw new Exception("Type InterviewExpressionState was not found");


            if (!(Activator.CreateInstance(interviewExpressionStorageType) is IInterviewExpressionStorage interviewExpressionStorage))
                throw new Exception("Error on IInterviewExpressionState generation");

            return interviewExpressionStorage;
        }


        public static string CompileAssembly(QuestionnaireDocument questionnaireDocument)
        {
            var expressionProcessorGenerator =
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(), 
                    new CodeGeneratorV2(CodeGenerationModelsFactory()),
                    new DynamicCompilerSettingsProvider());

            var latestSupportedVersion = new DesignerEngineVersionService(Mock.Of<IAttachmentService>(),
                Mock.Of<IDesignerTranslationService>()).LatestSupportedVersion;
            var package = new QuestionnaireCodeGenerationPackage(questionnaireDocument, null);
            var emitResult = 
                expressionProcessorGenerator.GenerateProcessorStateAssembly(package,  
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
