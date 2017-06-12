using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace PerformanceTest
{
    public abstract class ExpressionStorageBenchBase
    {
        protected Interview SetupInterview(
            QuestionnaireDocument questionnaireDocument,
            IEnumerable<object> events = null,
            ILatestInterviewExpressionState precompiledState = null)
        {
            questionnaireDocument.IsUsingExpressionStorage = true;
            var playOrderProvider = ServiceLocator.Current.GetInstance<IExpressionsPlayOrderProvider>();
            questionnaireDocument.ExpressionsPlayOrder = playOrderProvider.GetExpressionsPlayOrder(new ReadOnlyQuestionnaireDocument(questionnaireDocument));

            var questionnaireRepository = new Mocks.QuestionnaireStorageStub(() => new PlainQuestionnaire(questionnaireDocument, 1, null));
            var storage = GetInterviewExpressionStorage(questionnaireDocument);
            var statePrototypeProvider = new Mocks.InterviewExpressionStatePrototypeProviderStub(getStorage: () => storage);

            var interview = Create.Interview(
                questionnaireId: questionnaireDocument.PublicKey,
                questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: statePrototypeProvider);

            return interview;
        }

        public static IInterviewExpressionStorage GetInterviewExpressionStorage(QuestionnaireDocument questionnaireDocument)
        {
            var compiledAssembly = CompileAssemblyUsingLatestEngine(questionnaireDocument);

            Type interviewExpressionStateType =
                compiledAssembly.DefinedTypes
                    .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionStorage)));

            return Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionStorage;
        }

        protected static Assembly CompileAssemblyUsingLatestEngine(QuestionnaireDocument questionnaireDocument)
            => CompileAssembly(questionnaireDocument, Create.DesignerEngineVersionService().LatestSupportedVersion);


        protected static Assembly CompileAssembly(QuestionnaireDocument questionnaireDocument, int engineVersion)
        {
            var filePath = Path.GetTempFileName();

            var fileSystemAccessor = Create.FileSystemIOAccessor();
            const string pathToProfile = "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111";

            var referencesToAdd = new[]
            {
                "System.dll", "System.Core.dll", "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll",
                "System.Linq.Expressions.dll", "System.Linq.Queryable.dll", "mscorlib.dll",
                "System.Runtime.Extensions.dll", "System.Text.RegularExpressions.dll"
            };

            var settings = new List<IDynamicCompilerSettings>
            {
                new Mocks.DynamicCompilerSettingsStub
                {
                    DefaultReferencedPortableAssemblies = referencesToAdd,
                    PortableAssembliesPath = pathToProfile,
                    Name = "profile111"
                }
            };

            var defaultDynamicCompilerSettings = new Mocks.CompilerSettingsStub(settings);

            var expressionProcessorGenerator =
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(),
                    Create.CodeGenerator(),
                    Create.CodeGeneratorV2(),
                    new DynamicCompilerSettingsProvider(defaultDynamicCompilerSettings, fileSystemAccessor));

            string resultAssembly;
            var emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument,
                engineVersion, out resultAssembly);

            if (!emitResult.Success || string.IsNullOrEmpty(resultAssembly))
                throw new Exception($"Errors on IInterviewExpressionState generation:{Environment.NewLine}" + string.Join(Environment.NewLine, emitResult.Diagnostics.Select((d, i) => $"{i + 1}. {d.Message}")));

            File.WriteAllBytes(filePath, Convert.FromBase64String(resultAssembly));
            var compiledAssembly = Assembly.LoadFrom(filePath);

            return compiledAssembly;
        }

        protected Interview interview;

        protected ExpressionStorageBenchBase()
        {
            Setup.MockedServiceLocator();
            IterationSetup();
        }

        //[IterationSetup]
        public void IterationSetup()
        {
            this.interview = this.SetupInterview(this.CreateDocument());
        }

        protected abstract QuestionnaireDocument CreateDocument();
    }
}