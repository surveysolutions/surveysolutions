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

    /// <summary>
    /// The common infrastucture tests.
    /// </summary>
    [TestFixture]
    public class CommonInfrastuctureTests
    {
        private const string _testEventStore = "test_event_store";

        #region Fields

        /// <summary>
        /// The _kernel.
        /// </summary>
        private IKernel _kernel;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The set up.
        /// </summary>
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

        /// <summary>
        /// The store_template_event_in_db.
        /// </summary>
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

        #endregion

        #region Methods

        /// <summary>
        /// The get stream.
        /// </summary>
        /// <param name="storedEvent">
        /// The stored event.
        /// </param>
        /// <returns>
        /// The <see cref="UncommittedEventStream"/>.
        /// </returns>
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

        #endregion
    }

    /// <summary>
    /// The tests registry.
    /// </summary>
    public class TestsRegistry : CoreRegistry
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TestsRegistry"/> class.
        /// </summary>
        public TestsRegistry()
            : base(null, false)
        {
        }

        #endregion
    }

    /// <summary>
    /// The fake core.
    /// </summary>
    internal class FakeCore : CoreRegistry
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeCore"/> class.
        /// </summary>
        public FakeCore()
            : base(null, false)
        {
        }

        #endregion
    }

    /// <summary>
    /// The fake file storage.
    /// </summary>
    public class FakeFileStorage : IFileStorageService
    {
        #region Public Methods and Operators

        /// <summary>
        /// The delete file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        public void DeleteFile(string filename)
        {
        }

        /// <summary>
        /// The retrieve file.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="FileDescription"/>.
        /// </returns>
        public FileDescription RetrieveFile(string filename)
        {
            return new FileDescription();
        }

        /// <summary>
        /// The retrieve thumb.
        /// </summary>
        /// <param name="filename">
        /// The filename.
        /// </param>
        /// <returns>
        /// The <see cref="FileDescription"/>.
        /// </returns>
        public FileDescription RetrieveThumb(string filename)
        {
            return new FileDescription();
        }

        /// <summary>
        /// The store file.
        /// </summary>
        /// <param name="file">
        /// The file.
        /// </param>
        public void StoreFile(FileDescription file)
        {
        }

        #endregion
    }
}