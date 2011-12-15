using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Views.Question;
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
        public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public QuestionnaireController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandInvokerMock = new Mock<ICommandInvoker>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            Controller = new QuestionnaireController(CommandInvokerMock.Object, ViewRepositoryMock.Object);
        }


        [Test]
        public void WhenNewQuestionnaireIsSubmittedWIthValidModel_CommandIsSent()
        {
            Controller.Save(new QuestionnaireView() {Title = "test"});
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<CreateNewQuestionnaireCommand>()), Times.Once());
        }
        [Test]
        public void WhenExistingQuestionnaireIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            Controller.Save(new QuestionnaireView(entity.QuestionnaireId, "edit q", innerDocument.CreationDate,
                                                  DateTime.Now, new QuestionView[0]));
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<UpdateQuestionnaireCommand>()), Times.Once());
        }
        [Test]
        public void When_GetQuestionnaireIsExecutedModelIsReturned()
        {
            var input = new QuestionnaireBrowseInputModel();
            var output = new QuestionnaireBrowseView(0, 10, 0, new QuestionnaireBrowseItem[0]);
            ViewRepositoryMock.Setup(x => x.Load<QuestionnaireBrowseInputModel, QuestionnaireBrowseView>(input))
                .Returns(output);

            var result = Controller.Index(input);
            Assert.AreEqual(output, result.ViewData.Model);
        }

        [Test]
        public void When_GetQuestionnaireDetailsIsExecuted()
        {
            var output = new QuestionnaireView("questionnairedocuments/qId", "test", DateTime.Now, DateTime.Now, new QuestionView[0]);
            var input = new QuestionnaireViewInputModel("qId");

            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(v => v.QuestionnaireId.Equals(input.QuestionnaireId))))
                .Returns(output);

            var result = Controller.Details(output.Id);
            Assert.AreEqual(output, result.ViewData.Model);
        }
        [Test]
        public void When_DeleteQuestionnaireIsExecuted()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            Controller.Delete(entity.QuestionnaireId);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<DeleteQuestionnaireCommand>()), Times.Once());
        }
    }
}
