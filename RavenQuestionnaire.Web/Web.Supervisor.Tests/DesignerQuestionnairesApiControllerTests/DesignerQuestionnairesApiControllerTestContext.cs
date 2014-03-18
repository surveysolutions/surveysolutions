using System;
using System.IO;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using Questionnaire.Core.Web.Helpers;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernel.Utils.Compression;
using Web.Supervisor.Controllers;
using Web.Supervisor.DesignerPublicService;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace Web.Supervisor.Tests.DesignerQuestionnairesApiControllerTests
{
    internal class DesignerQuestionnairesApiControllerTestContext
    {
        protected static DesignerQuestionnairesApiController CreateDesignerQuestionnairesApiController(
            ICommandService commandService = null,
            IGlobalInfoProvider globalInfo = null,
            ISupportedVersionProvider supportedVersionProvider = null,
            IStringCompressor zipUtils = null,
            ILogger logger = null,
            IPublicService designerService = null)
        {
            HttpContext.Current = FakeHttpContext();
            var controller = new DesignerQuestionnairesApiController(
                commandService ?? Mock.Of<ICommandService>(),
                globalInfo ?? Mock.Of<IGlobalInfoProvider>(),
                supportedVersionProvider ?? Mock.Of<ISupportedVersionProvider>(),
                zipUtils ?? Mock.Of<IStringCompressor>(),
                logger ?? Mock.Of<ILogger>()
                )
            {
                DesignerService = designerService ?? Mock.Of<IPublicService>()
            };

            return controller;
        }

        public static HttpContext FakeHttpContext()
        {
            var uri = new Uri("http://hello.world.ru");
            var httpRequest = new HttpRequest(string.Empty, uri.ToString(),
                                                uri.Query.TrimStart('?'));
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id",
                                            new SessionStateItemCollection(),
                                            new HttpStaticObjectsCollection(),
                                            10, true, HttpCookieMode.AutoDetect,
                                            SessionStateMode.InProc, false);

            SessionStateUtility.AddHttpSessionStateToContext(httpContext, sessionContainer);

            return httpContext;
        }
    }
}
