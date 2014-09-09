using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Main.Core.View;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireControllerTests
{
    internal class QuestionnaireControllerTestContext
    {
        internal static QuestionnaireController CreateQuestionnaireController(
            ICommandService commandService = null,
            IMembershipUserService userHelper = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IQuestionnaireHelper questionnaireHelper = null,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null,
            IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons> sharedPersonsViewFactory = null,
            ILogger logger = null,
            IViewFactory<QuestionnaireViewInputModel, EditQuestionnaireView> editQuestionnaireViewFactory = null,
            IQuestionnaireInfoFactory questionnaireInfoFactory = null)
        {
            return new QuestionnaireController(commandService ?? Mock.Of<ICommandService>(),
                userHelper ?? Mock.Of<IMembershipUserService>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                questionnaireHelper ?? Mock.Of<IQuestionnaireHelper>(),
                questionnaireViewFactory ?? Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(),
                sharedPersonsViewFactory ?? Mock.Of<IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons>>(),
                logger ?? Mock.Of<ILogger>(),
                editQuestionnaireViewFactory ?? Mock.Of<IViewFactory<QuestionnaireViewInputModel, EditQuestionnaireView>>(),
                questionnaireInfoFactory ?? Mock.Of<IQuestionnaireInfoFactory>());
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