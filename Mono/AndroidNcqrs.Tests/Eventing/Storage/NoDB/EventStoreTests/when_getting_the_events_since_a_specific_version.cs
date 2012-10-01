using System.Linq;
using Ncqrs.Eventing.Sourcing;
using NUnit.Framework;

namespace Ncqrs.Eventing.Storage.NoDB.Tests.EventStoreTests
{
    [TestFixture]
    public class when_getting_the_events_since_a_specific_version : NoDBEventStoreTestFixture
    {
        private object[] _returnedEvents;

        [Test]
        public void it_should_return_the_events_since_version()
        {
	        var versions = new [] {0, 1, 2, 3};
	        foreach (var version in versions)
	        {
		        ActualOperation(version);
	        }
        }

		public void ActualOperation(int version)
		{
			_returnedEvents = EventStore.ReadFrom(EventSourceId, version, long.MaxValue).Select(x => x.Payload).ToArray();
			for (int i = 0; i < _returnedEvents.Length; i++)
			{
				Assert.That(_returnedEvents[i], Is.EqualTo(Events[i + version]));
			}
		}
    }
}