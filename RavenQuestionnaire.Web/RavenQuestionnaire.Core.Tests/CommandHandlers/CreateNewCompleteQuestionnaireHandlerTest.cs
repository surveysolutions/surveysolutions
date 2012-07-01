using System;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
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
        [SetUp]
        public void CreateObjects()
        {
            IKernel kernel = new StandardKernel();
        }
        [Test]
        public void WhenCommandIsReceivedWithInvalidQuestionnaireid_NullRefferenceException()
        {

            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            Mock<IStatisticRepository> statisticsRepositoryMock = new Mock<IStatisticRepository>();
            ICompleteQuestionnaireUploaderService completeQuestionnaireService =
             new CompleteQuestionnaireUploaderService(coompleteQuestionnaireRepositoryMock.Object, statisticsRepositoryMock.Object);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
        
            CreateNewCompleteQuestionnaireHandler handler = new CreateNewCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object, 
                completeQuestionnaireService);
            Assert.Throws<NullReferenceException>(
                () => handler.Handle(new CreateNewCompleteQuestionnaireCommand("invalid id", Guid.NewGuid(),
                                                                               new UserLight("-1", "dummyUser"),
                                                                               new SurveyStatus(Guid.Empty, "dummyStatus"),
                                                                               null))); 
        }

        [Test]
        public void WhenCommandIsReceived_NewCompleteQuestionnIsAddedToRepository()
        {
            CompleteQuestionnaire entity = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            Guid key = Guid.NewGuid();

            Questionnaire questionnaireDocument =new Questionnaire("some", Guid.NewGuid());

            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            Mock<IStatisticRepository> statisticsRepositoryMock = new Mock<IStatisticRepository>();
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            
            ICompleteQuestionnaireUploaderService completeQuestionnaireService =
                new CompleteQuestionnaireUploaderService(coompleteQuestionnaireRepositoryMock.Object, statisticsRepositoryMock.Object);
            questionnaireRepositoryMock.Setup(x => x.Load(key.ToString())).Returns(questionnaireDocument);


            CreateNewCompleteQuestionnaireHandler handler = 
                new CreateNewCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object, completeQuestionnaireService);

            handler.Handle(new CreateNewCompleteQuestionnaireCommand(key.ToString(), Guid.NewGuid(),
                                                                     new UserLight("-2", "dummy-2"),
                                                                     new SurveyStatus(Guid.NewGuid(), "dummyStatus100"), 
                                                                     null));

            coompleteQuestionnaireRepositoryMock.Verify(x => x.Add(It.IsAny<CompleteQuestionnaire>()), Times.Once());

        }
    }
}
