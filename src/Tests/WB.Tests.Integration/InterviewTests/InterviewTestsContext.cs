using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using MvvmCross.Tests;
using Ncqrs.Eventing;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Tests.Abc;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Integration.InterviewTests
{
    public class InterviewTestsContext : MvxIoCSupportingTest
    {
        internal static AnsweredYesNoOption Yes(decimal value)
        {
            return Create.Entity.AnsweredYesNoOption(value, true);
        }
        internal static AnsweredYesNoOption No(decimal value)
        {
            return Create.Entity.AnsweredYesNoOption(value, false);
        }

        protected static StatefulInterview SetupPreloadedInterview(
            AssemblyLoadContext assemblyLoadContext,
            PreloadedDataDto preloadedData,
            QuestionnaireDocument questionnaireDocument,
            IEnumerable<object> events = null)
        {
            Guid questionnaireId = questionnaireDocument.PublicKey;
            questionnaireDocument.IsUsingExpressionStorage = true;
            var playOrderProvider = IntegrationCreate.ExpressionsPlayOrderProvider();
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();
            questionnaireDocument.ExpressionsPlayOrder = playOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaireDocument.DependencyGraph = playOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaireDocument.ValidationDependencyGraph = playOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument);

            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1);
            plainQuestionnaire.ExpressionStorageType = GetLatestExpressionStorage(assemblyLoadContext, questionnaireDocument).GetType();

            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(repository
                => repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) ==
                   plainQuestionnaire);

            var interview = IntegrationCreate.PreloadedInterview(
                preloadedData, questionnaireId, questionnaireRepository);

            ApplyAllEvents(interview, events);

            return interview;
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] children)
        {
            var result = new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        Children = children.ToReadOnlyCollection()
                    }
                }.ToReadOnlyCollection()
            };

            return result;
        }

        protected static StatefulInterview SetupStatefullInterview(
            AssemblyLoadContext assemblyLoadContext,
            QuestionnaireDocument questionnaireDocument, 
            IEnumerable<object> events = null, 
            bool useLatestEngine = true,
            List<InterviewAnswer> answers = null,
            QuestionnaireIdentity questionnaireIdentity = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IQuestionOptionsRepository questionOptionsRepository = null)
        {
            return SetupStatefullInterviewWithExpressionStorage(assemblyLoadContext, questionnaireDocument, events, answers, questionnaireIdentity, questionnaireStorage, questionOptionsRepository);
        }

        protected static StatefulInterview SetupStatefullInterviewWithExpressionStorage(
            AssemblyLoadContext assemblyLoadContext,
            QuestionnaireDocument questionnaireDocument,
            IEnumerable<object> events = null,
            List<InterviewAnswer> answers = null,
            QuestionnaireIdentity questionnaireIdentity = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IQuestionOptionsRepository optionsRepository = null)
        {
            questionnaireIdentity = questionnaireIdentity ?? new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1);
            questionnaireDocument.IsUsingExpressionStorage = true;
            var playOrderProvider = IntegrationCreate.ExpressionsPlayOrderProvider();
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();
            questionnaireDocument.ExpressionsPlayOrder = playOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            var dependencyGraph = playOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaireDocument.DependencyGraph = dependencyGraph ?? throw new ArgumentException("please check questionnaire, you have cycle reference");
            questionnaireDocument.ValidationDependencyGraph = playOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument);

            var questionOptionsRepository = optionsRepository ?? Create.Storage.QuestionnaireQuestionOptionsRepository();

            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, questionnaireIdentity.Version, 
                questionOptionsRepository: questionOptionsRepository);
            questionnaire.ExpressionStorageType = GetLatestExpressionStorage(assemblyLoadContext, questionnaireDocument).GetType();

            var questionnaireRepository = questionnaireStorage ?? Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireIdentity.QuestionnaireId,
                questionnaire,
                questionnaireIdentity.Version);

            var interview = IntegrationCreate.StatefulInterview(
                questionnaireIdentity,
                answersOnPrefilledQuestions: answers,
                questionnaireRepository: questionnaireRepository,
                questionOptionsRepository: questionOptionsRepository);

            ApplyAllEvents(interview, events);

            return interview;
        }


        protected static StatefulInterview SetupStatefullInterviewWithExpressionStorageWithoutCreate(
            AssemblyLoadContext assemblyLoadContext,
            QuestionnaireDocument questionnaireDocument,
            QuestionnaireIdentity questionnaireIdentity = null,
            IQuestionnaireStorage questionnaireStorage = null)
        {
            questionnaireIdentity = questionnaireIdentity ?? new QuestionnaireIdentity(questionnaireDocument.PublicKey, 1);
            questionnaireDocument.IsUsingExpressionStorage = true;
            var playOrderProvider = IntegrationCreate.ExpressionsPlayOrderProvider();
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();
            questionnaireDocument.ExpressionsPlayOrder = playOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaireDocument.DependencyGraph = playOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaireDocument.ValidationDependencyGraph = playOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument);

            var questionnaireRepository = questionnaireStorage;
            if (questionnaireRepository == null)
            {
                var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
                plainQuestionnaire.ExpressionStorageType = GetLatestExpressionStorage(assemblyLoadContext, questionnaireDocument).GetType();
                questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                    questionnaireIdentity.QuestionnaireId,
                    plainQuestionnaire, questionnaireIdentity.Version);
            }

            var serviceLocatorMock = new Mock<IServiceLocator>();

            var qRepository = questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>();
            serviceLocatorMock.Setup(x => x.GetInstance<IQuestionnaireStorage>())
                .Returns(qRepository);

            serviceLocatorMock.Setup(x => x.GetInstance<IQuestionOptionsRepository>())
                .Returns(Mock.Of<IQuestionOptionsRepository>());

            var interview = new StatefulInterview(
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder(),
                Create.Storage.QuestionnaireQuestionOptionsRepository());

            interview.ServiceLocatorInstance = serviceLocatorMock.Object;

            return interview;
        }

        protected static Interview SetupInterviewWithExpressionStorage(
            AssemblyLoadContext assemblyLoadContext,
            QuestionnaireDocument questionnaireDocument,
            IEnumerable<object> events = null,
            IQuestionOptionsRepository optionsRepository = null)
        {
            return SetupInterviewWithExpressionStorage(assemblyLoadContext,
                new QuestionnaireCodeGenerationPackage(questionnaireDocument, null),
                events,
                optionsRepository);
        }

        protected static Interview SetupInterviewWithExpressionStorage(
            AssemblyLoadContext assemblyLoadContext,
            QuestionnaireCodeGenerationPackage package,
            IEnumerable<object> events = null,
            IQuestionOptionsRepository optionsRepository = null)
        {
            var questionnaireDocument = package.QuestionnaireDocument.Questionnaire;
            Guid questionnaireId = questionnaireDocument.PublicKey;
            questionnaireDocument.IsUsingExpressionStorage = true;
            var playOrderProvider = IntegrationCreate.ExpressionsPlayOrderProvider();
            var readOnlyQuestionnaireDocument = questionnaireDocument.AsReadOnly();
            questionnaireDocument.ExpressionsPlayOrder = playOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
            questionnaireDocument.DependencyGraph = playOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
            questionnaireDocument.ValidationDependencyGraph = playOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument);

            var optionRepo = optionsRepository ?? Create.Storage.QuestionnaireQuestionOptionsRepository();
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null, null, optionRepo);
            
            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(repository
                => repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
            
            SetUp.InstanceToMockedServiceLocator(questionnaireRepository);

            IInterviewExpressionStorage state = GetInterviewExpressionStorage(assemblyLoadContext, package);
            questionnaire.ExpressionStorageType = state.GetType();
            var statePrototypeProvider = Mock.Of<IInterviewExpressionStorageProvider>(a => a.GetExpressionStorage(It.IsAny<QuestionnaireIdentity>()) == state);

            var interview = IntegrationCreate.Interview(questionnaireId, questionnaireRepository, statePrototypeProvider, optionRepo);

            ApplyAllEvents(interview, events);

            return interview;
        }

        protected static Interview SetupInterview(AssemblyLoadContext assemblyLoadContext, QuestionnaireDocument questionnaireDocument, IQuestionOptionsRepository optionsRepository = null)
        {
            return SetupInterviewWithExpressionStorage(assemblyLoadContext, questionnaireDocument, null, optionsRepository);
        }

        protected static Interview SetupInterview(AssemblyLoadContext assemblyLoadContext, QuestionnaireCodeGenerationPackage package, IQuestionOptionsRepository optionsRepository = null)
        {
            return SetupInterviewWithExpressionStorage(assemblyLoadContext, package, null, optionsRepository);
        }

        protected static Interview SetupInterview(
            AssemblyLoadContext assemblyLoadContext,
            QuestionnaireDocument questionnaireDocument,
            IEnumerable<object> events)
        {
            return SetupInterviewWithExpressionStorage(assemblyLoadContext, questionnaireDocument, events);
        }

        private static IInterviewExpressionStorage GetLatestExpressionStorage(
            AssemblyLoadContext assemblyLoadContext,
            QuestionnaireDocument questionnaireDocument)
        {
            IInterviewExpressionStorage state = GetInterviewExpressionStorage(assemblyLoadContext, questionnaireDocument);
            return state;
        }

        public static void ApplyAllEvents(Interview interview, IEnumerable<object> events)
        {
            if (events == null)
                return;

            foreach (var evnt in events)
            {
                interview.Apply((IEvent)evnt);
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

        protected static RosterVector[] GetChangedOptions(EventContext eventContext, Guid questionId, RosterVector rosterVector) => 
            eventContext
                .GetSingleEvent<LinkedOptionsChanged>()
                .ChangedLinkedQuestions
                .SingleOrDefault(x => x.QuestionId.Equals(Create.Identity(questionId, rosterVector)))
                ?.Options;

        public static T GetFirstEventByType<T>(IEnumerable<UncommittedEvent> events)
            where T : class
        {
            var firstTypedEvent = events.FirstOrDefault(b => b.Payload is T);

            return (T) firstTypedEvent?.Payload;
        }

        protected static Assembly CompileAssembly(AssemblyLoadContext assemblyLoadContext,
            QuestionnaireDocument questionnaireDocument, int engineVersion)
        {
            var package = new QuestionnaireCodeGenerationPackage(questionnaireDocument, null);
            return CompileAssembly(assemblyLoadContext, package, engineVersion);
        }

        protected static Assembly CompileAssembly(AssemblyLoadContext assemblyLoadContext, QuestionnaireCodeGenerationPackage package, int engineVersion)
        {
            var expressionProcessorGenerator =
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(),
                    IntegrationCreate.CodeGeneratorV2(),
                    new DynamicCompilerSettingsProvider());
            var emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(package, engineVersion, out var resultAssembly);

            var filePath = Path.GetTempFileName();

            if (!emitResult.Success || string.IsNullOrEmpty(resultAssembly))
                throw new Exception(
                    $"Errors on IInterviewExpressionState generation:{Environment.NewLine}"
                    + string.Join(Environment.NewLine, emitResult.Diagnostics.Select((d, i) => $"{i + 1}. {d.Message}")));

            File.WriteAllBytes(filePath, Convert.FromBase64String(resultAssembly));

            var compiledAssembly = assemblyLoadContext.LoadFromAssemblyPath(filePath);

            return compiledAssembly;
        }

        public static IInterviewExpressionStorage GetInterviewExpressionStorage(AssemblyLoadContext assemblyLoadContext,
            QuestionnaireDocument questionnaireDocument)
        {
            return GetInterviewExpressionStorage(assemblyLoadContext,
                new QuestionnaireCodeGenerationPackage(questionnaireDocument, null));
        }
        public static IInterviewExpressionStorage GetInterviewExpressionStorage(AssemblyLoadContext assemblyLoadContext, QuestionnaireCodeGenerationPackage package)
        {
            var compiledAssembly = CompileAssembly(assemblyLoadContext, package, 21);

            Type interviewExpressionStorageType =
                compiledAssembly.GetTypes()
                    .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionStorage)));

            if (interviewExpressionStorageType == null)
                throw new Exception("Type InterviewExpressionState was not found");


            if (!(Activator.CreateInstance(interviewExpressionStorageType) is IInterviewExpressionStorage interviewExpressionStorage))
                throw new Exception("Error on IInterviewExpressionState generation");

            return interviewExpressionStorage;
        }
    }
}
