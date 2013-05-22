using System;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.Questionnaire.ExportServices;
using WB.Core.Questionnaire.ImportService.Commands;
using WB.UI.Designer.Controllers;

namespace WB.UI.Designer.Tests
{
    using System.IO;
    using System.IO.Compression;

    using WB.UI.Designer.BootstrapSupport;
    using WB.UI.Shared.Compression;
    using WB.UI.Shared.Web.Membership;

    [TestFixture]
    public class SynchronizationControllerTests
    {
        protected Mock<ICommandService> CommandServiceMock;
        protected Mock<IViewRepository> ViewRepositoryMock;
        protected Mock<IZipUtils> ZipUtilsMock;
        protected Mock<IExportService> ExportServiceMock;
        protected Mock<IMembershipUserService> UserHelperMock;
        
        [SetUp]
        public void Setup()
        {
            CommandServiceMock=new Mock<ICommandService>();
            ViewRepositoryMock=new Mock<IViewRepository>();
            ZipUtilsMock=new Mock<IZipUtils>();
            ExportServiceMock = new Mock<IExportService>();
            UserHelperMock=new Mock<IMembershipUserService>();
        }

        [Test]
        public void Import_When_RequestContainsQuestionnirie_Then_ImportCommandExecutedAndRedirectToQuestionnairieController()
        {
            // arrange
            SynchronizationController controller = CreateSynchronizationController();

            Mock<HttpPostedFileBase> file = new Mock<HttpPostedFileBase>();

            var inputStream = new MemoryStream();
            using (var zip =new GZipStream(inputStream, CompressionMode.Compress, true))
            {
                zip.Write(new byte[] { 1 }, 0, 1);
            }

            file.Setup(x => x.ContentLength).Returns((int)inputStream.Length);
            file.Setup(x => x.InputStream).Returns(inputStream);

            ZipUtilsMock.Setup(x => x.UnZip<IQuestionnaireDocument>(file.Object.InputStream))
                        .Returns(new QuestionnaireDocument());
            UserHelperMock.Setup(x => x.WebUser.UserId).Returns(Guid.NewGuid);

            // act
            var actionResult = (RedirectToRouteResult)controller.Import(file.Object);

            // assert
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<ImportQuestionnaireCommand>()), Times.Once());

            Assert.AreEqual(actionResult.RouteValues["action"],"Index");
            Assert.AreEqual(actionResult.RouteValues["controller"], "Questionnaire");

        }

        [Test]
        public void Import_When_RequestDoesntContainsQuestionnaire_Then_ImportCommandWasntExecutedAndRedirectToErrorController()
        {
            // arrange
            SynchronizationController controller = CreateSynchronizationController();
            Mock<HttpPostedFileBase> file = new Mock<HttpPostedFileBase>();

            // act
            controller.Import(file.Object);

            // assert
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<ImportQuestionnaireCommand>()), Times.Never());
            Assert.IsTrue(controller.TempData.ContainsKey(Alerts.ERROR));
        }

        [Test]
        public void Export_When_TemplateIsNotNull_Then_FileIsReturned()
        {
            // arrange
            SynchronizationController controller = CreateSynchronizationController();
            Guid templateId = Guid.NewGuid();
            string dataForZip = "zipped data";
            ExportServiceMock.Setup(x => x.GetQuestionnaireTemplate(templateId)).Returns(dataForZip);
            ZipUtilsMock.Setup(x => x.Zip(dataForZip)).Returns(new MemoryStream());
            // act
            controller.Export(templateId);

            // assert
            ExportServiceMock.Verify(x => x.GetQuestionnaireTemplate(templateId), Times.Once());
            ZipUtilsMock.Verify(x => x.Zip(dataForZip), Times.Once());
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Export_When_TemplateIsAbsent_Then_NullisReturned(string data)
        {
            // arrange
            SynchronizationController target = CreateSynchronizationController();
            Guid templateId = Guid.NewGuid();
            ExportServiceMock.Setup(x => x.GetQuestionnaireTemplate(templateId)).Returns(data);

            // act
            var result = target.Export(templateId);

            // assert
            Assert.That(result, Is.EqualTo(null));
        }

        private SynchronizationController CreateSynchronizationController()
        {
            return new SynchronizationController(ViewRepositoryMock.Object, CommandServiceMock.Object,
                                                 UserHelperMock.Object,
                                                 ZipUtilsMock.Object, ExportServiceMock.Object);
        }
    }
}
