using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Rest;
using WB.Core.Infrastructure.Files.Implementation.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.V2;
using WB.Core.SharedKernels.SurveySolutions;
using WB.Core.SharedKernels.SurveySolutions.Implementation.Services;
using It = Moq.It;

namespace WB.Tests.Integration.InterviewTests
{
    [Subject(typeof(Interview))]
    internal class InterviewTestsContext
    {
        protected static Interview CreateInterviewFromQuestionnaireDocumentRegisteringAllNeededDependencies(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireId = Guid.Parse("10000010000100100100100001000001");

            PlainQuestionnaire questionnaire = CreateQuestionnaire(questionnaireDocument);

            var questionnaireRepository = CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IInterviewExpressionStatePrototypeProvider>())
                .Returns(CreateInterviewExpressionStateProviderStub(questionnaireId));

            return CreateInterview(questionnaireId: questionnaireId);
        }

        protected static Interview CreateInterview(Guid? interviewId = null, Guid? userId = null, Guid? questionnaireId = null,
            Dictionary<Guid, object> answersToFeaturedQuestions = null, DateTime? answersTime = null, Guid? supervisorId = null)
        {
            return new Interview(
                interviewId ?? new Guid("A0A0A0A0B0B0B0B0A0A0A0A0B0B0B0B0"),
                userId ?? new Guid("F111F111F111F111F111F111F111F111"),
                questionnaireId ?? new Guid("B000B000B000B000B000B000B000B000"),1,
                answersToFeaturedQuestions ?? new Dictionary<Guid, object>(),
                answersTime ?? new DateTime(2012, 12, 20),
                supervisorId ?? new Guid("D222D222D222D222D222D222D222D222"));
        }

        protected static PlainQuestionnaire CreateQuestionnaire(QuestionnaireDocument questionnaireDocument, Guid? userId = null)
        {
            return new PlainQuestionnaire(questionnaireDocument, 1);
        }

        protected static IQuestionnaireRepository CreateQuestionnaireRepositoryStubWithOneQuestionnaire(Guid questionnaireId, IQuestionnaire questionaire)
        {
            return Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionaire
                && repository.GetHistoricalQuestionnaire(questionnaireId, Moq.It.IsAny<long>()) == questionaire);
        }

        protected static IInterviewExpressionStatePrototypeProvider CreateInterviewExpressionStateProviderStub(Guid questionnaireId)
        {
            var expressionState = new Mock<IInterviewExpressionStateV2>();

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

        protected static void SetupInstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }

        protected static Interview SetupInterview(QuestionnaireDocument questionnaireDocument, IEnumerable<object> events = null, IInterviewExpressionStateV2 precompiledState = null)
        {
            Guid questionnaireId = questionnaireDocument.PublicKey;

            var questionnaire = Create.Questionnaire(questionnaireDocument);

            var questionnaireRepository = Mock.Of<IQuestionnaireRepository>(repository
                => repository.GetQuestionnaire(questionnaireId) == questionnaire.GetQuestionnaire()
                    && repository.GetHistoricalQuestionnaire(questionnaireId, questionnaire.GetQuestionnaire().Version) == questionnaire.GetQuestionnaire()
                    && repository.GetHistoricalQuestionnaire(questionnaireId, 1) == questionnaire.GetQuestionnaire());

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IQuestionnaireRepository>())
                .Returns(questionnaireRepository);

            SetupRoslyn(questionnaireDocument, precompiledState);

            var interview = Create.Interview(questionnaireId: questionnaireId);

            ApplyAllEvents(interview, events);

            return interview;
        }

        protected static Interview SetupInterview(string questionnaireString, object[] events, IInterviewExpressionState precompiledState)
        {
            var json = new NewtonJsonUtils();
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

        public static void SetupRoslyn(QuestionnaireDocument questionnaireDocument, IInterviewExpressionStateV2 precompiledState = null)
        {
            IInterviewExpressionStateV2 state = precompiledState ?? GetInterviewExpressionState(questionnaireDocument) ;

            var statePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == state);

            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IInterviewExpressionStatePrototypeProvider>())
                .Returns(statePrototypeProvider);
        }

        public static IInterviewExpressionStateV2 GetInterviewExpressionState(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireVersionProvider =new EngineVersionService();
            var expressionProcessorGenerator =
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(
                        new DefaultDynamicCompillerSettings()
                        {
                            PortableAssembliesPath =
                                "C:\\Program Files (x86)\\Reference Assemblies\\Microsoft\\Framework\\.NETPortable\\v4.5\\Profile\\Profile111",
                            DefaultReferencedPortableAssemblies = new[] { "System.dll", "System.Core.dll", "mscorlib.dll", "System.Runtime.dll", 
                                "System.Collections.dll", "System.Linq.dll" }
                        }, new FileSystemIOAccessor()),
                    new CodeGenerator(questionnaireVersionProvider), questionnaireVersionProvider);

            string resultAssembly;
            var emitResult = expressionProcessorGenerator.GenerateProcessorStateAssemblyForVersion(questionnaireDocument,questionnaireVersionProvider.GetLatestSupportedVersion(), out resultAssembly);

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
