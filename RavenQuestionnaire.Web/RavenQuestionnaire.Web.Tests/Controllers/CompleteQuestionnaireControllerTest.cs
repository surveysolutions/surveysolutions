using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using Questionnaire.Core.Web.Membership;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using RavenQuestionnaire.Core.Views.Questionnaire;
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
        public void WhenNewCompleteQuestionnaireIsSubmittedWithValidModel_CommandIsSent()
        {
            Authentication.Setup(x => x.GetUserIdForCurrentUser()).Returns("some_user_id");
            Controller.SaveResult("some id", new CompleteAnswer[0]);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<CreateNewCompleteQuestionnaireCommand>()), Times.Once());
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
    }
}
