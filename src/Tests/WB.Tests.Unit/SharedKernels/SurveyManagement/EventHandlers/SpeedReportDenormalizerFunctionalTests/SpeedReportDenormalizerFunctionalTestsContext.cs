using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.SpeedReportDenormalizerFunctionalTests
{
    internal class SpeedReportDenormalizerFunctionalTestsContext
    {
        protected static InterviewSummaryCompositeDenormalizer CreateDenormalizer(
            IReadSideRepositoryWriter<InterviewSummary> interviewStatuses,
            IQuestionnaireStorage questionnaireStorage = null)
        {
            var defaultUserView = Create.Entity.UserViewLite(supervisorId: Guid.NewGuid());
            var userViewFactory = Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<Guid>()) == defaultUserView);
            var questionnaire = Mock.Of<IQuestionnaire>(x => x.GetPrefilledEntities() == new ReadOnlyCollection<Guid>(Array.Empty<Guid>()));
            var questionnaireStorage1 = questionnaireStorage ?? 
                                        Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire
                                                                                 && _.GetQuestionnaireOrThrow(Moq.It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
            
            var questionnaireItems = Mock.Of<IPlainStorageAccessor<QuestionnaireCompositeItem>>();
            Mock.Get(questionnaireItems)
                .Setup(reader => reader.Query(It.IsAny<Func<IQueryable<QuestionnaireCompositeItem>, List<QuestionnaireCompositeItem>>>()))
                .Returns(new List<QuestionnaireCompositeItem>());

            return new InterviewSummaryCompositeDenormalizer(new EventBusSettings(),
                interviewStatuses ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                new InterviewSummaryDenormalizer(userViewFactory, questionnaireStorage1, Create.Storage.NewMemoryCache()),
                new StatusChangeHistoryDenormalizerFunctional(userViewFactory),
                new InterviewStatusTimeSpanDenormalizer(),
                Mock.Of<IInterviewStatisticsReportDenormalizer>(),
                new InterviewGeoLocationAnswersDenormalizer(questionnaireStorage1), 
                new InterviewExportedCommentariesDenormalizer(userViewFactory, questionnaireStorage1),
                new InterviewDynamicReportAnswersDenormalizer(questionnaireStorage1, questionnaireItems));
        }
    }
}
