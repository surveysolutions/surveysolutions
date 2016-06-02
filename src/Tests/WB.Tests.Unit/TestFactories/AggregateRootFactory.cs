using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler.WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.TestFactories
{
    internal class AggregateRootFactory
    {
        public Interview Interview(Guid? interviewId = null, IPlainQuestionnaireRepository questionnaireRepository = null,
            IInterviewExpressionStatePrototypeProvider expressionProcessorStatePrototypeProvider = null)
        {
            var interview = new Interview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                expressionProcessorStatePrototypeProvider ?? Stub.InterviewExpressionStateProvider());

            interview.SetId(interviewId ?? Guid.NewGuid());

            return interview;
        }

        public Questionnaire Questionnaire(
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null,
            IFileSystemAccessor fileSystemAccessor = null)
            => new Questionnaire(
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                new ReferenceInfoForLinkedQuestionsFactory(),
                new QuestionnaireRosterStructureFactory(),
                questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                Mock.Of<IPlainKeyValueStorage<ReferenceInfoForLinkedQuestions>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireRosterStructure>>(),
                Mock.Of<IPlainKeyValueStorage<QuestionnaireQuestionsInfo>>(),
                fileSystemAccessor ?? Mock.Of<IFileSystemAccessor>());

        public StatefulInterview StatefulInterview(
            Guid? questionnaireId = null,
            long? questionnaireVersion = null,
            Guid? userId = null,
            IPlainQuestionnaireRepository questionnaireRepository = null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();
            var statefulInterview = new StatefulInterview(
                Mock.Of<ILogger>(),
                questionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(),
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues)
            {
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId.Value, questionnaireVersion ?? 1),
            };

            statefulInterview.Apply(new InterviewCreated(userId ?? Guid.NewGuid(), questionnaireId.Value, questionnaireVersion ?? 1));

            return statefulInterview;
        }

        public StatefulInterview StatefulInterview(Guid? questionnaireId = null, Guid? userId = null, IQuestionnaire questionnaire = null)
        {
            questionnaireId = questionnaireId ?? Guid.NewGuid();

            var statefulInterview = new StatefulInterview(
                Mock.Of<ILogger>(),
                Mock.Of<IPlainQuestionnaireRepository>(x => x.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>()) == questionnaire),
                Stub<IInterviewExpressionStatePrototypeProvider>.WithNotEmptyValues)
            {
                QuestionnaireIdentity = new QuestionnaireIdentity(questionnaireId.Value, 1),
            };

            statefulInterview.Apply(new InterviewCreated(userId ?? Guid.NewGuid(), questionnaireId.Value, 1));

            return statefulInterview;
        }
    }
}