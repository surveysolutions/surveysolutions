using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireControllerTests
{
    internal class QuestionnaireControllerTestContext
    {
        internal static QuestionnaireController CreateQuestionnaireController(
            ICommandService commandService = null,
            IMembershipUserService userHelper = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IQuestionnaireHelper questionnaireHelper = null,
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            ILogger logger = null,
            IQuestionnaireInfoFactory questionnaireInfoFactory = null)
        {
            return new QuestionnaireController(commandService ?? Mock.Of<ICommandService>(),
                userHelper ?? Mock.Of<IMembershipUserService>(),
                questionnaireHelper ?? Mock.Of<IQuestionnaireHelper>(),
                questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                Mock.Of<IFileSystemAccessor>(),
                logger ?? Mock.Of<ILogger>(),
                questionnaireInfoFactory ?? Mock.Of<IQuestionnaireInfoFactory>(),
                Mock.Of<IQuestionnaireChangeHistoryFactory>(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IQuestionnaireInfoViewFactory>(),
                Mock.Of<IPublicFoldersStorage>(),
                Mock.Of<IQuestionnaireHistoryVersionsService>());
        }

        protected static void SetControllerContextWithSession(Controller controller, string key, object value)
        {
            var httpRequest = new HttpRequest("", "http://localhost/", "");
            var httpResponce = new HttpResponse(new StringWriter());
            var context = new HttpContext(httpRequest, httpResponce);

            var sessionContainer = new HttpSessionStateContainer("options", new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(), 10, true,
                HttpCookieMode.AutoDetect,
                SessionStateMode.InProc, false);

            context.Items["AspSession"]=(typeof(HttpSessionState).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null, CallingConventions.Standard,
                new[] { typeof(HttpSessionStateContainer) },
                null)
                .Invoke(new object[] { sessionContainer }));

            HttpContext.Current = context;

            controller.ControllerContext = new ControllerContext(new HttpContextWrapper(context), new RouteData(), controller);

            controller.Session[key] = value;
        }

        protected static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
