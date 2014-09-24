using Machine.Specifications;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.ChangeStatus;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveyManagement.Views.Revalidate;
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
            IViewFactory<ChangeStatusInputModel, ChangeStatusView> changeStatusFactory = null,
            IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView> revalidateInterviewViewFactory = null,
            IInterviewSummaryViewFactory interviewSummaryViewFactory = null, IPlainInterviewFileStorage plainFileRepository = null)
        {
            return new InterviewController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                logger ?? Mock.Of<ILogger>(),
                changeStatusFactory ?? Stub<IViewFactory<ChangeStatusInputModel, ChangeStatusView>>.WithNotEmptyValues,
                revalidateInterviewViewFactory ?? Mock.Of<IViewFactory<InterviewInfoForRevalidationInputModel, InterviewInfoForRevalidationView>>(),
                interviewSummaryViewFactory ?? Stub<IInterviewSummaryViewFactory>.WithNotEmptyValues,
                Mock.Of<IViewFactory<InterviewHistoryInputModel, InterviewHistoryView>>(), plainFileRepository ?? Mock.Of<IPlainInterviewFileStorage>());
        }
    }
}
