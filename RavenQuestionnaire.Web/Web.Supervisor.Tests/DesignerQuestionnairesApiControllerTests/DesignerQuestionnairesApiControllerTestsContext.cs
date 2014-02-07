using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Compression;
using Web.Supervisor.Controllers;
using Web.Supervisor.DesignerPublicService;

namespace Web.Supervisor.Tests.DesignerQuestionnairesApiControllerTests
{
    [Subject(typeof(DesignerQuestionnairesApiController))]
    internal class DesignerQuestionnairesApiControllerTestsContext
    {
        protected static DesignerQuestionnairesApiController CreateDesignerQuestionnairesApiController(
            ICommandService commandService = null, IGlobalInfoProvider globalInfo = null, IStringCompressor zipUtils = null,
            ILogger logger = null, Func<IGlobalInfoProvider, IPublicService> getDesignerService = null)
        {
            return new DesignerQuestionnairesApiController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfo ?? new Mock<IGlobalInfoProvider> { DefaultValue = DefaultValue.Mock }.Object,
                zipUtils ?? new Mock<IStringCompressor> { DefaultValue = DefaultValue.Mock }.Object,
                logger ?? Mock.Of<ILogger>(),
                getDesignerService ?? (_ => new Mock<IPublicService> { DefaultValue = DefaultValue.Mock }.Object));
        }
    }
}