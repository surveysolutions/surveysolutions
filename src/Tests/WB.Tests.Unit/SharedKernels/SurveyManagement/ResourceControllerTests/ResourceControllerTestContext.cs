using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ResourceControllerTests
{
    [Subject(typeof(ResourceController))]
    internal class ResourceControllerTestContext
    {
        protected static ResourceController CreateController(ICommandService commandService = null,
            IGlobalInfoProvider globalInfoProvider = null,
            ILogger logger = null, IPlainInterviewFileStorage plainInterviewFileStorage = null)
        {
            return new ResourceController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfoProvider ?? Mock.Of<IGlobalInfoProvider>(),
                logger ?? Mock.Of<ILogger>(), plainInterviewFileStorage ?? Mock.Of<IPlainInterviewFileStorage>());
        }
    }
}
