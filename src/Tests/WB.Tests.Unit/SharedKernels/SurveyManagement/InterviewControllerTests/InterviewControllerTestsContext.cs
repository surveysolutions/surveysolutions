using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.InterviewHistory;
using WB.Core.BoundedContexts.Headquarters.Views.Revalidate;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class InterviewControllerTestsContext
    {
        protected static InterviewController CreateController(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfoProvider = null,
            ILogger logger = null,
            IChangeStatusFactory changeStatusFactory = null,
            IInterviewTroubleshootFactory revalidateInterviewViewFactory = null,
            IInterviewSummaryViewFactory interviewSummaryViewFactory = null,
            IInterviewDetailsViewFactory interviewDetailsViewFactory = null,
            IInterviewPackagesService incomingSyncPackagesQueue = null)
        {
            return new InterviewController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                logger ?? Mock.Of<ILogger>(),
                changeStatusFactory ?? Stub<IChangeStatusFactory>.WithNotEmptyValues,
                revalidateInterviewViewFactory ?? Mock.Of<IInterviewTroubleshootFactory>(),
                interviewSummaryViewFactory ?? Stub<IInterviewSummaryViewFactory>.WithNotEmptyValues,
                Mock.Of<IInterviewHistoryFactory>(),
                interviewDetailsViewFactory ?? Mock.Of<IInterviewDetailsViewFactory>());
        }
    }
}
