using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Main.Core.Documents;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.Questionnaire.ImportService.Commands;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers;
using WB.UI.Designer.Utils;

namespace WB.UI.Designer.Tests
{
    [TestFixture]
    public class SynchronizationControllerTests
    {
        protected Mock<ICommandService> CommandServiceMock;
        protected Mock<IViewRepository> ViewRepositoryMock;
        protected Mock<IZipUtils> ZipUtilsMock;
        protected Mock<IUserHelper> UserHelperMock;
        
        [SetUp]
        public void Setup()
        {
            CommandServiceMock=new Mock<ICommandService>();
            ViewRepositoryMock=new Mock<IViewRepository>();
            ZipUtilsMock=new Mock<IZipUtils>();
            UserHelperMock=new Mock<IUserHelper>();
        }

        [Test]
        public void Import_When_RequestContainsQuestionnirie_Then_ImportCommandExecutedAndRedirectToQuestionnairieController()
        {
            // arrange
            SynchronizationController controller = CreateSynchronizationController();

            Mock<HttpPostedFileBase> file = new Mock<HttpPostedFileBase>();
            ZipUtilsMock.Setup(x => x.UnzipTemplate<IQuestionnaireDocument>(controller.Request, file.Object))
                        .Returns(new QuestionnaireDocument());
            UserHelperMock.Setup(x => x.CurrentUserId).Returns(Guid.NewGuid);

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
            var actionResult = (RedirectToRouteResult)controller.Import(file.Object);

            // assert
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<ImportQuestionnaireCommand>()), Times.Never());
            Assert.AreEqual(actionResult.RouteValues["action"], "Index");
            Assert.AreEqual(actionResult.RouteValues["controller"], "Error");
        }

        private SynchronizationController CreateSynchronizationController()
        {
            return new SynchronizationController(ViewRepositoryMock.Object, CommandServiceMock.Object,
                                                 UserHelperMock.Object,
                                                 ZipUtilsMock.Object);
        }
    }
}
