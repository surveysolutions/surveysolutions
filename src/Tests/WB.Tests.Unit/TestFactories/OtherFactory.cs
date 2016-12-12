using System;
using System.Threading.Tasks;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Spec;
using NSubstitute;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Unit.TestFactories
{
    internal class OtherFactory
    {
        public CommittedEvent CommittedEvent(UncommittedEvent evnt, Guid eventSourceId)
            => new CommittedEvent(Guid.NewGuid(),
                        evnt.Origin,
                        evnt.EventIdentifier,
                        eventSourceId,
                        evnt.EventSequence,
                        evnt.EventTimeStamp,
                        0,
                        evnt.Payload);

        public CommittedEvent CommittedEvent(string origin = null, Guid? eventSourceId = null, IEvent payload = null,
            Guid? eventIdentifier = null, int eventSequence = 1, Guid? commitId = null)
            => new CommittedEvent(
                commitId ?? Guid.NewGuid(),
                origin,
                eventIdentifier ?? Guid.Parse("44440000444440000004444400004444"),
                eventSourceId ?? Guid.Parse("55550000555550000005555500005555"),
                eventSequence,
                new DateTime(2014, 10, 22),
                0,
                payload ?? Mock.Of<IEvent>());

        public EventContext EventContext()
            => new EventContext();

        public NavigationState NavigationState(IStatefulInterviewRepository interviewRepository = null)
        {
            return new NavigationState(
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(), Substitute.For<IViewModelNavigationService>());
        }

        public UncommittedEvent UncommittedEvent(Guid? eventSourceId = null, IEvent payload = null, int sequence = 1, int initialVersion = 1)
            => new UncommittedEvent(Guid.NewGuid(), eventSourceId ?? Guid.NewGuid(), sequence, initialVersion, DateTime.Now, payload);
    }
}