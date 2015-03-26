using System;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.BootstrapSupport;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers;
using WB.UI.Shared.Web.Membership;
using TemplateInfo = WB.Core.BoundedContexts.Designer.Services.TemplateInfo;

namespace WB.Tests.Unit.Applications.Designer
{
    [TestFixture]
    public class AdminControllerTests
    {
        [SetUp]
        public void Setup()
        {
            this.CommandServiceMock=new Mock<ICommandService>();
            this.ZipUtilsMock = new Mock<IStringCompressor>();
            this.ExportServiceMock = new Mock<IQuestionnaireExportService>();
            this.UserHelperMock = new Mock<IMembershipUserService>();
            this.questionnaireViewFactoryMock =  new Mock<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>();
            
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void Import_When_RequestContainsQuestionnirie_Then_ImportCommandExecutedAndRedirectToQuestionnairieController()
        {
            // arrange
            AdminController controller = this.CreateAdminController();

            Mock<HttpPostedFileBase> file = new Mock<HttpPostedFileBase>();

            var inputStream = new MemoryStream();
            using (var zip =new GZipStream(inputStream, CompressionMode.Compress, true))
            {
                zip.Write(new byte[] { 1 }, 0, 1);
            }

            file.Setup(x => x.ContentLength).Returns((int)inputStream.Length);
            file.Setup(x => x.InputStream).Returns(inputStream);

            this.ZipUtilsMock.Setup(x => x.DecompressGZip<IQuestionnaireDocument>(file.Object.InputStream))
                        .Returns(new QuestionnaireDocument());
            this.UserHelperMock.Setup(x => x.WebUser.UserId).Returns(Guid.NewGuid);

            // act
            var actionResult = (RedirectToRouteResult)controller.Import(file.Object);

            // assert
            this.CommandServiceMock.Verify(x => x.Execute(It.IsAny<ImportQuestionnaireCommand>(), It.IsAny<string>()), Times.Once());

            Assert.AreEqual(actionResult.RouteValues["action"],"Index");
            Assert.AreEqual(actionResult.RouteValues["controller"], "Questionnaire");

        }

        [Test]
        public void Import_When_RequestDoesntContainsQuestionnaire_Then_ImportCommandWasntExecutedAndRedirectToErrorController()
        {
            // arrange
            AdminController controller = this.CreateAdminController();
            Mock<HttpPostedFileBase> file = new Mock<HttpPostedFileBase>();

            // act
            controller.Import(file.Object);

            // assert
            this.CommandServiceMock.Verify(x => x.Execute(It.IsAny<ImportQuestionnaireCommand>(), It.IsAny<string>()), Times.Never());
            Assert.IsTrue(controller.TempData.ContainsKey(Alerts.ERROR));
        }

        [Test]
        public void Export_When_TemplateIsNotNull_Then_FileIsReturned()
        {
            // arrange
            AdminController controller = this.CreateAdminController();
            Guid templateId = Guid.NewGuid();
            TemplateInfo dataForZip = new TemplateInfo() { Source = "zipped data", Title = "template" };

            var doc = new QuestionnaireDocument();
            var view = new QuestionnaireView(doc);
            this.questionnaireViewFactoryMock.Setup(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>())).Returns(view);

            this.ExportServiceMock.Setup(x => x.GetQuestionnaireTemplateInfo(Moq.It.IsAny<QuestionnaireDocument>())).Returns(dataForZip);
            this.ZipUtilsMock.Setup(x => x.Compress(dataForZip.Source)).Returns(new MemoryStream());
            // act
            controller.Export(templateId);

            // assert
            this.ExportServiceMock.Verify(x => x.GetQuestionnaireTemplateInfo(Moq.It.IsAny<QuestionnaireDocument>()), Times.Once());
            this.ZipUtilsMock.Verify(x => x.Compress(dataForZip.Source), Times.Once());
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Export_When_TemplateIsAbsent_Then_NullisReturned(string data)
        {
            // arrange
            
            Guid templateId = Guid.NewGuid();
            var template = new TemplateInfo() { Source = data };

            var doc = new QuestionnaireDocument();
            var view = new QuestionnaireView(doc);
            this.questionnaireViewFactoryMock.Setup(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>())).Returns(view);

            this.ExportServiceMock.Setup(x => x.GetQuestionnaireTemplateInfo(Moq.It.IsAny<QuestionnaireDocument>())).Returns(template);

            AdminController target = this.CreateAdminController();

            // act
            var result = target.Export(templateId);

            // assert
            Assert.That(result, Is.EqualTo(null));
        }

        private AdminController CreateAdminController(IMembershipUserService userHelper = null,
            IQuestionnaireHelper questionnaireHelper = null,
            ILogger logger = null,
            IStringCompressor zipUtils = null,
            ICommandService commandService = null,
            IQuestionnaireExportService exportService = null,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null)
        {
            return new AdminController(userHelper ?? UserHelperMock.Object,
            questionnaireHelper ?? Mock.Of<IQuestionnaireHelper>(),
            logger ?? Mock.Of<ILogger>(),
            zipUtils ?? ZipUtilsMock.Object,
            commandService ?? CommandServiceMock.Object,
            exportService ?? ExportServiceMock.Object,
            questionnaireViewFactory ?? questionnaireViewFactoryMock.Object);
        }

        protected Mock<ICommandService> CommandServiceMock;
        protected Mock<IStringCompressor> ZipUtilsMock;
        protected Mock<IQuestionnaireExportService> ExportServiceMock;
        protected Mock<IMembershipUserService> UserHelperMock;
        protected Mock<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>> questionnaireViewFactoryMock;
    }
}
