using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.Storage.Raven.Implementation.ReadSide;

namespace WB.Tests.Unit.Infrastructure.ReadSideServiceTests
{
    [Subject(typeof(ReadSideService))]
    internal class ReadSideServiceTestContext
    {
        protected static ReadSideService CreateRavenReadSideService(IStreamableEventStore streamableEventStore = null, IEventDispatcher eventDispatcher=null)
        {
            ReadSideService.InstanceCount = 0;

            return new ReadSideService(streamableEventStore ?? Mock.Of<IStreamableEventStore>(),
                eventDispatcher ?? Mock.Of<IEventDispatcher>(), Mock.Of<ILogger>());
        }
    }
}
