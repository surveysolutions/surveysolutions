using System;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Repositories;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Translations;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Tests.Unit.TestFactories
{
    internal class AggregateRootFactory
    {
        public Interview Interview(Guid? interviewId = null,
            IQuestionnaireStorage questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null,
            QuestionnaireIdentity questionnaireId = null,
            ISubstitionTextFactory textFactory = null,
            IQuestionOptionsRepository questionOptionsRepository = null)
        {
            var textFactoryMock = new Mock<ISubstitionTextFactory> {DefaultValue = DefaultValue.Mock};
            var interview = new Interview(questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
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
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>(),
                translationsStorage ?? new InMemoryPlainStorageAccessor<TranslationInstance>());


        public StatefulInterview StatefulInterview(Guid? questionnaireId = null, Guid? userId = null, IQuestionnaire questionnaire = null, bool shouldBeInitialized = true)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();

            var statefulInterview = new StatefulInterview(
                Mock.Of<IQuestionnaireStorage>(x => x.GetQuestionnaire(It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire),
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues,
                Create.Service.SubstitionTextFactory());

            if (shouldBeInitialized)
            {
                statefulInterview.CreateInterviewOnClient(Create.Entity.QuestionnaireIdentity(questionnaireId.Value, 1), Guid.NewGuid(), DateTime.Now, userId ?? Guid.NewGuid());
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
            var statefulInterview = new StatefulInterview(
                questionnaireRepository ?? Mock.Of<IQuestionnaireStorage>(),
                interviewExpressionStatePrototypeProvider ??
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues,
                Create.Service.SubstitionTextFactory());

            if (shouldBeInitialized)
            {
                statefulInterview.CreateInterviewOnClient(Create.Entity.QuestionnaireIdentity(questionnaireId.Value, 1), Guid.NewGuid(), DateTime.Now, userId ?? Guid.NewGuid());
            }

            return statefulInterview;
        }
    }
}