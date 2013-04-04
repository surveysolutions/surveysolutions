using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Main.Core.Documents;
using Main.Core.View;
using Moq;
using NUnit.Framework;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.Questionnaire.ImportService.Commands;
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

        [SetUp]
        public void Setup()
        {
            CommandServiceMock=new Mock<ICommandService>();
            ViewRepositoryMock=new Mock<IViewRepository>();
            ZipUtilsMock=new Mock<IZipUtils>();
        }

        [Test]
        public void Import_When_RequestContainsQuestionnirie_Then_ImportCommandExecutedAndRedirectToQuestionnairieController()
        {
            // arrange
            SynchronizationController controller = CreateSynchronizationController();

            Mock<HttpPostedFileBase> file = new Mock<HttpPostedFileBase>();
            ZipUtilsMock.Setup(x => x.UnzipTemplate<IQuestionnaireDocument>(controller.Request, file.Object))
                        .Returns(new QuestionnaireDocument());

            // act
            controller.Import(file.Object);

            // assert
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<ImportQuestionnaireCommand>()), Times.Once());

        }

        private SynchronizationController CreateSynchronizationController()
        {
            return new SynchronizationController(ViewRepositoryMock.Object, CommandServiceMock.Object,
                                                 ZipUtilsMock.Object);
        }
    }
}
