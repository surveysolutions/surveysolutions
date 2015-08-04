using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Revalidate;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.Core.Synchronization;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.InterviewControllerTests
{
    internal class InterviewControllerTestsContext
    {
        protected static InterviewController CreateController(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfoProvider = null,
            ILogger logger = null,
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory = null,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory = null,
            IInterviewSummaryViewFactory interviewSummaryViewFactory = null,
            IInterviewDetailsViewFactory interviewDetailsViewFactory = null,
            IIncomingSyncPackagesQueue incomingSyncPackagesQueue = null)
        {
            return new InterviewController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                logger ?? Mock.Of<ILogger>(),
                changeStatusFactory ?? Stub<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>.WithNotEmptyValues,
                revalidateInterviewViewFactory ?? Mock.Of<IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView>>(),
                interviewSummaryViewFactory ?? Stub<IInterviewSummaryViewFactory>.WithNotEmptyValues,
                Mock.Of<IInterviewHistoryFactory>(),
                interviewDetailsViewFactory ?? Mock.Of<IInterviewDetailsViewFactory>());
        }
    }
}
