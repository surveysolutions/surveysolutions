using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        public static InterviewSummaryCompositeDenormalizer CreateDenormalizer(IReadSideRepositoryWriter<InterviewSummary> interviewStatuses = null)
        {
            var defaultUserView = Create.Entity.UserView(supervisorId: Guid.NewGuid());
            var userViewFactory = Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<UserViewInputModel>()) == defaultUserView);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument();
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), It.IsAny<long>()) == questionnaireDocument);
            return new InterviewSummaryCompositeDenormalizer(
                interviewStatuses ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                new InterviewSummaryDenormalizer(userViewFactory, questionnaireStorage), 
                new StatusChangeHistoryDenormalizerFunctional(userViewFactory),
                new InterviewStatusTimeSpanDenormalizer());
        }

        public static StatusChangeHistoryDenormalizerFunctional CreateStatusChangeHistoryDenormalizerFunctional()
        {
            var defaultUserView = Create.Entity.UserView(supervisorId: Guid.NewGuid());
            var userViewFactory = Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<UserViewInputModel>()) == defaultUserView);
            return new StatusChangeHistoryDenormalizerFunctional(userViewFactory);
        }
    }
}
