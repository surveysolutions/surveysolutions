using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.BootstrapSupport;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.Applications
{
    [TestFixture]
    internal class AdminControllerTests
    {
        [SetUp]
        public void Setup()
        {
            this.CommandServiceMock=new Mock<ICommandService>();
            this.ZipUtilsMock = new Mock<IStringCompressor>();
            this.UserHelperMock = new Mock<IMembershipUserService>();
            this.questionnaireViewFactoryMock =  new Mock<IQuestionnaireViewFactory>();
            
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

            this.ZipUtilsMock.Setup(
                x => x.DecompressGZip<QuestionnaireDocumentWithLookUpTables>(It.IsAny<Stream>()))
                .Returns(new QuestionnaireDocumentWithLookUpTables()
                {
                    LookupTables = new Dictionary<Guid, string>(),
                    QuestionnaireDocument = new QuestionnaireDocument()
                });
            this.UserHelperMock.Setup(x => x.WebUser.UserId).Returns(Guid.NewGuid);

            // act
            var actionResult = (RedirectToRouteResult)controller.Import(file.Object);

            // assert
            this.CommandServiceMock.Verify(x => x.Execute(It.IsAny<ImportQuestionnaire>(), It.IsAny<string>()), Times.Once());

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
            this.CommandServiceMock.Verify(x => x.Execute(It.IsAny<ImportQuestionnaire>(), It.IsAny<string>()), Times.Never());
            Assert.IsTrue(controller.TempData.ContainsKey(Alerts.ERROR));
        }

        [Test]
        public void Export_When_TemplateIsNotNull_Then_FileIsReturned()
        {
            // arrange
            AdminController controller = this.CreateAdminController();
            Guid templateId = Guid.NewGuid();

            var doc = new QuestionnaireDocument() {Title = "test"};
            var view = Create.QuestionnaireView(doc);
            this.questionnaireViewFactoryMock.Setup(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>())).Returns(view);

            this.ZipUtilsMock.Setup(x => x.Compress(Moq.It.IsAny<string>())).Returns(new MemoryStream());
            // act
            controller.Export(templateId);

            // assert
            this.ZipUtilsMock.Verify(x => x.Compress(Moq.It.IsAny<string>()), Times.Once());
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        public void Export_When_TemplateIsAbsent_Then_NullisReturned(string data)
        {
            // arrange
            
            Guid templateId = Guid.NewGuid();

            this.questionnaireViewFactoryMock.Setup(x => x.Load(Moq.It.IsAny<QuestionnaireViewInputModel>())).Returns<QuestionnaireView>(null);

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
            IQuestionnaireViewFactory questionnaireViewFactory = null)
        {
            return new AdminController(userHelper ?? this.UserHelperMock.Object,
                questionnaireHelper ?? Mock.Of<IQuestionnaireHelper>(),
                logger ?? Mock.Of<ILogger>(),
                zipUtils ?? this.ZipUtilsMock.Object,
                commandService ?? this.CommandServiceMock.Object,
                questionnaireViewFactory ?? this.questionnaireViewFactoryMock.Object,
                Mock.Of<ISerializer>(_ => _.Serialize(Moq.It.IsAny<QuestionnaireDocument>())==""),
                Mock.Of<IAccountListViewFactory>(),
                Mock.Of<ILookupTableService>(),
                Mock.Of<IAttachmentService>(),
                Mock.Of<ITranslationsService>());
        }

        protected Mock<ICommandService> CommandServiceMock;
        protected Mock<IStringCompressor> ZipUtilsMock;
        protected Mock<IMembershipUserService> UserHelperMock;
        protected Mock<IQuestionnaireViewFactory> questionnaireViewFactoryMock;
    }
}
