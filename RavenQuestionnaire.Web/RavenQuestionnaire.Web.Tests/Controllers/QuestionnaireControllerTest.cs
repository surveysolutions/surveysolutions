using System;
using System.Web;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Controllers;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    
    
    /// <summary>
    ///This is a test class for QuestionnaireControllerTest and is intended
    ///to contain all QuestionnaireControllerTest Unit Tests
    ///</summary>
    [TestFixture]
    public class QuestionnaireControllerTest
    {
        public Mock<ICommandService> CommandServiceMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public QuestionnaireController Controller { get; set; }
        [SetUp]
        public void CreateObjects()
        {
            ViewRepositoryMock = new Mock<IViewRepository>();

            CommandServiceMock = new Mock<ICommandService>();
            NcqrsEnvironment.SetDefault<ICommandService>(CommandServiceMock.Object);
            Controller = new QuestionnaireController(ViewRepositoryMock.Object);
        }


        [Test]
        public void WhenNewQuestionnaireIsSubmittedWithValidModel_CommandIsSent()
        {
            Controller.Save(new QuestionnaireView() {Title = "test"});
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<CreateQuestionnaireCommand>()), Times.Once());
        }
        /*[Test]
        public void WhenExistingQuestionnaireIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);

            Controller.Save(new QuestionnaireView(innerDocument));
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<UpdateQuestionnaireCommand>()), Times.Once());
        }*/
        [Test]
        public void When_GetQuestionnaireIsExecutedModelIsReturned()
        {
            var input = new QuestionnaireBrowseInputModel();
            var output = new QuestionnaireBrowseView(0, 10, 0, new QuestionnaireBrowseItem[0],"");
            ViewRepositoryMock.Setup(x => x.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input))
                .Returns(output);

            var result = Controller.ItemList(input);
            Assert.AreEqual(output, result.ViewData.Model);
        }

        [Test]
        public void When_GetQuestionnaireDetailsIsExecuted()
        {
            QuestionnaireDocument innerDoc = new QuestionnaireDocument();
            innerDoc.PublicKey = Guid.NewGuid();
            innerDoc.Title = "test";
            innerDoc.CreationDate = DateTime.Now;
            innerDoc.LastEntryDate = DateTime.Now;
            var output = new QuestionnaireView(innerDoc);
            var input = new QuestionnaireViewInputModel(innerDoc.PublicKey);

            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(v => v.QuestionnaireId.Equals(input.QuestionnaireId))))
                .Returns(output);

            var result = Controller.Details(output.PublicKey);
            Assert.AreEqual(output, result.ViewData.Model);
        }
        [Test]
        public void Details_IdIsEmpty_ExceptionThrowed()
        {
            Assert.Throws<HttpException>(() => Controller.Details(Guid.Empty));
        }
        /*[Test]
        public void When_DeleteQuestionnaireIsExecuted()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            Controller.Delete(entity.QuestionnaireId);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<DeleteQuestionnaireCommand>()), Times.Once());
        }*/
        [Test]
        public void Edit_EditFormIsReturned()
        {
            QuestionnaireDocument innerDoc = new QuestionnaireDocument();
            innerDoc.PublicKey = Guid.NewGuid();
            innerDoc.Title = "test";
            innerDoc.CreationDate = DateTime.Now;
            innerDoc.LastEntryDate = DateTime.Now;
            var output = new QuestionnaireView(innerDoc);
            var input = new QuestionnaireViewInputModel(innerDoc.PublicKey);

            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(v => v.QuestionnaireId.Equals(input.QuestionnaireId))))
                .Returns(output);

            var result = Controller.Edit(output.PublicKey);
            Assert.AreEqual(output, result.ViewData.Model);
        }
    }
}
