using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.V6;
using WB.Core.SharedKernels.DataCollection.V7;
using WB.Infrastructure.Native.Files.Implementation.FileSystem;
using It = Moq.It;

namespace WB.Tests.Integration.InterviewTests
{
    [Subject(typeof(Interview))]
    internal class InterviewTestsContext
    {
        protected static Interview SetupInterviewFromQuestionnaireDocumentRegisteringAllNeededDependencies(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireId = Guid.Parse("10000010000100100100100001000001");

            PlainQuestionnaire questionnaire = CreateQuestionnaire(questionnaireDocument);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);
            var interviewExpressionStatePrototypeProvider = CreateInterviewExpressionStateProviderStub(questionnaireId);

            return Create.Interview(
                questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: interviewExpressionStatePrototypeProvider);
        }

        protected static PlainQuestionnaire CreateQuestionnaire(QuestionnaireDocument questionnaireDocument, Guid? userId = null)
        {
            return new PlainQuestionnaire(questionnaireDocument, 1);
        }

        protected static IPlainQuestionnaireRepository CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire)
        {
            return Mock.Of<IPlainQuestionnaireRepository>(repository
                => repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);
        }

        protected static IInterviewExpressionStatePrototypeProvider CreateInterviewExpressionStateProviderStub(Guid questionnaireId)
        {
            var expressionState = new Mock<IInterviewExpressionStateV7>();

            var emptyList = new List<Identity>();

            expressionState.Setup(_ => _.Clone()).Returns(expressionState.Object);
            expressionState.Setup(_ => _.ProcessEnablementConditions()).Returns(new EnablementChanges(emptyList, emptyList, emptyList, emptyList));
            
            return Mock.Of<IInterviewExpressionStatePrototypeProvider>(
                provider => provider.GetExpressionState(questionnaireId, Moq.It.IsAny<long>()) == expressionState.Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            var result = new QuestionnaireDocument();
            var chapter = new Group("Chapter");
            result.Children.Add(chapter);

            foreach (var child in children)
            {
                chapter.Children.Add(child);
            }

            return result;
        }

        protected static Interview SetupInterview(QuestionnaireDocument questionnaireDocument, IEnumerable<object> events = null, IInterviewExpressionStateV7 precompiledState = null)
        {
            Guid questionnaireId = questionnaireDocument.PublicKey;

            var questionnaireRepository = Mock.Of<IPlainQuestionnaireRepository>(repository
                =>  repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) ==new PlainQuestionnaire(questionnaireDocument,1));

            IInterviewExpressionStateV7 state = precompiledState ?? GetInterviewExpressionState(questionnaireDocument) ;

            var statePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == state);

            var interview = Create.Interview(
                questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository,
                expressionProcessorStatePrototypeProvider: statePrototypeProvider);

            ApplyAllEvents(interview, events);

            return interview;
        }

        protected static Interview SetupInterview(string questionnaireString, object[] events, IInterviewExpressionState precompiledState)
        {
            var json = new NewtonJsonSerializer(new JsonSerializerSettingsFactory());
            var questionnaireDocument = json.Deserialize<QuestionnaireDocument>(questionnaireString);
            return SetupInterview(questionnaireDocument, events);
        }

        public static void ApplyAllEvents(Interview interview, IEnumerable<object> events)
        {
            if (events == null)
                return;

            foreach (var evnt in events)
            {
                interview.Apply((dynamic)evnt);
            }
        }

        public static bool HasEvent<T>(IEnumerable<UncommittedEvent> events)
            where T : class
        {
            return events.Any(b => b.Payload is T);
        }

        public static bool HasEvent<T>(IEnumerable<UncommittedEvent> events, Func<T, bool> @where)
            where T : class
        {
            return events.Any(b => (b.Payload is T) && @where((T)b.Payload));
        }

        public static IEnumerable<T> EventsByType<T>(IEnumerable<UncommittedEvent> events)
            where T : class
        {
            return events.Select(evnt => evnt.Payload).OfType<T>();
        }

        public static T GetFirstEventByType<T>(IEnumerable<UncommittedEvent> events)
            where T : class
        {
            var firstTypedEvent = events.FirstOrDefault(b => b.Payload is T);

            return firstTypedEvent != null ? ((T)firstTypedEvent.Payload) : null;
        }

        public static IInterviewExpressionStateV7 GetInterviewExpressionState(QuestionnaireDocument questionnaireDocument)
        {
            var fileSystemAccessor = new FileSystemIOAccessor(); 
            var questionnaireVersionProvider =new DesignerEngineVersionService();

            const string pathToProfile = "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111";
            var referencesToAdd = new[] { "System.dll", "System.Core.dll", "mscorlib.dll", "System.Runtime.dll", "System.Collections.dll", "System.Linq.dll" };

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
            var emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument,questionnaireVersionProvider.GetLatestSupportedVersion(), out resultAssembly);

            var filePath = Path.GetTempFileName();

            if (emitResult.Success && !string.IsNullOrEmpty(resultAssembly))
            {
                File.WriteAllBytes(filePath, Convert.FromBase64String(resultAssembly));

                var compiledAssembly = Assembly.LoadFrom(filePath);

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

            throw new Exception("Error on IInterviewExpressionState generation");
        }
    }
}
