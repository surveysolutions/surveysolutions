using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Commands.Questionnaire.Question;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities.Question;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class DeleteQuestionHandlerTest
    {
        [Test]
        public void WhenCommandIsReceived_QuestionnIsDeletedFromRepository()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Guid key = Guid.NewGuid();
            innerDocument.PublicKey = key;

            Questionnaire entity = new Questionnaire(innerDocument);
            var question = new SingleQuestion(Guid.NewGuid(),"question");
            innerDocument.Children.Add(question);
            Assert.True(
                innerDocument.Children.Count == 1);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(entity);

            DeleteQuestionHandler handler = new DeleteQuestionHandler(questionnaireRepositoryMock.Object);
            handler.Handle(new DeleteQuestionCommand(question.PublicKey, entity.QuestionnaireId, null));

            Assert.True(
                innerDocument.Children.Count == 0);

        }
    }
}
