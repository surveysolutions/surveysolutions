// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommonInfrastuctureTests.cs" company="">
//   
// </copyright>
// <summary>
//   The common infrastucture tests.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Microsoft.Practices.ServiceLocation;
using Moq;

namespace AndroidMain.Core.Tests.CommonTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Android.App;
    using Android.Content;

    using AndroidMain.Core.Tests.AndroidSpecificTests;

    using AndroidNcqrs.Eventing.Storage.SQLite;

    using Main.Core;
    using Main.Core.Commands.Questionnaire.Completed;
    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Services;

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
           // Application.Context.DeleteDatabase(DataBaseHelper.DATABASE_NAME);

            var registry = new TestsRegistry();

            this._kernel = new StandardKernel();
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);

            Context appContext = TestsContext.CurrentContext;

            var store = new MvvmCrossSqliteEventStore(_testEventStore);
            this._kernel.Bind<IEventStore>().ToConstant(store).InSingletonScope();

            this._kernel.Bind<IFileStorageService>().To<FakeFileStorage>();

            this._kernel.Load(new FakeCore());

            NcqrsInit.Init(this._kernel);
        }

        // [Test, Ignore]
        // public void should_desirialize_event()
        // {
        // var storedEvent = StoredEvent.GetCreateTemplateEvent();

        // Assert.NotNull(storedEvent);

        // Assert.That(storedEvent.EventIdentifier, 
        // Is.EqualTo(Guid.Parse("6de866ac-2a8d-4dc9-8a85-bf425b318caa")));
        // }

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
            var command = new CreateCompleteQuestionnaireCommand(newTemplateGuid, sourceId, new UserLight());

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

            var result = new UncommittedEventStream(storedEvent.EventSourceId);
            result.Append(@event);

            return result;
        }
    }

    public class TestsRegistry : CoreRegistry
    {
        public TestsRegistry()
            : base(null, false)
        {
        }
    }

    internal class FakeCore : CoreRegistry
    {
        public FakeCore()
            : base(null, false)
        {
        }
    }

    public class FakeFileStorage : IFileStorageService
    {
        public void DeleteFile(string filename)
        {
        }

        public FileDescription RetrieveFile(string filename)
        {
            return new FileDescription();
        }

        public FileDescription RetrieveThumb(string filename)
        {
            return new FileDescription();
        }

        public void StoreFile(FileDescription file)
        {
        }
    }
}