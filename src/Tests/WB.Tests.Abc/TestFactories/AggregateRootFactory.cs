using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Tests.Abc.TestFactories
{
    public class AggregateRootFactory
    {
        public Interview Interview(Guid? interviewId = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null,
            QuestionnaireIdentity questionnaireId = null,
            ISubstitutionTextFactory textFactory = null,
            IQuestionOptionsRepository questionOptionsRepository = null)
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            questionnaireDocument.IsUsingExpressionStorage = true;

            var questionnaireDefaultRepository = Mock.Of<IQuestionnaireStorage>(repository =>
                repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == Create.Entity.PlainQuestionnaire(questionnaireDocument, 1) &&
                repository.GetQuestionnaireDocument(It.IsAny<QuestionnaireIdentity>()) == questionnaireDocument);

            var textFactoryMock = new Mock<ISubstitutionTextFactory> {DefaultValue = DefaultValue.Mock};

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IQuestionnaireStorage>())
                .Returns(questionnaireRepository ?? questionnaireDefaultRepository);

            serviceLocator.Setup(x => x.GetInstance<IInterviewExpressionStatePrototypeProvider>())
                .Returns( expressionProcessorStatePrototypeProvider ?? CreateDefaultInterviewExpressionStateProvider(null));

            serviceLocator.Setup(x => x.GetInstance<IQuestionOptionsRepository>())
                .Returns(Mock.Of<IQuestionOptionsRepository>);

            var interview = new Interview(
                textFactory ?? textFactoryMock.Object,
                Create.Service.InterviewTreeBuilder()
                );

            interview.ServiceLocatorInstance = serviceLocator.Object;

            interview.SetId(interviewId ?? Guid.NewGuid());

            if (questionOptionsRepository != null)
            {
                SetUp.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(questionOptionsRepository);
            }

            interview.Apply(Create.Event.InterviewCreated(
                questionnaireId: questionnaireId?.QuestionnaireId ?? Guid.NewGuid(),
                questionnaireVersion: questionnaireId?.Version ?? 1));

            return interview;
        }

        public Questionnaire Questionnaire(
            IQuestionnaireStorage questionnaireStorage = null,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IPlainStorageAccessor<Enumerator.Native.Questionnaire.TranslationInstance> translationsStorage = null)
            => new Questionnaire(
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IQuestionnaireAssemblyAccessor>(),
                questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                translationsStorage ?? new InMemoryPlainStorageAccessor<Enumerator.Native.Questionnaire.TranslationInstance>());

        public StatefulInterview StatefulInterview(Guid interviewId, 
            Guid? questionnaireId = null,
            Guid? userId = null,
            Guid? supervisorId = null,
            QuestionnaireDocument questionnaire = null,
            bool shouldBeInitialized = true)
        {
            var interview = this.StatefulInterview(questionnaireId, userId, supervisorId, questionnaire, shouldBeInitialized);
            interview.SetId(interviewId);
            return interview;
        }

        public StatefulInterview StatefulInterview(Guid? questionnaireId = null,
            Guid? userId = null,
            Guid? supervisorId = null,
            QuestionnaireDocument questionnaire = null, 
            bool shouldBeInitialized = true,
            Action<Mock<IInterviewLevel>> setupLevel = null,
            List<InterviewAnswer> answers = null,
            List<string> protectedAnswers = null)
        {
            questionnaireId = questionnaireId ?? questionnaire?.PublicKey ?? Guid.NewGuid();
            if (questionnaire != null)
            {
                questionnaire.IsUsingExpressionStorage = true;
                var playOrderProvider = Create.Service.ExpressionsPlayOrderProvider();
                var readOnlyQuestionnaireDocument = questionnaire.AsReadOnly();
                questionnaire.ExpressionsPlayOrder = playOrderProvider.GetExpressionsPlayOrder(readOnlyQuestionnaireDocument);
                questionnaire.DependencyGraph = playOrderProvider.GetDependencyGraph(readOnlyQuestionnaireDocument);
                questionnaire.ValidationDependencyGraph = playOrderProvider.GetValidationDependencyGraph(readOnlyQuestionnaireDocument);
            }

            var questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire ?? 
                Create.Entity.QuestionnaireDocumentWithOneQuestion());

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IQuestionnaireStorage>())
                .Returns(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>());

            serviceLocator.Setup(x => x.GetInstance<IInterviewExpressionStatePrototypeProvider>())
                .Returns(CreateDefaultInterviewExpressionStateProvider(setupLevel));

            serviceLocator.Setup(x => x.GetInstance<IQuestionOptionsRepository>())
                .Returns(Mock.Of<IQuestionOptionsRepository>);


            var statefulInterview = new StatefulInterview(
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder());

            statefulInterview.ServiceLocatorInstance = serviceLocator.Object;

            if (shouldBeInitialized)
            {
                var command = Create.Command.CreateInterview(Guid.Empty, userId ?? Guid.NewGuid(), Create.Entity.QuestionnaireIdentity(questionnaireId.Value, 1),
                    supervisorId ?? Guid.NewGuid(), InterviewKey.Parse("11-11-11-11"), null, answers, protectedAnswers);
                statefulInterview.CreateInterview(command);
            }

            return statefulInterview;
        }

        public StatefulInterview StatefulInterview(
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            Guid? userId = null,
            IQuestionnaireStorage questionnaireRepository = null,
            bool shouldBeInitialized = true,
            Action<Mock<IInterviewLevel>> setupLevel = null,
            IQuestionOptionsRepository questionOptionsRepository = null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IQuestionnaireStorage>())
                .Returns(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>());

            serviceLocator.Setup(x => x.GetInstance<IInterviewExpressionStatePrototypeProvider>())
                .Returns(CreateDefaultInterviewExpressionStateProvider(setupLevel));

            serviceLocator.Setup(x => x.GetInstance<IQuestionOptionsRepository>())
                .Returns(questionOptionsRepository ?? Mock.Of<IQuestionOptionsRepository>());

            var statefulInterview = new StatefulInterview(
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder()
                );
            statefulInterview.ServiceLocatorInstance = serviceLocator.Object;

            if (shouldBeInitialized)
            {
                var command = Create.Command.CreateInterview(Guid.Empty, userId ?? Guid.NewGuid(), Create.Entity.QuestionnaireIdentity(questionnaireId.Value, 1),
                    Guid.NewGuid(), null, null, null);
                statefulInterview.CreateInterview(command);
            }

            return statefulInterview;
        }

        private static IInterviewExpressionStatePrototypeProvider CreateDefaultInterviewExpressionStateProvider(Action<Mock<IInterviewLevel>> setupLevel = null)
        {
            //Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues,
            var expressionStorage = new Mock<IInterviewExpressionStorage>();
            var levelMock = new Mock<IInterviewLevel>();
            setupLevel?.Invoke(levelMock);
            expressionStorage.Setup(x => x.GetLevel(It.IsAny<Identity>())).Returns(levelMock.Object);

            var expressionState = Stub<ILatestInterviewExpressionState>.WithNotEmptyValues;

            var defaultExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(_
                => _.GetExpressionStorage(It.IsAny<QuestionnaireIdentity>()) == expressionStorage.Object
                && _.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == expressionState);

            return defaultExpressionStatePrototypeProvider;
        }
    }
}
