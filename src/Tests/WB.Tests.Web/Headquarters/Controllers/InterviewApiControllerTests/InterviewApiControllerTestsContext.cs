using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers.Api;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewApiControllerTests
{
    [NUnit.Framework.TestOf(typeof(InterviewApiController))]
    internal class InterviewApiControllerTestsContext
    {
        protected static InterviewApiController CreateController(ICommandService commandService = null,
            IAuthorizedUser authorizedUser = null, ILogger logger = null,
            IAllInterviewsFactory allInterviewsViewFactory = null,
            IChangeStatusFactory changeStatusFactory = null)
        {
            return new InterviewApiController(
                authorizedUser ?? Mock.Of<IAuthorizedUser>(), 
                allInterviewsViewFactory ?? Stub<IAllInterviewsFactory>.WithNotEmptyValues,
                changeStatusFactory ?? Stub<IChangeStatusFactory>.WithNotEmptyValues,
                Mock.Of<IQuestionnaireStorage>());
        }
    }
}
