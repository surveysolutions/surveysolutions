using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using MvvmCross.Tests;
using Ncqrs.Eventing;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Services.CodeGeneration;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport.Preloading;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Enumerator.Native.Questionnaire.Impl;
using WB.Tests.Abc;

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

            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(repository
                => repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == Create.Entity.PlainQuestionnaire(questionnaireDocument, 1));

            SetUp.InstanceToMockedServiceLocator(questionnaireRepository);
            var questionOptionsRepository = new QuestionnaireQuestionOptionsRepository();
            SetUp.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(questionOptionsRepository);

            var state = GetLatestExpressionStorage(questionnaireDocument);

            var statePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionStorage(It.IsAny<QuestionnaireIdentity>()) == state);

            var interview = IntegrationCreate.PreloadedInterview(
                preloadedData, questionnaireId, questionnaireRepository, statePrototypeProvider);

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
            QuestionnaireDocument questionnaireDocument, 
            IEnumerable<object> events = null, 
            ILatestInterviewExpressionState precompiledState = null, 
            bool useLatestEngine = true,
            List<InterviewAnswer> answers = null,
            QuestionnaireIdentity questionnaireIdentity = null,
            IQuestionnaireStorage questionnaireStorage = null,
            IQuestionOptionsRepository questionOptionsRepository = null)
        {
            return SetupStatefullInterviewWithExpressionStorage(questionnaireDocument, events, answers, questionnaireIdentity, questionnaireStorage, questionOptionsRepository);
        }

        protected static StatefulInterview SetupStatefullInterviewWithExpressionStorage(
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

            var state = GetLatestExpressionStorage(questionnaireDocument);

            var statePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionStorage(It.IsAny<QuestionnaireIdentity>()) == state);

            var questionOptionsRepository = optionsRepository ??  new QuestionnaireQuestionOptionsRepository();

            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1 , questionOptionsRepository: questionOptionsRepository);

            var questionnaireRepository = questionnaireStorage ?? Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                questionnaireIdentity.QuestionnaireId,
                questionnaire,
                questionnaireIdentity.Version);

            SetUp.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(questionOptionsRepository);

            var interview = IntegrationCreate.StatefulInterview(
                questionnaireIdentity,
                expressionProcessorStatePrototypeProvider: statePrototypeProvider,
                answersOnPrefilledQuestions: answers,
                questionnaireRepository: questionnaireRepository,
                questionOptionsRepository: questionOptionsRepository);

            ApplyAllEvents(interview, events);

            return interview;
        }


        protected static StatefulInterview SetupStatefullInterviewWithExpressionStorageWithoutCreate(
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

            var state = GetLatestExpressionStorage(questionnaireDocument);

            var statePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionStorage(It.IsAny<QuestionnaireIdentity>()) == state);

            var questionnaireRepository = questionnaireStorage ?? Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(
                                              questionnaireIdentity.QuestionnaireId,
                                              Create.Entity.PlainQuestionnaire(questionnaireDocument),
                                              questionnaireIdentity.Version);

            var questionOptionsRepository = new QuestionnaireQuestionOptionsRepository();
            SetUp.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(questionOptionsRepository);

            var interview = new StatefulInterview(
                //questionnaireRepository,
                //statePrototypeProvider,
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder()
                //,questionOptionsRepository
                );

            return interview;
        }

        protected static Interview SetupInterviewWithExpressionStorage(
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

            var optionRepo = new QuestionnaireQuestionOptionsRepository();
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument, 1, null, null, optionRepo);
            
            var questionnaireRepository = Mock.Of<IQuestionnaireStorage>(repository
                => repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
            
            SetUp.InstanceToMockedServiceLocator(questionnaireRepository);
            SetUp.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(optionRepo);

            var state = GetLatestExpressionStorage(questionnaireDocument);
            var statePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(a => a.GetExpressionStorage(It.IsAny<QuestionnaireIdentity>()) == state);

            var interview = IntegrationCreate.Interview(questionnaireId, questionnaireRepository, statePrototypeProvider, optionRepo);

            ApplyAllEvents(interview, events);

            return interview;
        }

        protected static Interview SetupInterview(QuestionnaireDocument questionnaireDocument)
        {
            return SetupInterviewWithExpressionStorage(questionnaireDocument, null);
        }

        protected static Interview SetupInterview(
            QuestionnaireDocument questionnaireDocument,
            IEnumerable<object> events)
        {
            return SetupInterviewWithExpressionStorage(questionnaireDocument, events);
        }

        private static IInterviewExpressionStorage GetLatestExpressionStorage(
            QuestionnaireDocument questionnaireDocument,
            IInterviewExpressionStorage precompiledState = null)
        {
            IInterviewExpressionStorage state = precompiledState ?? GetInterviewExpressionStorage(questionnaireDocument);
            return state;
        }

        private static ILatestInterviewExpressionState GetLatestInterviewExpressionState(
            QuestionnaireDocument questionnaireDocument, 
            ILatestInterviewExpressionState precompiledState = null)
        {
            ILatestInterviewExpressionState state = precompiledState ?? GetInterviewExpressionState(questionnaireDocument);
            return state;
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

        protected static Assembly CompileAssemblyUsingQuestionnaireEngine(QuestionnaireDocument questionnaireDocument)
            => CompileAssembly(questionnaireDocument, IntegrationCreate.DesignerEngineVersionService().GetQuestionnaireContentVersion(questionnaireDocument));

        protected static Assembly CompileAssemblyUsingLatestEngine(QuestionnaireDocument questionnaireDocument)
            => CompileAssembly(questionnaireDocument, IntegrationCreate.DesignerEngineVersionService().LatestSupportedVersion);

        protected static Assembly CompileAssembly(QuestionnaireDocument questionnaireDocument, int engineVersion)
        {
            var expressionProcessorGenerator =
                new QuestionnaireExpressionProcessorGenerator(
                    new RoslynCompiler(),
                    IntegrationCreate.CodeGenerator(),
                    IntegrationCreate.CodeGeneratorV2(),
                    new DynamicCompilerSettingsProvider());

            var emitResult = expressionProcessorGenerator.GenerateProcessorStateAssembly(questionnaireDocument, engineVersion, out var resultAssembly);

            var filePath = Path.GetTempFileName();

            if (!emitResult.Success || string.IsNullOrEmpty(resultAssembly))
                throw new Exception(
                    $"Errors on IInterviewExpressionState generation:{Environment.NewLine}"
                    + string.Join(Environment.NewLine, emitResult.Diagnostics.Select((d, i) => $"{i + 1}. {d.Message}")));

            File.WriteAllBytes(filePath, Convert.FromBase64String(resultAssembly));

            var compiledAssembly = Assembly.LoadFrom(filePath);

            return compiledAssembly;
        }

        public static IInterviewExpressionStorage GetInterviewExpressionStorage(QuestionnaireDocument questionnaireDocument)
        {
            var compiledAssembly = CompileAssembly(questionnaireDocument, 21);

            Type interviewExpressionStorageType =
                compiledAssembly.GetTypes()
                    .FirstOrDefault(type => type.GetInterfaces().Contains(typeof(IInterviewExpressionStorage)));

            if (interviewExpressionStorageType == null)
                throw new Exception("Type InterviewExpressionState was not found");


            if (!(Activator.CreateInstance(interviewExpressionStorageType) is IInterviewExpressionStorage interviewExpressionStorage))
                throw new Exception("Error on IInterviewExpressionState generation");

            return interviewExpressionStorage;
        }

        public static ILatestInterviewExpressionState GetInterviewExpressionState(QuestionnaireDocument questionnaireDocument, bool useLatestEngine = true)
        {
            var compiledAssembly = useLatestEngine ? CompileAssemblyUsingLatestEngine(questionnaireDocument) : CompileAssemblyUsingQuestionnaireEngine(questionnaireDocument);

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
    }
}
