using System;
using Moq;
using NUnit.Framework;
using Ninject;
using RavenQuestionnaire.Core.CommandHandlers;
using RavenQuestionnaire.Core.CommandHandlers.Questionnaire.Completed;
using RavenQuestionnaire.Core.Commands.Questionnaire.Completed;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.Subscribers;
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
            kernel.Bind<ISubscriber>().ToConstant(new Subscriber(kernel));
        }
        [Test]
        public void WhenCommandIsReceivedWithInvalidQuestionnaireid_NullRefferenceException()
        {

            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            Mock<IStatisticRepository> statisticsRepositoryMock = new Mock<IStatisticRepository>();
            Mock<ICommandInvokerAsync> asyncMock = new Mock<ICommandInvokerAsync>();
            Mock<ISubscriber> subscriberMock = new Mock<ISubscriber>();
            ICompleteQuestionnaireUploaderService completeQuestionnaireService =
             new CompleteQuestionnaireUploaderService(coompleteQuestionnaireRepositoryMock.Object, statisticsRepositoryMock.Object, asyncMock.Object, subscriberMock.Object);
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
        
            CreateNewCompleteQuestionnaireHandler handler = new CreateNewCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object, 
                completeQuestionnaireService);
            Assert.Throws<NullReferenceException>(
                () => handler.Handle(new CreateNewCompleteQuestionnaireCommand("invalid id",
                                                                               new UserLight("-1", "dummyUser"),
                                                                               new SurveyStatus(Guid.Empty, "dummyStatus"),
                                                                               null))); 
        }

        [Test]
        public void WhenCommandIsReceived_NewCompleteQuestionnIsAddedToRepository()
        {
            CompleteQuestionnaire entity = CompleteQuestionnaireFactory.CreateCompleteQuestionnaireWithAnswersInBaseQuestionnaire();
            Questionnaire questionnaireDocument =new Questionnaire("some");

            Mock<ICompleteQuestionnaireRepository> coompleteQuestionnaireRepositoryMock = new Mock<ICompleteQuestionnaireRepository>();
            Mock<IStatisticRepository> statisticsRepositoryMock = new Mock<IStatisticRepository>();
            Mock<ICommandInvokerAsync> asyncMock = new Mock<ICommandInvokerAsync>();
            Mock<IQuestionnaireRepository> questionnaireRepositoryMock = new Mock<IQuestionnaireRepository>();
            Mock<ISubscriber> subscriberMock = new Mock<ISubscriber>();
            ICompleteQuestionnaireUploaderService completeQuestionnaireService =
                new CompleteQuestionnaireUploaderService(coompleteQuestionnaireRepositoryMock.Object, statisticsRepositoryMock.Object, asyncMock.Object, subscriberMock.Object);
            questionnaireRepositoryMock.Setup(x => x.Load("questionnairedocuments/qID")).Returns(questionnaireDocument);


            CreateNewCompleteQuestionnaireHandler handler = 
                new CreateNewCompleteQuestionnaireHandler(questionnaireRepositoryMock.Object, completeQuestionnaireService);

            handler.Handle(new CreateNewCompleteQuestionnaireCommand("qID",
                                                                     new UserLight("-2", "dummy-2"),
                                                                     new SurveyStatus(Guid.NewGuid(), "dummyStatus100"), 
                                                                     null));

            coompleteQuestionnaireRepositoryMock.Verify(x => x.Add(It.IsAny<CompleteQuestionnaire>()), Times.Once());

        }
    }
}
