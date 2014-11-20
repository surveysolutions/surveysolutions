using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;

namespace WB.Core.Infrastructure.Raven.Tests.RavenReadSideServiceTests
{
    [Subject(typeof(RavenReadSideService))]
    internal class RavenReadSideServiceTestContext
    {
        protected static RavenReadSideService CreateRavenReadSideService(IStreamableEventStore streamableEventStore = null, IEventDispatcher eventDispatcher=null)
        {
            return new RavenReadSideService(streamableEventStore ?? Mock.Of<IStreamableEventStore>(),
                eventDispatcher ?? Mock.Of<IEventDispatcher>(), Mock.Of<ILogger>());
        }
    }
}
