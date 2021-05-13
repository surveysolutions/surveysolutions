using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using Ncqrs;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Tests.Abc.TestFactories
{
    public class AggregateRootFactory
    {
        public Interview Interview(Guid? interviewId = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IInterviewExpressionStorageProvider expressionProcessorStorageProvider = null,
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
            
            serviceLocator.Setup(x => x.GetInstance<IQuestionOptionsRepository>())
                .Returns(Mock.Of<IQuestionOptionsRepository>);

            var interview = new Interview(
                textFactory ?? textFactoryMock.Object,
                Create.Service.InterviewTreeBuilder(),
                Create.Storage.QuestionnaireQuestionOptionsRepository());

            interview.ServiceLocatorInstance = serviceLocator.Object;

            interview.SetId(interviewId ?? Guid.NewGuid());

            if (questionOptionsRepository != null)
            {
                SetUp.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(questionOptionsRepository);
            }

            return interview;
        }

        public Questionnaire Questionnaire(
            IQuestionnaireStorage questionnaireStorage = null,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null,
            IFileSystemAccessor fileSystemAccessor = null,
            IPlainStorageAccessor<Enumerator.Native.Questionnaire.TranslationInstance> translationsStorage = null,
            IReusableCategoriesStorage categoriesStorage = null,
            IPlainKeyValueStorage<QuestionnairePdf> pdfStorage = null,
            IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage = null)
            => new Questionnaire(
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IQuestionnaireAssemblyAccessor>(),
                questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                translationsStorage ?? new InMemoryPlainStorageAccessor<Enumerator.Native.Questionnaire.TranslationInstance>(),
                categoriesStorage ?? Mock.Of<IReusableCategoriesStorage>(),
                pdfStorage ?? new InMemoryPlainStorageAccessor<QuestionnairePdf>(),
                questionnaireBackupStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireBackup>>());

        public StatefulInterview StatefulInterview(Guid interviewId, 
            Guid? questionnaireId = null,
            Guid? userId = null,
            Guid? supervisorId = null,
            QuestionnaireDocument questionnaire = null,
            bool shouldBeInitialized = true,
            IClock clock  = null)
        {
            var interview = this.StatefulInterview(questionnaireId, userId, supervisorId, questionnaire,
                shouldBeInitialized, clock: clock);
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
            List<string> protectedAnswers = null,
            IQuestionOptionsRepository optionsRepository = null,
            Type expressionStorageType = null,
            IClock clock = null)
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

            var questionOptionsRepository = optionsRepository ?? Mock.Of<IQuestionOptionsRepository>();
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(
                questionnaire ?? Create.Entity.QuestionnaireDocumentWithOneQuestion(), 1,
                questionOptionsRepository: questionOptionsRepository);

            plainQuestionnaire.ExpressionStorageType = expressionStorageType ?? typeof(DummyInterviewExpressionStorage);

            var questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(plainQuestionnaire, questionnaire ?? 
                Create.Entity.QuestionnaireDocumentWithOneQuestion());

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<IQuestionnaireStorage>())
                .Returns(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>());
            
            serviceLocator.Setup(x => x.GetInstance<IQuestionOptionsRepository>()).Returns(questionOptionsRepository);

            var statefulInterview = new StatefulInterview(
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder(),
                Create.Storage.QuestionnaireQuestionOptionsRepository());

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
            
            serviceLocator.Setup(x => x.GetInstance<IQuestionOptionsRepository>())
                .Returns(questionOptionsRepository ?? Mock.Of<IQuestionOptionsRepository>());

            var statefulInterview = new StatefulInterview(
                Create.Service.SubstitutionTextFactory(),
                Create.Service.InterviewTreeBuilder(),
                Create.Storage.QuestionnaireQuestionOptionsRepository());

            statefulInterview.ServiceLocatorInstance = serviceLocator.Object;

            if (shouldBeInitialized)
            {
                var command = Create.Command.CreateInterview(Guid.Empty, userId ?? Guid.NewGuid(), Create.Entity.QuestionnaireIdentity(questionnaireId.Value, 1),
                    Guid.NewGuid(), null, null, null);
                statefulInterview.CreateInterview(command);
            }

            return statefulInterview;
        }

        public AssignmentAggregateRoot AssignmentAggregateRoot()
        {
            return new AssignmentAggregateRoot();

        }
    }
}
