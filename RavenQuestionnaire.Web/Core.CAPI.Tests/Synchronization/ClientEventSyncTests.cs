// -----------------------------------------------------------------------
// <copyright file="ClientEventSyncTests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using Core.CAPI.Synchronization;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;
using Main.DenormalizerStorage;
using Moq;
using NUnit.Framework;
using Ncqrs;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;

namespace Core.CAPI.Tests.Synchronization
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class ClientEventSyncTests
    {
        [Test]
        public void ReadEvents_EventStoreIsEmpty_EmptyListReturned()
        {
            Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>> repositoryMock = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            
            Assert.AreEqual(target.ReadEvents().Count(), 0);
           
        }

        [Test]
        public void ReadEvents_EventStoreContainsinitialQuestionnaires_EmptyListReturned()
        {
            Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>> repositoryMock = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            Mock<IEventStore> storeMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault<IEventStore>(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version())));
            var questionnaireList = new[]
                                        {
                                            new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument()
                                                                                    {
                                                                                        PublicKey = eventSourceId,
                                                                                        Status = SurveyStatus.Initial
                                                                                    })
                                        }.AsQueryable();
            repositoryMock.Setup(
                x =>
                x.Query()).Returns(questionnaireList);
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 0);
            repositoryMock.Verify(x => x.Query(), Times.Once());
        }
        [Test]
        public void ReadEvents_EventStoreContainsCompleteQuestionnaires_NotEmptyListReturned()
        {
            Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>> repositoryMock = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            Mock<IEventStore> storeMock=new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault<IEventStore>(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version())));
            var questionnaireList = new[]
                                                                                      {
                                                                                          new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument(){PublicKey = eventSourceId, Status = SurveyStatus.Complete})
                                                                                      }.AsQueryable();
            repositoryMock.Setup(
                x =>
                x.Query()).Returns(questionnaireList);
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 1);
            repositoryMock.Verify(x => x.Query(), Times.Once());
        }
        [Test]
        public void ReadEvents_EventStoreContainsErrorQuestionnaires_NotEmptyListReturned()
        {
            Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>> repositoryMock = new Mock<IDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            Mock<IEventStore> storeMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault<IEventStore>(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(eventSourceId,
                                         new CommittedEvent(Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1,
                                                            DateTime.Now, new object(), new Version())));
            var questionnaireList = new[]
                                        {
                                            new CompleteQuestionnaireBrowseItem(new CompleteQuestionnaireDocument()
                                                                                    {
                                                                                        PublicKey = eventSourceId,
                                                                                        Status = SurveyStatus.Error
                                                                                    })
                                        }.AsQueryable();
            repositoryMock.Setup(
                x =>
                x.Query()).Returns(questionnaireList);
            ClientEventSync target = new ClientEventSync(repositoryMock.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 1);
            repositoryMock.Verify(x => x.Query(), Times.Once());
        }
    }
}
