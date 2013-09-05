﻿using System;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Controllers;

namespace WB.UI.Designer.Tests
{
    using System.IO;
    using System.IO.Compression;

    using WB.Core.SharedKernel.Utils.Compression;
    using WB.UI.Designer.BootstrapSupport;
    using WB.UI.Shared.Web.Membership;

    using TemplateInfo = WB.Core.BoundedContexts.Designer.Services.TemplateInfo;

    [TestFixture]
    public class SynchronizationControllerTests
    {
        protected Mock<ICommandService> CommandServiceMock;
        protected Mock<IStringCompressor> ZipUtilsMock;
        protected Mock<IJsonExportService> ExportServiceMock;
        protected Mock<IMembershipUserService> UserHelperMock;
        
        [SetUp]
        public void Setup()
        {
            CommandServiceMock=new Mock<ICommandService>();
            ZipUtilsMock = new Mock<IStringCompressor>();
            ExportServiceMock = new Mock<IJsonExportService>();
            UserHelperMock=new Mock<IMembershipUserService>();
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
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

            ZipUtilsMock.Setup(x => x.Decompress<IQuestionnaireDocument>(file.Object.InputStream))
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
            TemplateInfo dataForZip = new TemplateInfo() { Source = "zipped data", Title = "template" };
            ExportServiceMock.Setup(x => x.GetQuestionnaireTemplate(templateId)).Returns(dataForZip);
            ZipUtilsMock.Setup(x => x.Compress(dataForZip.Source)).Returns(new MemoryStream());
            // act
            controller.Export(templateId);

            // assert
            ExportServiceMock.Verify(x => x.GetQuestionnaireTemplate(templateId), Times.Once());
            ZipUtilsMock.Verify(x => x.Compress(dataForZip.Source), Times.Once());
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Export_When_TemplateIsAbsent_Then_NullisReturned(string data)
        {
            // arrange
            SynchronizationController target = CreateSynchronizationController();
            Guid templateId = Guid.NewGuid();
            ExportServiceMock.Setup(x => x.GetQuestionnaireTemplate(templateId))
                             .Returns(new TemplateInfo() { Source = data });

            // act
            var result = target.Export(templateId);

            // assert
            Assert.That(result, Is.EqualTo(null));
        }

        private SynchronizationController CreateSynchronizationController()
        {
            return new SynchronizationController(CommandServiceMock.Object,
                                                 UserHelperMock.Object,
                                                 ZipUtilsMock.Object, ExportServiceMock.Object);
        }
    }
}
