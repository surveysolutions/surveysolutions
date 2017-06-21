using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Moq;
using NSubstitute;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ExpressionStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Tests.Abc.TestFactories
{
    public class AggregateRootFactory
    {
        public Interview Interview(Guid? interviewId = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null,
            QuestionnaireIdentity questionnaireId = null,
            ISubstitionTextFactory textFactory = null,
            IQuestionOptionsRepository questionOptionsRepository = null)
        {
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            questionnaireDocument.IsUsingExpressionStorage = true;

            var questionnaireDefaultRepository = Mock.Of<IQuestionnaireStorage>(repository =>
                repository.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == new PlainQuestionnaire(questionnaireDocument, 1, null) &&
                repository.GetQuestionnaireDocument(It.IsAny<QuestionnaireIdentity>()) == questionnaireDocument);

            var textFactoryMock = new Mock<ISubstitionTextFactory> {DefaultValue = DefaultValue.Mock};
            var interview = new Interview(questionnaireRepository ?? questionnaireDefaultRepository,
                expressionProcessorStatePrototypeProvider ?? Stub.InterviewExpressionStateProvider(),
                textFactory ?? textFactoryMock.Object);

            interview.SetId(interviewId ?? Guid.NewGuid());

            if (questionOptionsRepository != null)
            {
                Setup.InstanceToMockedServiceLocator<IQuestionOptionsRepository>(questionOptionsRepository);
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
            IPlainStorageAccessor<TranslationInstance> translationsStorage = null)
            => new Questionnaire(
                questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IQuestionnaireAssemblyAccessor>(),
                questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                translationsStorage ?? new InMemoryPlainStorageAccessor<TranslationInstance>());

        public StatefulInterview StatefulInterview(Guid interviewId, 
            Guid? questionnaireId = null,
            Guid? userId = null,
            QuestionnaireDocument questionnaire = null,
            bool shouldBeInitialized = true)
        {
            var interview = this.StatefulInterview(questionnaireId, userId, questionnaire, shouldBeInitialized);
            interview.SetId(interviewId);
            return interview;
        }

        public StatefulInterview StatefulInterview(Guid? questionnaireId = null,
            Guid? userId = null,
            QuestionnaireDocument questionnaire = null, 
            bool shouldBeInitialized = true)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();
            if (questionnaire != null)
            {
                questionnaire.IsUsingExpressionStorage = true;
                questionnaire.ExpressionsPlayOrder = Create.Service.ExpressionsPlayOrderProvider().GetExpressionsPlayOrder(questionnaire.AsReadOnly());
            }

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);
            //Mock.Of<IQuestionnaireStorage>(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire)

            var statefulInterview = new StatefulInterview(
                questionnaireRepository,
                CreateDefaultInterviewExpressionStateProvider(),
                Create.Service.SubstitionTextFactory());

            if (shouldBeInitialized)
            {
                var command = Create.Command.CreateInterview(Guid.Empty, userId ?? Guid.NewGuid(), Create.Entity.QuestionnaireIdentity(questionnaireId.Value, 1), DateTime.Now,
                    Guid.NewGuid(), null, null, null);
                statefulInterview.CreateInterviewWithPreloadedData(command);
            }

            return statefulInterview;
        }

        public StatefulInterview StatefulInterview(
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            Guid? userId = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider interviewExpressionStatePrototypeProvider = null,
            bool shouldBeInitialized = true)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();

            var defaultExpressionStatePrototypeProvider = CreateDefaultInterviewExpressionStateProvider();

            var statefulInterview = new StatefulInterview(
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewExpressionStatePrototypeProvider ?? defaultExpressionStatePrototypeProvider,
                Create.Service.SubstitionTextFactory());

            if (shouldBeInitialized)
            {
                var command = Create.Command.CreateInterview(Guid.Empty, userId ?? Guid.NewGuid(), Create.Entity.QuestionnaireIdentity(questionnaireId.Value, 1), DateTime.Now,
                    Guid.NewGuid(), null, null, null);
                statefulInterview.CreateInterviewWithPreloadedData(command);
            }

            return statefulInterview;
        }

        private static IInterviewExpressionStatePrototypeProvider CreateDefaultInterviewExpressionStateProvider()
        {
            //Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues,
            var expressionStorage = new Mock<IInterviewExpressionStorage>();
            var levelMock = new Mock<IInterviewLevel>();
            expressionStorage.Setup(x => x.GetLevel(It.IsAny<Identity>())).Returns(levelMock.Object);

            var expressionState = Stub<ILatestInterviewExpressionState>.WithNotEmptyValues;

            var defaultExpressionStatePrototypeProvider = Mock.Of<IInterviewExpressionStatePrototypeProvider>(_
                => _.GetExpressionStorage(It.IsAny<QuestionnaireIdentity>()) == expressionStorage.Object
                && _.GetExpressionState(It.IsAny<Guid>(), It.IsAny<long>()) == expressionState);

            return defaultExpressionStatePrototypeProvider;
        }
    }
}