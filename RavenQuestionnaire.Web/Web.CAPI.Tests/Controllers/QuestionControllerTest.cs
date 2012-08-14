using System;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using RavenQuestionnaire.Core;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.Answer;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;
using RavenQuestionnaire.Web.Controllers;

namespace RavenQuestionnaire.Web.Tests.Controllers
{
    [TestFixture]
    public class QuestionControllerTest
    {
        public Mock<ICommandService> CommandServiceMock { get; set; }
        public Mock<IViewRepository> ViewRepositoryMock { get; set; }
        public QuestionController Controller { get; set; }

        [SetUp]
        public void CreateObjects()
        {
            CommandServiceMock = new Mock<ICommandService>();
            ViewRepositoryMock = new Mock<IViewRepository>();
            NcqrsEnvironment.SetDefault<ICommandService>(CommandServiceMock.Object);
            Controller = new QuestionController(ViewRepositoryMock.Object);
        }
        [Test]
        public void WhenNewQuestioneIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = Guid.NewGuid().ToString();
          

            ViewRepositoryMock.Setup(
                x =>
                x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                    It.Is<QuestionnaireViewInputModel>(
                        v => v.QuestionnaireId.Equals("questionnairedocuments/" + innerDocument.Id))))
                .Returns(new QuestionnaireView(innerDocument));
            Controller.Save(new QuestionView[]
                                {new QuestionView() {Title = "test", QuestionnaireId = innerDocument.Id}}, new AnswerView[0]);
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<AddQuestionCommand>()), Times.Once());
        }

        [Test]
        public void WhenExistingQuestionIsSubmittedWIthValidModel_CommandIsSent()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = Guid.NewGuid().ToString();
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion(Guid.NewGuid(), "question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, false, false, Order.AsIs, null, null, Guid.NewGuid());

            var questionView = new QuestionView(innerDocument, question);
            ViewRepositoryMock.Setup(
              x =>
              x.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                  It.Is<QuestionnaireViewInputModel>(
                      v => v.QuestionnaireId.Equals("questionnairedocuments/" + innerDocument.Id))))
              .Returns(new QuestionnaireView(innerDocument));


            Controller.Save(new QuestionView[] {questionView}, questionView.Answers);
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<ChangeQuestionCommand>()), Times.Once());
        }
        /*[Test]
        public void When_DeleteQuestionIsExecuted()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = Guid.NewGuid().ToString();
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion(Guid.NewGuid(), "question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, false, false, Order.AsIs, null, null, Guid.NewGuid());

            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/" + innerDocument.Id)).Returns(entity);

            Controller.Delete(question.PublicKey, entity.QuestionnaireId);
            CommandServiceMock.Verify(x => x.Execute(It.IsAny<DeleteQuestionCommand>()), Times.Once());
        }*/

        [Test]
        public void When_EditQuestionDetailsIsReturned()
        {
           // var output = new QuestionnaireView("questionnairedocuments/qId", "test", DateTime.Now, DateTime.Now, new QuestionView[0]);

            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Core.Entities.Questionnaire entity = new Core.Entities.Questionnaire(innerDocument);
            var question = entity.AddQuestion(Guid.NewGuid(), "question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, false, false, Order.AsIs, null, null, Guid.NewGuid());
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
