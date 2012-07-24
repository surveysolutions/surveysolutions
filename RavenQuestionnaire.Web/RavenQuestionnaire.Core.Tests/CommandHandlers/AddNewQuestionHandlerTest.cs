using System;
using System.Linq;
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
        public void WhenCommandIsReceived_NewQuestionIsAddedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);

            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, string.Empty)).Returns(true);
            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object, validator.Object);
            AnswerView[] answers = new AnswerView[] { new AnswerView() { Title = "answer", AnswerType = AnswerType.Select } };
            handler.Handle(new AddNewQuestionCommand(Guid.NewGuid(),"test", "testExport", QuestionType.SingleOption, entity.QuestionnaireId, null, Guid.NewGuid(), string.Empty, string.Empty,
                                                        null, false, false, Order.AsIs, answers.Select(a => ConvertAnswer(a)).ToArray(), null));

            questionnaireRepositoryMock.Verify(x => x.Load(key.ToString()), Times.Once());

        }
        [Test]
        public void WhenCommandIsReceived_ToGroup_NewQuestionIsAddedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            
            innerDocument.PublicKey = key;
            Group topGroup = new Group("top group");
            innerDocument.Children.Add(topGroup);
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);
            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, string.Empty)).Returns(true);

            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object, validator.Object);
            AnswerView[] answers = new AnswerView[] { new AnswerView() { Title = "answer", AnswerType = AnswerType.Select } };
            handler.Handle(new AddNewQuestionCommand(Guid.NewGuid(), "test", "testExport", QuestionType.SingleOption, entity.QuestionnaireId,
                topGroup.PublicKey, Guid.NewGuid(), string.Empty, string.Empty,
                null, false, false, Order.AsIs, answers.Select(ConvertAnswer).ToArray(), null));

            questionnaireRepositoryMock.Verify(x => x.Load(key.ToString()), Times.Once());
            Assert.AreEqual((innerDocument.Children[0] as Group).Children.Count, 1);
            Assert.AreEqual(((IQuestion)(innerDocument.Children[0] as Group).Children[0]).QuestionText, "test");
        }
        [Test]
        public void WhenCommandIsReceived_ToNotExistingGroup_NewQuestionIsAddedToRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);
            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, It.Is<string>(s=>string.IsNullOrEmpty(s)))).Returns(true);
            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object, validator.Object);
            AnswerView[] answers = new AnswerView[] { new AnswerView() { Title = "answer", AnswerType = AnswerType.Select } };
            Assert.Throws<ArgumentException>(
                () =>
                handler.Handle(new AddNewQuestionCommand(Guid.NewGuid(), "test", "testExport", QuestionType.SingleOption,
                                                                  entity.QuestionnaireId, Guid.NewGuid(), Guid.NewGuid(), string.Empty,
                                                                  string.Empty, string.Empty, false, false, Order.AsIs,
                                                                  answers.Select(ConvertAnswer).ToArray(), null)));
        }

        [Test]
        public void WhenCommandIsReceived_NotValidExpression_QuestionIsNotAdded()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;
            Questionnaire entity = new Questionnaire(innerDocument);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);
            Mock<IExpressionExecutor<Questionnaire, bool>> validator = new Mock<IExpressionExecutor<Questionnaire, bool>>();
            validator.Setup(x => x.Execute(entity, "invalid condition")).Returns(false); 
            
            AddNewQuestionHandler handler = new AddNewQuestionHandler(questionnaireRepositoryMock.Object, validator.Object);
            AnswerView[] answers = new AnswerView[] { new AnswerView() { Title = "answer", AnswerType = AnswerType.Select,  } };
            handler.Handle(new AddNewQuestionCommand(Guid.NewGuid(), "test", "testExport", QuestionType.SingleOption, entity.QuestionnaireId,
               null, Guid.NewGuid(), "invalid condition", string.Empty, string.Empty, false, false, Order.AsIs,
                answers.Select(ConvertAnswer).ToArray(), null));

            Assert.AreEqual(innerDocument.Children.Count, 0);
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


        private static Answer ConvertAnswer(AnswerView a)
        {
            var answer = new Answer();
            answer.AnswerValue = a.AnswerValue;
            answer.AnswerType = a.AnswerType;
            answer.AnswerText = a.Title;
            answer.Mandatory = a.Mandatory;
            answer.PublicKey = a.PublicKey;
            return answer;
        }

    }
}
