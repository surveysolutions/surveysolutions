// -----------------------------------------------------------------------
// <copyright file="ClientEventSyncTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using RavenQuestionnaire.Core;
using Main.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;
using Web.CAPI.Synchronization;

namespace RavenQuestionnaire.Web.Tests.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class ClientEventSyncTests
    {
        [Test]
        public void ReadEvents_EventStoreIsEmpty_EmptyListReturned()
        {
            Mock<IViewRepository> repositoryMock=new Mock<IViewRepository>();
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            
            Assert.AreEqual(target.ReadEvents().Count(), 0);
           
        }

        [Test]
        public void ReadEvents_EventStoreContainsinitialQuestionnaires_EmptyListReturned()
        {
            Mock<IViewRepository> repositoryMock = new Mock<IViewRepository>();
            Mock<IEventStore> storeMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault<IEventStore>(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version())));
            var questionnaireList = new CompleteQuestionnaireBrowseView(1, 10, 1, new []
                                                                                      {
                                                                                          new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument(){PublicKey = eventSourceId, Status = SurveyStatus.Initial})
                                                                                      },
                                                                        string.Empty);
            repositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(
                    It.IsAny<CompleteQuestionnaireBrowseInputModel>())).Returns(questionnaireList);
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 0);
            repositoryMock.Verify(x => x.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(It.IsAny<CompleteQuestionnaireBrowseInputModel>()), Times.Once());
        }
        [Test]
        public void ReadEvents_EventStoreContainsCompleteQuestionnaires_NotEmptyListReturned()
        {
            Mock<IViewRepository> repositoryMock = new Mock<IViewRepository>();
            Mock<IEventStore> storeMock=new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault<IEventStore>(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version())));
            var questionnaireList = new CompleteQuestionnaireBrowseView(1, 10, 1, new[]
                                                                                      {
                                                                                          new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument(){PublicKey = eventSourceId, Status = SurveyStatus.Complete})
                                                                                      },
                                                                        string.Empty);
            repositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(
                    It.IsAny<CompleteQuestionnaireBrowseInputModel>())).Returns(questionnaireList);
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 1);
            repositoryMock.Verify(x => x.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(It.IsAny<CompleteQuestionnaireBrowseInputModel>()), Times.Once());
        }
        [Test]
        public void ReadEvents_EventStoreContainsErrorQuestionnaires_NotEmptyListReturned()
        {
            Mock<IViewRepository> repositoryMock = new Mock<IViewRepository>();
            Mock<IEventStore> storeMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault<IEventStore>(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version())));
            var questionnaireList = new CompleteQuestionnaireBrowseView(1, 10, 1, new[]
                                                                                      {
                                                                                          new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument(){PublicKey = eventSourceId, Status = SurveyStatus.Error})
                                                                                      },
                                                                        string.Empty);
            repositoryMock.Setup(
                x =>
                x.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(
                    It.IsAny<CompleteQuestionnaireBrowseInputModel>())).Returns(questionnaireList);
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 1);
            repositoryMock.Verify(x => x.Load<CompleteQuestionnaireBrowseInputModel, CompleteQuestionnaireBrowseView>(It.IsAny<CompleteQuestionnaireBrowseInputModel>()), Times.Once());
        }
    }
}
