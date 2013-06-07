// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientEventSyncTests.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;

namespace Core.CAPI.Tests.Synchronization
{
    using System;
    using System.Linq;

    using Core.CAPI.Synchronization;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using Ncqrs;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Storage;

    using NUnit.Framework;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class ClientEventSyncTests
    {
        #region Public Methods and Operators

        /// <summary>
        /// The read events_ event store contains complete questionnaires_ not empty list returned.
        /// </summary>
        [Test]
        public void ReadEvents_EventStoreContainsCompleteQuestionnaires_NotEmptyListReturned()
        {
            var repositoryMock = new Mock<IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            var storeMock = new Mock<IEventStore>();
            var users = new Mock<IQueryableDenormalizerStorage<UserDocument>>();
            NcqrsEnvironment.SetDefault(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(
                    eventSourceId, 
                    new CommittedEvent(
                        Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object(), new Version())));
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaireList =
                new[]
                    {
                        new CompleteQuestionnaireBrowseItem(
                            new CompleteQuestionnaireDocument
                                {
                                   PublicKey = eventSourceId, Status = SurveyStatus.Complete 
                                })
                    }.AsQueryable();
            repositoryMock.Setup(x => x.Query()).Returns(questionnaireList);
            var target = new ClientEventStreamReader(repositoryMock.Object, users.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 1);
            repositoryMock.Verify(x => x.Query(), Times.Once());
        }

        /// <summary>
        /// The read events_ event store contains error questionnaires_ not empty list returned.
        /// </summary>
        [Test]
        public void ReadEvents_EventStoreContainsErrorQuestionnaires_NotEmptyListReturned()
        {
            var repositoryMock = new Mock<IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            var users = new Mock<IQueryableDenormalizerStorage<UserDocument>>();
            var storeMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(
                    eventSourceId, 
                    new CommittedEvent(
                        Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object(), new Version())));
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaireList =
                new[]
                    {
                        new CompleteQuestionnaireBrowseItem(
                            new CompleteQuestionnaireDocument { PublicKey = eventSourceId, Status = SurveyStatus.Error })
                    }.AsQueryable();
            repositoryMock.Setup(x => x.Query()).Returns(questionnaireList);
            var target = new ClientEventStreamReader(repositoryMock.Object, users.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 1);
            repositoryMock.Verify(x => x.Query(), Times.Once());
        }

        /// <summary>
        /// The read events_ event store containsinitial questionnaires_ empty list returned.
        /// </summary>
        [Test]
        public void ReadEvents_EventStoreContainsinitialQuestionnaires_EmptyListReturned()
        {
            var repositoryMock = new Mock<IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            var users = new Mock<IQueryableDenormalizerStorage<UserDocument>>();
            var storeMock = new Mock<IEventStore>();
            NcqrsEnvironment.SetDefault(storeMock.Object);
            Guid eventSourceId = Guid.NewGuid();
            storeMock.Setup(x => x.ReadFrom(eventSourceId, int.MinValue, int.MaxValue)).Returns(
                new CommittedEventStream(
                    eventSourceId, 
                    new CommittedEvent(
                        Guid.NewGuid(), Guid.NewGuid(), eventSourceId, 1, DateTime.Now, new object(), new Version())));
            IQueryable<CompleteQuestionnaireBrowseItem> questionnaireList =
                new[]
                    {
                        new CompleteQuestionnaireBrowseItem(
                            new CompleteQuestionnaireDocument
                                {
                                   PublicKey = eventSourceId, Status = SurveyStatus.Initial 
                                })
                    }.AsQueryable();
            repositoryMock.Setup(x => x.Query()).Returns(questionnaireList);
            var target = new ClientEventStreamReader(repositoryMock.Object, users.Object);
            Assert.AreEqual(target.ReadEvents().Count(), 0);
            repositoryMock.Verify(x => x.Query(), Times.Once());
        }

        /// <summary>
        /// The read events_ event store is empty_ empty list returned.
        /// </summary>
        [Test]
        public void ReadEvents_EventStoreIsEmpty_EmptyListReturned()
        {
            var repositoryMock = new Mock<IQueryableDenormalizerStorage<CompleteQuestionnaireBrowseItem>>();
            var users = new Mock<IQueryableDenormalizerStorage<UserDocument>>();
            var target = new ClientEventStreamReader(repositoryMock.Object, users.Object);

            Assert.AreEqual(target.ReadEvents().Count(), 0);
        }

        #endregion
    }
}