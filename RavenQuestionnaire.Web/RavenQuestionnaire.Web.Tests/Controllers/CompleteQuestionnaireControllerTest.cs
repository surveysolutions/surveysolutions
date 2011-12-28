using System;
using System.Web;
using Moq;
using NUnit.Framework;
using Questionnaire.Core.Web.Security;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Core.Views.Status;
using RavenQuestionnaire.Web.Controllers;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class CompleteQuestionnaireControllerTest
    {
        public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public Mock<IFormsAuthentication> Authentication { get; set; }
        public CompleteQuestionnaireController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandInvokerMock = new Mock<ICommandInvoker>();  
            ViewRepositoryMock = new Mock<IViewRepository>();
            Authentication = new Mock<IFormsAuthentication>();
            Controller = new CompleteQuestionnaireController(CommandInvokerMock.Object, ViewRepositoryMock.Object,
                                                             Authentication.Object);
        }

        [Test]
        public void When_GetCompleteQuestionnaireIsExecutedModelIsReturned()
        {
            var input = new CompleteQuestionnaireBrowseInputModel();
            var output = new CompleteQuestionnaireBrowseView(0, 10, 0, new CompleteQuestionnaireBrowseItem[0]);
            ViewRepositoryMock.Setup(x => x.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(input))
                .Returns(output);

            var result = Controller.Index(input);
            Assert.AreEqual(output, result.ViewData.Model);
        }
        [Test]
        public void When_GetQuestionnaireResultIsExecuted()
        {
            CompleteQuestionnaireDocument innerDoc = new CompleteQuestionnaireDocument();
            innerDoc.Id = "completequestionnairedocuments/cqId";
            innerDoc.CreationDate = DateTime.Now;
            innerDoc.LastEntryDate = DateTime.Now;
            innerDoc.Questionnaire= new QuestionnaireDocument();
            innerDoc.Status = new SurveyStatus("-1", "dummyStatus");
            innerDoc.Responsible = new UserLight("-1", "dummyUser");
            var output = new CompleteQuestionnaireView(innerDoc);
            var input = new CompleteQuestionnaireViewInputModel("cqId");

            ViewRepositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireView>(
                    It.Is<CompleteQuestionnaireViewInputModel>(v => v.CompleteQuestionnaireId.Equals(input.CompleteQuestionnaireId))))
                .Returns(output);

            var result = Controller.Result(output.Id);
            Assert.AreEqual(output, result.ViewData.Model);
        }
        [Test]
        public void When_DeleteQuestionnaireIsExecuted()
        {
             Controller.Delete("some_id");
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<DeleteCompleteQuestionnaireCommand>()), Times.Once());
        }


        [Test]
        public void Participate_EmptyId_404Exception()
        {
            Assert.Throws<HttpException>(() => Controller.Participate(null));
        }
        [Test]
        public void Participate_ValidId_FormIsReturned()
        {
        
        }
        [Test]
        public void Question_ValidId_FormIsReturned()
        {
            QuestionnaireDocument innerDoc = new QuestionnaireDocument();
            innerDoc.Id = "questionnairedocuments/cqId";
            CompleteQuestionnaireViewEnumerable template = new CompleteQuestionnaireViewEnumerable(innerDoc);
            var input = new CompleteQuestionnaireViewInputModel("cqId", null, false);
            ViewRepositoryMock.Setup(
               x =>
               x.Load<CompleteQuestionnaireViewInputModel, CompleteQuestionnaireViewEnumerable>(
                   It.Is<CompleteQuestionnaireViewInputModel>(v => v.CompleteQuestionnaireId.Equals(input.CompleteQuestionnaireId))))
               .Returns(template);
            var result = Controller.Question("cqId", null);
            Assert.AreEqual(result.ViewData.Model.GetType(), typeof(CompleteQuestionnaireViewEnumerable));
            Assert.AreEqual(result.ViewData.Model, template);
        }


        [Test]
        public void SaveSingleResult_Valid_FormIsReturned()
        {
            ViewRepositoryMock.Setup(
                x =>
                x.Load<StatusViewInputModel, StatusView>(
                    It.IsAny<StatusViewInputModel>()))
                .Returns(new StatusView());
            Controller.SaveSingleResult("cId", null,
                                        new CompleteAnswer[] {new CompleteAnswer(new Answer(), Guid.NewGuid())});
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<UpdateAnswerInCompleteQuestionnaireCommand>()), Times.Once());
        }
    }
}
