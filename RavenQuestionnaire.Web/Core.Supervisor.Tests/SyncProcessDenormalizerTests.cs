// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SyncProcessDenormalizerTests.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide;

namespace Core.Supervisor.Tests
{
    using System;

    using Core.Supervisor.Denormalizer;

    using Main.Core.Documents;
    using Main.Core.Events.Synchronization;
    using Main.Core.View.CompleteQuestionnaire;
    using Main.DenormalizerStorage;

    using Moq;

    using Ncqrs.Eventing;
    using Ncqrs.Eventing.ServiceModel.Bus;

    using NUnit.Framework;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [TestFixture]
    public class SyncProcessDenormalizerTests
    {
        /// <summary>
        /// The handle new synchronization process created_ event is come_ one new item is added to storage.
        /// </summary>
        [Test]
        public void HandleNewSynchronizationProcessCreated_EventIsCome_OneNewItemIsAddedToStorage()
        {
            var storage = new Mock<IReadSideRepositoryWriter<SyncProcessStatisticsDocument>>();

            var denormalizer = new SyncProcessDenormalizer(storage.Object);

            var evnt = new NewSynchronizationProcessCreated
                {
                   ProcessGuid = Guid.NewGuid(), SynckType = SynchronizationType.Push 
                };

            IPublishedEvent<NewSynchronizationProcessCreated> e =
                new PublishedEvent<NewSynchronizationProcessCreated>(
                    new UncommittedEvent(Guid.NewGuid(), evnt.ProcessGuid, 1, 1, DateTime.Now, evnt, new Version(1, 0)));

            denormalizer.Handle(e);

            storage.Verify(x => x.Store(It.IsAny<SyncProcessStatisticsDocument>(), evnt.ProcessGuid), Times.Once());
        }

        /// <summary>
        /// The handle process ended_ event is come_ is ended set in true.
        /// </summary>
        [Test]
        public void HandleProcessEnded_EventIsCome_IsEndedSetInTrue()
        {
            var statistics = new SyncProcessStatisticsDocument(Guid.NewGuid())
                {
                   CreationDate = DateTime.Now, SyncType = SynchronizationType.Push 
                };

            var storage = new Mock<IReadSideRepositoryWriter<SyncProcessStatisticsDocument>>();
            storage.Setup(d => d.GetById(Guid.Empty)).Returns(statistics);


            var denormalizer = new SyncProcessDenormalizer(storage.Object);

            var evnt = new ProcessEnded { Status = EventState.Completed };

            IPublishedEvent<ProcessEnded> e =
                new PublishedEvent<ProcessEnded>(
                    new UncommittedEvent(Guid.NewGuid(), Guid.NewGuid(), 1, 1, DateTime.Now, evnt, new Version(1, 0)));

            denormalizer.Handle(e);

            Assert.IsTrue(statistics.IsEnded);
        }
    }
}