using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateCompleteQuestionnaireHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_CompleteQuestionnaireIsUpdatedToRepository()
        {
            CompleteQuestionnaireDocument innerDocument = new CompleteQuestionnaireDocument();
            innerDocument.Id = "cqID";
            innerDocument.Questionnaire = new QuestionnaireDocument() { Id = "qID" };
            Question question = new Question("?", QuestionType.SingleOption);

            innerDocument.Questionnaire.Questions.Add(question);
            Answer answer= new Answer();
            question.Answers.Add(answer);

            CompleteQuestionnaire entity = new CompleteQuestionnaire(innerDocument);
            Questionnaire qEntity= new Questionnaire(innerDocument.Questionnaire);
            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();

            Mock<IStatusRepository> statusRepositoryMock = new Mock<IStatusRepository>();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();

            coompleteQuestionnaireRepositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqID")).Returns(entity);

            UpdateCompleteQuestionnaireHandler handler = new UpdateCompleteQuestionnaireHandler(coompleteQuestionnaireRepositoryMock.Object,
                statusRepositoryMock.Object, userRepositoryMock.Object);

            handler.Handle(new Commands.UpdateCompleteQuestionnaireCommand("cqID", new CompleteAnswer[] { new CompleteAnswer(answer, question.PublicKey) }, "-11", "-111", null));

            coompleteQuestionnaireRepositoryMock.Verify(x => x.Load("completequestionnairedocuments/cqID"));
            Assert.AreEqual(innerDocument.CompletedAnswers.Count, 1);
            Assert.AreEqual(innerDocument.CompletedAnswers[0].QuestionPublicKey, question.PublicKey);
            Assert.AreEqual(innerDocument.CompletedAnswers[0].PublicKey, answer.PublicKey);

        }
    }
}
