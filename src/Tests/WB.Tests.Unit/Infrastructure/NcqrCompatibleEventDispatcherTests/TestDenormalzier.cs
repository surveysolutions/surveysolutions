using System;
using System.Collections.Generic;
using System.Linq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventHandlers;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Unit.Infrastructure.NcqrCompatibleEventDispatcherTests
{
    public class TestDenormalzier : IEventHandler,
        IEventHandler<InterviewCreated>
    {
        public string Name { get; }

        public virtual void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestDenormalzier1 : IEventHandler,
        IEventHandler<InterviewCreated>
    {
        public string Name { get; }

        public virtual void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            throw new System.NotImplementedException();
        }
    }

    public class TestFunctionalDenormalzierNonThrowing : IFunctionalEventHandler
    {
        public int HandleCount { get; private set; }

        public int TotalEvents { get; private set; }

        public void Handle(IEnumerable<IPublishableEvent> publishableEvents)
        {
            HandleCount++;
            TotalEvents += publishableEvents.Count();
        }
    }

    public class TestDenormalzierNonThrowing : IEventHandler,
        IEventHandler<InterviewCreated>
    {
        public string Name { get; }

        public int HandleCount { get; private set; }
        public virtual void Handle(IPublishedEvent<InterviewCreated> evnt)
        {
            HandleCount++;
        }
    }
}
