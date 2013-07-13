using System;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing.Storage.NoDB.Tests.Fakes;
using Ncqrs.Spec;
using NUnit.Framework;

namespace Ncqrs.Eventing.Storage.NoDB.Tests.EventStoreTests
{
    [TestFixture]
    //[Ignore("Tests failing when executed in CMD (e.q. running BUILD.bat), they succeed when executed in Visual Studio.")]
    public class when_saving_events_based_on_stale_state : NoDBEventStoreTestFixture
    {
        [SetUp]
        public void SetUp()
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
        }

        [Test, ExpectedException(typeof(ConcurrencyException))]
        public void it_should_throw_a_concurrency_exception()
        {
            var eventStream = Prepare.Events(new AccountTitleChangedEvent("Title"))
                .ForSourceUncomitted(EventSourceId, Guid.NewGuid());

            EventStore.Store(eventStream);
        }
    }
}