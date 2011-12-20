using System;
using System.Collections.Generic;
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
            var question = entity.AddQuestion("question", QuestionType.SingleOption, string.Empty);
            var questionView = new QuestionView(question.PublicKey, question.QuestionText, question.QuestionType,
                                                question.Answers, question.QuestionnaireId, question.ConditionExpression);


            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(
                        v => v.QuestionnaireId.Equals("questionnairedocuments/qID"))))
                .Returns(new QuestionnaireView(innerDocument.Id, innerDocument.Title, innerDocument.CreationDate,
                                               innerDocument.LastEntryDate, new QuestionView[0]));
            Controller.Save(new QuestionView() {QuestionText = "test", QuestionnaireId = question.QuestionnaireId});
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<AddNewQuestionCommand>()), Times.Once());
        }

        [Test]
        public void WhenExistingQuestionIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion("question", QuestionType.SingleOption, string.Empty);

            var questionView = new QuestionView(question.PublicKey, question.QuestionText, question.QuestionType,
                                                question.Answers, question.QuestionnaireId, question.ConditionExpression);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);
            ViewRepositoryMock.Setup(
              x =>
              x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                  It.Is<QuestionnaireViewInputModel>(
                      v => v.QuestionnaireId.Equals("questionnairedocuments/qID"))))
              .Returns(new QuestionnaireView(innerDocument.Id, innerDocument.Title, innerDocument.CreationDate,
                                             innerDocument.LastEntryDate, new QuestionView[0]));
          

            Controller.Save(questionView);
            CommandInvokerMock.Verify(x => x.Execute(It.IsAny<UpdateQuestionCommand>()), Times.Once());
        }
        [Test]
        public void When_DeleteQuestionIsExecuted()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion("question", QuestionType.SingleOption, string.Empty);

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
            var question = entity.AddQuestion("question", QuestionType.SingleOption, string.Empty);
            var questionView = new QuestionView(question.PublicKey, question.QuestionText, question.QuestionType,
                                                question.Answers, question.QuestionnaireId, question.ConditionExpression);

            var input = new QuestionViewInputModel(question.PublicKey, entity.QuestionnaireId);

            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionViewInputModel, QuestionView>(
                    It.Is<QuestionViewInputModel>(
                        v => v.QuestionnaireId.Equals(input.QuestionnaireId) && v.PublickKey.Equals(input.PublickKey))))
                .Returns(questionView);

            var result = Controller.Edit(question.PublicKey, question.QuestionnaireId);
            Assert.AreEqual(questionView, ((PartialViewResult)result).ViewData.Model);
        }
     
    }
}
