using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Controllers;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class QuestionControllerTest
    {
        public Mock<ICommandInvoker> CommandInvokerMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public QuestionController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandInvokerMock = new Mock<ICommandInvoker>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            Controller = new QuestionController(CommandInvokerMock.Object, ViewRepositoryMock.Object);
        }
        [Test]
        public void WhenNewQuestioneIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion("question", QuestionType.SingleOption, string.Empty, null);
            var questionView = new QuestionView(innerDocument,question);


            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(
                        v => v.QuestionnaireId.Equals("questionnairedocuments/qID"))))
                .Returns(new QuestionnaireView(innerDocument));
            Controller.Save(new QuestionView() { QuestionText = "test", QuestionnaireId = innerDocument.Id }, false);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<AddNewQuestionCommand>()), Times.Once());
        }

        [Test]
        public void WhenExistingQuestionIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion("question", QuestionType.SingleOption, string.Empty, null);

            var questionView = new QuestionView(innerDocument ,question);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);
            ViewRepositoryMock.Setup(
              x =>
              x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                  It.Is<QuestionnaireViewInputModel>(
                      v => v.QuestionnaireId.Equals("questionnairedocuments/qID"))))
              .Returns(new QuestionnaireView(innerDocument));
          

            Controller.Save(questionView, false);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<UpdateQuestionCommand>()), Times.Once());
        }
        [Test]
        public void When_DeleteQuestionIsExecuted()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion("question", QuestionType.SingleOption, string.Empty, null);

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            Controller.Delete(question.PublicKey, entity.QuestionnaireId);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<DeleteQuestionCommand>()), Times.Once());
        }

        [Test]
        public void When_EditQuestionDetailsIsReturned()
        {
           // var output = new QuestionnaireView("questionnairedocuments/qId", "test", DateTime.Now, DateTime.Now, new QuestionView[0]);

            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion("question", QuestionType.SingleOption, string.Empty, null);
            var questionView = new QuestionView(innerDocument, question);

            var input = new QuestionViewInputModel(question.PublicKey, entity.QuestionnaireId);

            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionViewInputModel, QuestionView>(
                    It.Is<QuestionViewInputModel>(
                        v => v.QuestionnaireId.Equals(input.QuestionnaireId) && v.PublicKey.Equals(input.PublicKey))))
                .Returns(questionView);

            var result = Controller.Edit(question.PublicKey, innerDocument.Id);
            Assert.AreEqual(questionView, ((PartialViewResult)result).ViewData.Model);
        }
     
    }
}
