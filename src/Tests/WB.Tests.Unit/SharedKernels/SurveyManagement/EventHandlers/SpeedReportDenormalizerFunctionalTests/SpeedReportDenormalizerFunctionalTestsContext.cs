using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.SpeedReportDenormalizerFunctionalTests
{
    internal class SpeedReportDenormalizerFunctionalTestsContext
    {
        protected static InterviewSummaryCompositeDenormalizer CreateDenormalizer(
            IReadSideRepositoryWriter<InterviewSummary> interviewStatuses)
        {
            var defaultUserView = Create.Entity.UserViewLite(supervisorId: Guid.NewGuid());
            var userViewFactory = Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<Guid>()) == defaultUserView);
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument());
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
            return new InterviewSummaryCompositeDenormalizer(
                interviewStatuses ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                new TestInMemoryWriter<InterviewSummary, int>(),
                new InterviewSummaryDenormalizer(userViewFactory, questionnaireStorage),
                new StatusChangeHistoryDenormalizerFunctional(userViewFactory),
                new InterviewStatusTimeSpanDenormalizer(),
                Mock.Of<IInterviewStatisticsReportDenormalizer>(),
                new InterviewGeoLocationAnswersDenormalizer(null, questionnaireStorage), 
                new InterviewExportedCommentariesDenormalizer(userViewFactory, questionnaireStorage), 
                Create.Storage.NewMemoryCache());
        }
    }
}
