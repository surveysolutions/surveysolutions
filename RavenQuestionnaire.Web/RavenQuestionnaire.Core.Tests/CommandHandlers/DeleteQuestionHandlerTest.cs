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
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class DeleteQuestionHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionnIsDeletedFromRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Id = "qID";

            Questionnaire entity = new Questionnaire(innerDocument);
            var question = new Question(entity, "question", QuestionType.MultyOption);
            innerDocument.Questions.Add(question);
            Assert.True(
                innerDocument.Questions.Count == 1);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(entity);

            DeleteQuestionHandler handler = new DeleteQuestionHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new Commands.DeleteQuestionCommand(question.PublicKey, entity.QuestionnaireId));

            Assert.True(
                innerDocument.Questions.Count == 0);

        }
    }
}
