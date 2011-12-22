using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Commands;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateAnswerInCompleteQuestionnaireHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_NewAnswerIsAddedTCompleteQuestionnaire()
        {
            Mock<ICompleteQuestionnaireRepository> repositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            CompleteQuestionnaireDocument qDoqument= new CompleteQuestionnaireDocument();
            qDoqument.Questionnaire= new QuestionnaireDocument();
            Question question = new Question( "q",
                                             QuestionType.SingleOption);
            Answer answer= new Answer();
            question.Answers.Add(answer);
            qDoqument.Questionnaire.Questions.Add(question);
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            repositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqId")).Returns(questionanire);
            UpdateAnswerInCompleteQuestionnaireHandler handler = new UpdateAnswerInCompleteQuestionnaireHandler(repositoryMock.Object);
            UpdateAnswerInCompleteQuestionnaireCommand command = new UpdateAnswerInCompleteQuestionnaireCommand("cqId",
                                                                                                                new CompleteAnswer
                                                                                                                    (answer,
                                                                                                                     question.PublicKey));
            handler.Handle(command);
            repositoryMock.Verify(x => x.Load("completequestionnairedocuments/cqId"), Times.Once());
            Assert.AreEqual(qDoqument.CompletedAnswers[0].QuestionPublicKey, question.PublicKey);
            Assert.AreEqual(qDoqument.CompletedAnswers[0].PublicKey, answer.PublicKey);
        }
    }
}
