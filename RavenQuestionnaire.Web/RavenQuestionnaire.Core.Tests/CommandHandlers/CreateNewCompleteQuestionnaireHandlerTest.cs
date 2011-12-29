using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;
using RavenQuestionnaire.Core.Tests.Utils;

namespace RavenQuestionnaire.Core.Tests.CommandHandlers
{
    [TestFixture]
    public class CreateNewCompleteQuestionnaireHandlerTest
    {
        [Test]
        public void WhenCommandIsReceivedWithInvalidQuestionnaireid_NullRefferenceException()
        {

            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
      
            ICompleteQuestionnaireUploaderService completeQuestionnaireService =
             new CompleteQuestionnaireUploaderService(coompleteQuestionnaireRepositoryMock.Object);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
        
            CreateNewCompleteQuestionnaireHandler handler = new CreateNewCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object, 
                completeQuestionnaireService);
            Assert.Throws<NullReferenceException>(() => handler.Handle(new Commands.CreateNewCompleteQuestionnaireCommand("invalid id", 
                new UserLight("-1", "dummyUser"), 
                new SurveyStatus("-1","dummyStatus"),
                null))); 
        }

        [Test]
        public void WhenCommandIsReceived_NewCompleteQuestionnIsAddedToRepository()
        {
            CompleteQuestionnaire entity = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            CompleteQuestionnaireDocument innerDocument =
               ((IEntity<CompleteQuestionnaireDocument>)entity).GetInnerDocument();

            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            ICompleteQuestionnaireUploaderService completeQuestionnaireService =
                new CompleteQuestionnaireUploaderService(coompleteQuestionnaireRepositoryMock.Object);
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(
                entity.GetQuestionnaireTemplate());


            CreateNewCompleteQuestionnaireHandler handler = 
                new CreateNewCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object, completeQuestionnaireService);
            CompleteAnswer[] answers = new CompleteAnswer[] { new CompleteAnswer(innerDocument.Questionnaire.Questions[0].Answers[0], 
                innerDocument.Questionnaire.Questions[0].PublicKey) };

            handler.Handle(new Commands.CreateNewCompleteQuestionnaireCommand("qID", 
                new UserLight("-2", "dummy-2"), 
                new SurveyStatus("-100", "dummyStatus100"), 
                null));

            coompleteQuestionnaireRepositoryMock.Verify(x => x.Add(It.IsAny<CompleteQuestionnaire>()), Times.Once());

        }
    }
}
