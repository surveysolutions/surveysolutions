using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class UpdateCompleteQuestionnaireHandlerTest
    {
        [SetUp]
        public void CreateObjects()
        {
           // iteratorContainerMock = new Mock<IIteratorContainer>();
        }
        [Test]
        public void WhenCommandIsReceived_CompleteQuestionnaireIsUpdatedToRepository()
        {
            CompleteQuestionnaireDocument innerDocument = new CompleteQuestionnaireDocument();
            innerDocument.Id = "cqID";
            CompleteQuestion question = new CompleteQuestion("?", QuestionType.SingleOption);

            innerDocument.Questions.Add(question);
            CompleteAnswer answer = new CompleteAnswer(new Answer(), question.PublicKey);
            question.Answers.Add(answer);

            CompleteQuestionnaire entity = new CompleteQuestionnaire(innerDocument);
            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();

            Mock<IStatusRepository> statusRepositoryMock = new Mock<IStatusRepository>();
            Mock<IUserRepository> userRepositoryMock = new Mock<IUserRepository>();

            coompleteQuestionnaireRepositoryMock.Setup(x => x.Load("completequestionnairedocuments/cqID")).Returns(entity);

            UpdateCompleteQuestionnaireHandler handler = new UpdateCompleteQuestionnaireHandler(coompleteQuestionnaireRepositoryMock.Object,
                statusRepositoryMock.Object, userRepositoryMock.Object);

            handler.Handle(new UpdateCompleteQuestionnaireCommand("cqID", 
                Guid.Empty,
                "test status change",
                "-111", 
                null));

            coompleteQuestionnaireRepositoryMock.Verify(x => x.Load("completequestionnairedocuments/cqID"));
       
        }
    }
}
