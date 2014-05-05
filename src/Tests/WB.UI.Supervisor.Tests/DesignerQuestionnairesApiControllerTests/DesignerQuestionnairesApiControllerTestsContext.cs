using System;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernels.SurveyManagement.Services;
using WB.UI.Supervisor.Controllers;
using WB.UI.Supervisor.DesignerPublicService;

namespace WB.UI.Supervisor.Tests.DesignerQuestionnairesApiControllerTests
{
    [Subject(typeof(DesignerQuestionnairesApiController))]
    internal class DesignerQuestionnairesApiControllerTestsContext
    {
        protected static DesignerQuestionnairesApiController CreateDesignerQuestionnairesApiController(
            ICommandService commandService = null, IGlobalInfoProvider globalInfo = null, IStringCompressor zipUtils = null,
            ILogger logger = null, Func<IGlobalInfoProvider, IPublicService> getDesignerService = null,
            ISupportedVersionProvider supportedVersionProvider = null)
        {
            return new DesignerQuestionnairesApiController(
                supportedVersionProvider ?? new Mock<ISupportedVersionProvider> { DefaultValue = DefaultValue.Mock }.Object,
                commandService ?? Mock.Of<ICommandService>(),
                globalInfo ?? new Mock<IGlobalInfoProvider> { DefaultValue = DefaultValue.Mock }.Object,
                zipUtils ?? new Mock<IStringCompressor> { DefaultValue = DefaultValue.Mock }.Object,
                logger ?? Mock.Of<ILogger>(),
                getDesignerService ?? (_ => new Mock<IPublicService> { DefaultValue = DefaultValue.Mock }.Object)
               );
        }
    }
}