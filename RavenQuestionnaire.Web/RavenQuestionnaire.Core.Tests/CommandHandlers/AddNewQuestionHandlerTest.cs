using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Question;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class AddNewQuestionHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewQuestionnIsAddedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, string.Empty)).Returns(true);
            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object, validator.Object);
            AnswerView[] answers = new AnswerView[]{ new AnswerView(){ AnswerText = "answer", AnswerType = AnswerType.Text} };
            handler.Handle(new AddNewQuestionCommand("test", "testExport", QuestionType.SingleOption, entity.QuestionnaireId, null, string.Empty, string.Empty,
                                                              null, Order.AsIs, answers, null));

            questionnaireRepositoryMock.Verify(x => x.Load("questionnairedocuments/qID"), Times.Once());

        }
        [Test]
        public void WhenCommandIsReceived_ToGroup_NewQuestionnIsAddedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Group topGroup = new Group("top group");
            innerDocument.Groups.Add(topGroup);
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);
            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, string.Empty)).Returns(true);
            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object, validator.Object);
            AnswerView[] answers = new AnswerView[] { new AnswerView() { AnswerText = "answer", AnswerType = AnswerType.Text } };
            handler.Handle(new AddNewQuestionCommand("test", "testExport", QuestionType.SingleOption, entity.QuestionnaireId,
                topGroup.PublicKey, string.Empty, string.Empty,
                null, Order.AsIs, answers, null));

            questionnaireRepositoryMock.Verify(x => x.Load("questionnairedocuments/qID"), Times.Once());
            Assert.AreEqual((innerDocument.Groups[0] as Group).Questions.Count,1);
            Assert.AreEqual((innerDocument.Groups[0] as Group).Questions[0].QuestionText, "test");
        }
        [Test]
        public void WhenCommandIsReceived_ToNotExistingGroup_NewQuestionnIsAddedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);
            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, string.Empty)).Returns(true);
            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object, validator.Object);
            AnswerView[] answers = new AnswerView[] { new AnswerView() { AnswerText = "answer", AnswerType = AnswerType.Text } };
            Assert.Throws<ArgumentException>(
                () =>
                handler.Handle(new AddNewQuestionCommand("test", "testExport", QuestionType.SingleOption,
                                                                  entity.QuestionnaireId, Guid.NewGuid(), string.Empty,
                                                                  string.Empty, string.Empty, Order.AsIs,
                                                                  answers, null)));
        }

        [Test]
        public void WhenCommandIsReceived_NotValidExpression_QuestionIsNotAdded()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);
            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, string.Empty)).Returns(false);
            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object, validator.Object);
            AnswerView[] answers = new AnswerView[] { new AnswerView() { AnswerText = "answer", AnswerType = AnswerType.Text } };
            handler.Handle(new AddNewQuestionCommand("test", "testExport", QuestionType.SingleOption, entity.QuestionnaireId,
               null, string.Empty, string.Empty, string.Empty, Order.AsIs,
                answers, null));

            Assert.AreEqual(innerDocument.Questions.Count,0);
        }
        /*
        [Test]
        public void AddQuestion_ConditionIsInvalid_EvaluationExceptionIsThrowed()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Assert.Throws<EvaluationException>(
                () => questionnaire.AddQuestion("question", QuestionType.SingleOption, "totaly invalid condition", null));
        }*/
    }
}
