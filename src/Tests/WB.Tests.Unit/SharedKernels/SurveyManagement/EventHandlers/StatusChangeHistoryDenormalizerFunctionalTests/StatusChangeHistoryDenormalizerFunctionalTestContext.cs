using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    internal class StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        public static StatusChangeHistoryDenormalizerFunctional CreateDenormalizer(IReadSideRepositoryWriter<InterviewStatuses> interviewStatuses = null)
        {
            var defaultUserView = Create.Entity.UserView(supervisorId: Guid.NewGuid());
            return
                new StatusChangeHistoryDenormalizerFunctional(
                    interviewStatuses ?? Mock.Of<IReadSideRepositoryWriter<InterviewStatuses>>(),
                    Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<UserViewInputModel>()) == defaultUserView),
                    Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(
                        _ => _.GetById(Moq.It.IsAny<string>()) == new InterviewSummary()));
        }
    }
}
