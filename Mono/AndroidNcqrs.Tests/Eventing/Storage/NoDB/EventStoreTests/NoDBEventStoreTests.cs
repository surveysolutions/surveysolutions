using System;
using System.IO;
using Ncqrs.Eventing.Sourcing;
using Ncqrs.Eventing.Storage.NoDB.Tests.Fakes;
using Ncqrs.Spec;
using NUnit.Framework;

namespace Ncqrs.Eventing.Storage.NoDB.Tests.EventStoreTests
{
    public abstract class NoDBEventStoreTestFixture
    {
        protected NoDBEventStore EventStore;
        protected object[] Events;
        protected Guid EventSourceId;

	    private string _baseDirectoryPath;

        [TestFixtureSetUp]
        public void BaseSetup()
        {
	        _baseDirectoryPath = Path.Combine(Path.GetTempPath(), GetType().Name);

            EventStore = new NoDBEventStore(_baseDirectoryPath);
            EventSourceId = Guid.NewGuid();
            Guid entityId = Guid.NewGuid();
            Events = new object[] {new AccountTitleChangedEvent("Title")};
            var eventStream = Prepare.Events(Events)
                .ForSourceUncomitted(EventSourceId, Guid.NewGuid());
            EventStore.Store(eventStream);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Directory.Delete(GetPath(), true);
        }

        protected string GetPath()
        {
            //return "./NoDBTests/" + GetType().Name+"/"+EventSourceId.ToString().Substring(0, 2);
	        return _baseDirectoryPath;
        }
    }
}