using System;
using Moq;
using NUnit.Framework;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
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
            Questionnaire questionnaireDocument =new Questionnaire("some");

            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            ICompleteQuestionnaireUploaderService completeQuestionnaireService =
                new CompleteQuestionnaireUploaderService(coompleteQuestionnaireRepositoryMock.Object);
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(questionnaireDocument);


            CreateNewCompleteQuestionnaireHandler handler = 
                new CreateNewCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object, completeQuestionnaireService);
        
            handler.Handle(new Commands.CreateNewCompleteQuestionnaireCommand("qID", 
                new UserLight("-2", "dummy-2"), 
                new SurveyStatus("-100", "dummyStatus100"), 
                null));

            coompleteQuestionnaireRepositoryMock.Verify(x => x.Add(It.IsAny<CompleteQuestionnaire>()), Times.Once());

        }
    }
}
