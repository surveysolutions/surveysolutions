using System;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Headquarters.Controllers;

namespace WB.Tests.Unit.Applications.Headquarters.DesignerQuestionnairesApiControllerTests
{
    [Subject(typeof(DesignerQuestionnairesApiController))]
    internal class DesignerQuestionnairesApiControllerTestsContext
    {
        protected static DesignerQuestionnairesApiController CreateDesignerQuestionnairesApiController(
            ICommandService commandService = null, IGlobalInfoProvider globalInfo = null, IStringCompressor zipUtils = null,
            ILogger logger = null, Func<IGlobalInfoProvider, RestCredentials> getDesignerUserCredentials = null, IRestService restService = null,
            ISupportedVersionProvider supportedVersionProvider = null)
        {
            return new DesignerQuestionnairesApiController(
                supportedVersionProvider ?? new Mock<ISupportedVersionProvider> { DefaultValue = DefaultValue.Mock }.Object,
                commandService ?? Mock.Of<ICommandService>(),
                globalInfo ?? new Mock<IGlobalInfoProvider> { DefaultValue = DefaultValue.Mock }.Object,
                zipUtils ?? new Mock<IStringCompressor> { DefaultValue = DefaultValue.Mock }.Object,
                logger ?? Mock.Of<ILogger>(),
                getDesignerUserCredentials ?? (_ => new Mock<RestCredentials> { DefaultValue = DefaultValue.Mock }.Object),
                restService ?? Mock.Of<IRestService>()
               );
        }
    }
}