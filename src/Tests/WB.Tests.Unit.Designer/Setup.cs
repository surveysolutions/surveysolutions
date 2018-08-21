using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using Main.Core.Documents;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;

namespace WB.Tests.Unit.Designer
{
    internal static class Setup
    {
        public static IAttachmentService AttachmentsServiceForOneQuestionnaire(Guid questionnaireId, params AttachmentView[] attachments)
        {
            var attachmentServiceMock = new Mock<IAttachmentService>();

            attachmentServiceMock.Setup(x => x.GetAttachmentSizesByQuestionnaire(questionnaireId))
                .Returns(attachments.Select(y => new AttachmentSize { AttachmentId = y.Meta.AttachmentId, Size = y.Content.Size}).ToList());
            
            return attachmentServiceMock.Object;
        }


        public static void InstanceToMockedServiceLocator<TInstance>(TInstance instance)
        {
            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<TInstance>())
                .Returns(instance);
        }

        public static IDesignerEngineVersionService DesignerEngineVersionService(bool isClientVersionSupported = true, bool isQuestionnaireVersionSupported = true, int questionnaireContentVersion = 9)
        {
            return Mock.Of<IDesignerEngineVersionService>(_ 
                => _.IsClientVersionSupported(Moq.It.IsAny<int>()) == isClientVersionSupported
                && _.GetListOfNewFeaturesForClient(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<int>()) == (isQuestionnaireVersionSupported ? new string[0] : new []{"New questionnaire feature"})
                && _.GetQuestionnaireContentVersion(Moq.It.IsAny<QuestionnaireDocument>()) == questionnaireContentVersion);
        }

        public static void CommandApiControllerToAcceptAttachment(
            ApiController controller, 
            byte[] fileContent, 
            MediaTypeHeaderValue contentType, 
            string serializedCommand,
            string fileParamName = "file",
            string commandParamName = "command")
        {
            var binaryContent = new ByteArrayContent(fileContent);

            binaryContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                Name = fileParamName
            };
            binaryContent.Headers.ContentType = contentType;

            var commandContent = new StringContent(serializedCommand);
            commandContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = commandParamName
            };

            var multipartContent = new MultipartContent { binaryContent, commandContent };

            var request = new HttpRequestMessage
            {
                Content = multipartContent
            };
            request.SetConfiguration(new HttpConfiguration());

            var controllerContext = new HttpControllerContext
            {
                Request = request
            };

            controller.ControllerContext = controllerContext;
        }

        public static void ServiceLocatorForAttachmentService(IPlainStorageAccessor<AttachmentContent> attachmentContentStorage, IPlainStorageAccessor<AttachmentMeta> attachmentMetaStorage)
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IPlainStorageAccessor<AttachmentContent>>())
                .Returns(attachmentContentStorage);

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IPlainStorageAccessor<AttachmentMeta>>())
                .Returns(attachmentMetaStorage);

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        public static IPlainKeyValueStorage<QuestionnaireStateTracker> QuestionnaireStateTrackerForOneQuestionnaire()
        {
            return Mock.Of<IPlainKeyValueStorage<QuestionnaireStateTracker>>(_ => 
                _.GetById(It.IsAny<string>()) == Create.QuestionnaireStateTacker());
        }

        public static IQuestionnaireTranslator QuestionnaireTranslator(QuestionnaireDocument questionnaireDocument, ITranslation translation, QuestionnaireDocument translatedQuestionnaireDocument)
        {
            var serviceMock = new Mock<IQuestionnaireTranslator>();
            serviceMock.Setup(x => x.Translate(questionnaireDocument, translation))
                 .Returns(translatedQuestionnaireDocument);

            return serviceMock.Object;
        }

        public static void ServiceLocatorForCustomWebApiAuthorizeFilter(
            IMembershipUserService membershipUserService = null)
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };

            serviceLocatorMock
                .Setup(locator => locator.GetInstance<IMembershipUserService>())
                .Returns(membershipUserService ?? Mock.Of<IMembershipUserService>());

            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);
        }

        public static HttpActionContext HttpActionContextWithOutAllowAnonymousOnAction()
        {
            var emptyAllowAnonymousList = new Collection<AllowAnonymousAttribute>();
            var controllerDescriptor = Mock.Of<HttpControllerDescriptor>(c => c.GetCustomAttributes<AllowAnonymousAttribute>() == emptyAllowAnonymousList);
            var actionDescriptor = Mock.Of<HttpActionDescriptor>(a => a.ControllerDescriptor == controllerDescriptor
                && a.GetCustomAttributes<AllowAnonymousAttribute>() == emptyAllowAnonymousList);
            return Mock.Of<HttpActionContext>(c => c.ActionDescriptor == actionDescriptor);
        }

        public static void HttpContextWithIsAuthenticatedFlag()
        {
            var httpRequest = new HttpRequest("", "http://same-url/", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            IIdentity identity = Mock.Of<IIdentity>(i => i.IsAuthenticated == true);
            IPrincipal user = Mock.Of<IPrincipal>(u => u.Identity == identity);
            httpContext.User = user;

            HttpContext.Current = httpContext;
        }
    }
}
