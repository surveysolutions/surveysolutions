﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonInfrastuctureTests.cs" company="">
//   
// </copyright>
// <summary>
//   The common infrastucture tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.Sourcing.Snapshotting;
using WB.Core.Infrastructure.CommandBus;

namespace AndroidMain.Core.Tests.CommonTests
{
    using System;
    using System.Linq;

    using Android.Content;

    using AndroidMain.Core.Tests.AndroidSpecificTests;

    using AndroidNcqrs.Eventing.Storage.SQLite;

    using Main.Core;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;
    using Ncqrs.Eventing;
    using Ncqrs.Eventing.Storage;

    using Ninject;

    using NUnit.Framework;

    [TestFixture]
    public class CommonInfrastuctureTests
    {
        private const string _testEventStore = "test_event_store";

        private IKernel _kernel;

        [SetUp]
        public void SetUp()
        {
            var registry = new TestsRegistry();

            this._kernel = new StandardKernel();
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            Context appContext = TestsContext.CurrentContext;

            var store = new MvvmCrossSqliteEventStore(_testEventStore);
            this._kernel.Bind<IEventStore>().ToConstant(store).InSingletonScope();

            this._kernel.Load(new FakeCore());

            NcqrsEnvironment.SetDefault<ISnapshottingPolicy>(new SimpleSnapshottingPolicy(1));

            var snpshotStore = new InMemoryEventStore();
            // key param for storing im memory
            NcqrsEnvironment.SetDefault<ISnapshotStore>(snpshotStore);

            var bus = new InProcessEventBus(true);
            NcqrsEnvironment.SetDefault<IEventBus>(bus);
            this._kernel.Bind<IEventBus>().ToConstant(bus);
        }

        [Test]
        public void store_template_event_in_db()
        {
            StoredEvent storedEvent = StoredEvent.GetCreateTemplateEvent();

            Guid sourceId = storedEvent.EventSourceId;

            var store = NcqrsEnvironment.Get<IEventStore>() as MvvmCrossSqliteEventStore;

            Assert.NotNull(store);

            UncommittedEventStream eventStream = this.GetStream(storedEvent);

            store.Store(eventStream);

            CommittedEventStream storedEvents = store.ReadFrom(sourceId, long.MinValue, long.MaxValue);

            Assert.That(storedEvents.Count(), Is.EqualTo(1));

            Guid newTemplateGuid = Guid.NewGuid();
            ICommand command = null; // TODO: use another command here instead of new CreateCompleteQuestionnaireCommand(newTemplateGuid, sourceId, new UserLight());

            var commandService = NcqrsEnvironment.Get<ICommandService>();
            commandService.Execute(command);


            CommittedEventStream newStoredEvents = store.ReadFrom(newTemplateGuid, long.MinValue, long.MaxValue);
            Assert.That(newStoredEvents.Count(), Is.EqualTo(1));
        }

        private UncommittedEventStream GetStream(StoredEvent storedEvent)
        {
            var @event = new UncommittedEvent(
                storedEvent.EventIdentifier, 
                storedEvent.EventSourceId, 
                storedEvent.EventSequence, 
                0, 
                storedEvent.EventTimeStamp, 
                storedEvent.Data, 
                new Version(1, 0));

            var result = new UncommittedEventStream(storedEvent.EventSourceId, null);
            result.Append(@event);

            return result;
        }
    }

    public class TestsRegistry : CoreRegistry {}

    internal class FakeCore : CoreRegistry {}
}