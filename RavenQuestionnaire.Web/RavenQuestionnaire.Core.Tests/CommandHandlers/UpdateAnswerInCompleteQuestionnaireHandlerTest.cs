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
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.ExpressionExecutors;
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
            CompleteQuestion question = new CompleteQuestion("q",
                                             QuestionType.SingleOption);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), Guid.NewGuid());
            question.Answers.Add(answer);
            qDoqument.Questions.Add(question);
            CompleteQuestionnaire questionanire = new CompleteQuestionnaire(qDoqument);
            repositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqId")).Returns(questionanire);
            UpdateAnswerInCompleteQuestionnaireHandler handler = new UpdateAnswerInCompleteQuestionnaireHandler(repositoryMock.Object, new CompleteQuestionnaireConditionExecutor());
            UpdateAnswerInCompleteQuestionnaireCommand command = new UpdateAnswerInCompleteQuestionnaireCommand("cqId",
                                                                                                                new CompleteAnswer
                                                                                                                    []
                                                                                                                    {
                                                                                                                        new CompleteAnswer
                                                                                                                            (answer,
                                                                                                                             question
                                                                                                                                 .
                                                                                                                                 PublicKey)
                                                                                                                    },
                                                                                                                null);
            handler.Handle(command);
            repositoryMock.Verify(x => x.Load("completequestionnairedocuments/cqId"), Times.Once());
            Assert.AreEqual(qDoqument.Questions[0].PublicKey, question.PublicKey);
            Assert.AreEqual(qDoqument.Questions[0].Answers[0].Selected, true);
        }
    }
}
