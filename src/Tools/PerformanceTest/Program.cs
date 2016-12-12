using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using ICodeGenerator = WB.Core.BoundedContexts.Designer.Services.ICodeGenerator;

namespace PerformanceTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Setup.MockedServiceLocator();

            var multioptions = Enumerable.Range(1, 200).Select(x => Create.Option(x)).ToArray();
            var rosterChildQuestions = Enumerable.Range(1, 30).Select(x => Create.TextQuestion(variable: $"text{x}")).ToArray();
            var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Create.MultiQuestion(q1Id, variable: "q1", options: multioptions, maxAllowedAnswers: Constants.MaxLongRosterRowCount),
                    Create.MultiRoster(rosterId, variable:"r1", enablementCondition:"single==1", sizeQuestionId: q1Id, children: rosterChildQuestions),
                    Create.SingleQuestion(q2Id, variable: "single", options: new List<Answer>
                    {
                        Create.Option("1", text: "Enable roster"),
                        Create.Option("2", text: "Disable roster")
                    })
                });

            ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);

            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine($"Interview {i+1}");
                var interview = SetupInterview(questionnaireDocument, precompiledState: interviewState);
                
                interview.AnswerSingleOptionQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, 1m);

                interview.AnswerMultipleOptionsQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, Enumerable.Range(1, 200).ToArray());
                ////add by one
                //for (int j = 1; j < multioptions.Length; j++)
                //{
                //    interview.AnswerMultipleOptionsQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, Enumerable.Range(1, j).ToArray());
                //}
                // remove by one
                for (int j = multioptions.Length; j >= 1; j--)
                {
                    interview.AnswerMultipleOptionsQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, Enumerable.Range(1, j).ToArray());
                }

                // add batch
                interview.AnswerMultipleOptionsQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, Enumerable.Range(1, 200).ToArray());
                //remove batch
                interview.RemoveAnswer(q1Id, RosterVector.Empty, userId, DateTime.Now);
            }
        }


        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly Guid questionnaireId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF");
        private static readonly Guid userId = Guid.NewGuid();

        protected static Interview SetupInterview(
           QuestionnaireDocument questionnaireDocument,
           IEnumerable<object> events = null,
           ILatestInterviewExpressionState precompiledState = null)
        {
            Guid questionnaireId = questionnaireDocument.PublicKey;

            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(repository
                => repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == new PlainQuestionnaire(questionnaireDocument, 1, null));

            var state = GetLatestInterviewExpressionState(questionnaireDocument, precompiledState);

            var statePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == state);

            var interview = Create.Interview(
                questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: statePrototypeProvider);

            return interview;
        }

        private static ILatestInterviewExpressionState GetLatestInterviewExpressionState(
           QuestionnaireDocument questionnaireDocument,
           ILatestInterviewExpressionState precompiledState = null)
        {
            ILatestInterviewExpressionState state = precompiledState ?? GetInterviewExpressionState(questionnaireDocument);
            return state;
        }


        public static ILatestInterviewExpressionState GetInterviewExpressionState(QuestionnaireDocument questionnaireDocument, bool useLatestEngine = true)
        {
            var compiledAssembly = useLatestEngine
                ? CompileAssemblyUsingLatestEngine(questionnaireDocument)
                : CompileAssemblyUsingQuestionnaireEngine(questionnaireDocument);

            Type interviewExpressionStateType =
                compiledAssembly.GetTypes()
                    .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionState)));

            if (interviewExpressionStateType == null)
                throw new Exception("Type InterviewExpressionState was not found");

            var interviewExpressionState = new InterviewExpressionStateUpgrader().UpgradeToLatestVersionIfNeeded(Activator.CreateInstance(interviewExpressionStateType) as IInterviewExpressionState);
            if (interviewExpressionState == null)
                throw new Exception("Error on IInterviewExpressionState generation");

            return interviewExpressionState;
        }

        protected static Assembly CompileAssemblyUsingQuestionnaireEngine(QuestionnaireDocument questionnaireDocument)
            => CompileAssembly(questionnaireDocument, Create.DesignerEngineVersionService().GetQuestionnaireContentVersion(questionnaireDocument));

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
                Mock.Of<IDynamicCompilerSettings>(_
                    => _.PortableAssembliesPath == pathToProfile
                        && _.DefaultReferencedPortableAssemblies == referencesToAdd
                        && _.Name == "profile111")
            };

            var defaultDynamicCompilerSettings = Mock.Of<ICompilerSettings>(_ => _.SettingsCollection == settings);

            var expressionProcessorGenerator =
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(),
                    Create.CodeGenerator(),
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


        public static IExpressionProcessorGenerator CreateExpressionProcessorGenerator(ICodeGenerator codeGenerator = null, IDynamicCompiler dynamicCompiler = null)
        {
            var fileSystemAccessor = Create.FileSystemIOAccessor();

            const string pathToProfile = "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111";
            var referencesToAdd = new[] { "System.dll", "System.Core.dll", "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll", "System.Linq.Expressions.dll", "System.Linq.Queryable.dll", "mscorlib.dll", "System.Runtime.Extensions.dll", "System.Text.RegularExpressions.dll" };

            var settings = new List<IDynamicCompilerSettings>
            {
                Mock.Of<IDynamicCompilerSettings>(_
                    => _.PortableAssembliesPath == pathToProfile
                    && _.DefaultReferencedPortableAssemblies == referencesToAdd
                    && _.Name == "profile111")
            };

            var defaultDynamicCompilerSettings = Mock.Of<ICompilerSettings>(_ => _.SettingsCollection == settings);

            return
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(),
                    Create.CodeGenerator(),
                    new DynamicCompilerSettingsProvider(defaultDynamicCompilerSettings, fileSystemAccessor));
        }
    }
}