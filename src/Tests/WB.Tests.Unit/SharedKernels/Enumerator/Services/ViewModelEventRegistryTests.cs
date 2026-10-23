using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services
{
    [TestOf(typeof(ViewModelEventRegistry))]
    public class ViewModelEventRegistryTests
    {
        [Test]
        public void when_routing_entity_events_should_match_interview_and_identity()
        {
            var registry = Create.Service.LiteEventRegistry();
            var interviewId = Guid.NewGuid();
            var otherInterviewId = Guid.NewGuid();
            var identity = Create.Entity.Identity(Id.g1, RosterVector.Empty);
            var otherIdentity = Create.Entity.Identity(Id.g2, RosterVector.Empty);
            var matchingHandler = new StateEventsHandler();
            var otherIdentityHandler = new StateEventsHandler();
            var otherInterviewHandler = new StateEventsHandler();

            registry.Subscribe(matchingHandler, interviewId.ToString("N"), identity);
            registry.Subscribe(otherIdentityHandler, interviewId.ToString("N"), otherIdentity);
            registry.Subscribe(otherInterviewHandler, otherInterviewId.ToString("N"), identity);

            var committedEvent = Create.Other.CommittedEvent(
                eventSourceId: interviewId,
                payload: Create.Event.AnswersDeclaredValid(identity));

            registry.GetViewModelsByEvent(committedEvent)
                .Should().ContainSingle().Which.Should().BeSameAs(matchingHandler);
        }

        [Test]
        public void when_handler_has_global_and_entity_subscriptions_should_return_it_once()
        {
            var registry = Create.Service.LiteEventRegistry();
            var interviewId = Guid.NewGuid();
            var identity = Create.Entity.Identity(Id.g1, RosterVector.Empty);
            var handler = new StateEventsHandler();
            registry.Subscribe(handler, interviewId.ToString("N"));
            registry.Subscribe(handler, interviewId.ToString("N"), identity);

            var committedEvent = Create.Other.CommittedEvent(
                eventSourceId: interviewId,
                payload: Create.Event.AnswersDeclaredValid(identity));

            registry.GetViewModelsByEvent(committedEvent)
                .Should().ContainSingle().Which.Should().BeSameAs(handler);
        }

        [Test]
        public void when_unsubscribing_or_resetting_should_remove_all_subscriptions()
        {
            var registry = Create.Service.LiteEventRegistry();
            var interviewId = Guid.NewGuid();
            var identity = Create.Entity.Identity(Id.g1, RosterVector.Empty);
            var handler = new StateEventsHandler();
            registry.Subscribe(handler, interviewId.ToString("N"));
            registry.Subscribe(handler, interviewId.ToString("N"), identity);
            var committedEvent = Create.Other.CommittedEvent(
                eventSourceId: interviewId,
                payload: Create.Event.AnswersDeclaredValid(identity));

            registry.Unsubscribe(handler);

            registry.IsSubscribed(handler).Should().BeFalse();
            registry.GetViewModelsByEvent(committedEvent).Should().BeEmpty();

            registry.Subscribe(handler, interviewId.ToString("N"), identity);
            registry.Reset();

            registry.IsSubscribed(handler).Should().BeFalse();
            registry.GetViewModelsByEvent(committedEvent).Should().BeEmpty();
        }

        [Test]
        public void when_removing_interview_should_remove_global_and_entity_subscriptions()
        {
            var registry = Create.Service.LiteEventRegistry();
            var interviewId = Guid.NewGuid();
            var identity = Create.Entity.Identity(Id.g1, RosterVector.Empty);
            var handler = new StateEventsHandler();
            registry.Subscribe(handler, interviewId.ToString("N"));
            registry.Subscribe(handler, interviewId.ToString("N"), identity);

            registry.RemoveAggregateRoot(interviewId.ToString("N"));

            registry.IsSubscribed(handler).Should().BeFalse();
        }

        [Test]
        public void when_routing_state_events_should_route_every_affected_entity_event_shape()
        {
            var registry = Create.Service.LiteEventRegistry();
            var interviewId = Guid.NewGuid();
            var identity = Create.Entity.Identity(Id.g1, RosterVector.Empty);
            var handler = new StateEventsHandler();
            registry.Subscribe(handler, interviewId.ToString("N"), identity);

            var events = new IEvent[]
            {
                Create.Event.AnswersDeclaredValid(identity),
                Create.Event.AnswersDeclaredInvalid(identity),
                Create.Event.AnswersDeclaredPlausible(identity),
                Create.Event.AnswersDeclaredImplausible(identity, new[] { 0 }),
                Create.Event.StaticTextsDeclaredValid(identity),
                Create.Event.StaticTextsDeclaredInvalid(identity),
                Create.Event.StaticTextsDeclaredPlausible(identity),
                Create.Event.StaticTextsDeclaredImplausible(identity, new[] { 0 }),
                Create.Event.QuestionsEnabled(identity),
                Create.Event.QuestionsDisabled(new[] { identity }),
                Create.Event.StaticTextsEnabled(identity),
                Create.Event.StaticTextsDisabled(identity),
                Create.Event.GroupsEnabled(new[] { identity }),
                Create.Event.GroupsDisabled(new[] { identity }),
                Create.Event.SubstitutionTitlesChanged(
                    questions: new[] { identity },
                    staticTexts: new[] { identity },
                    groups: new[] { identity })
            };

            foreach (var @event in events)
            {
                var committedEvent = Create.Other.CommittedEvent(eventSourceId: interviewId, payload: @event);
                registry.GetViewModelsByEvent(committedEvent)
                    .Should().ContainSingle().Which.Should().BeSameAs(handler, @event.GetType().Name);
            }
        }

        private sealed class StateEventsHandler :
            IAsyncViewModelEventHandler<AnswersDeclaredValid>,
            IAsyncViewModelEventHandler<AnswersDeclaredInvalid>,
            IAsyncViewModelEventHandler<AnswersDeclaredPlausible>,
            IAsyncViewModelEventHandler<AnswersDeclaredImplausible>,
            IAsyncViewModelEventHandler<StaticTextsDeclaredValid>,
            IAsyncViewModelEventHandler<StaticTextsDeclaredInvalid>,
            IAsyncViewModelEventHandler<StaticTextsDeclaredPlausible>,
            IAsyncViewModelEventHandler<StaticTextsDeclaredImplausible>,
            IAsyncViewModelEventHandler<QuestionsEnabled>,
            IAsyncViewModelEventHandler<QuestionsDisabled>,
            IAsyncViewModelEventHandler<StaticTextsEnabled>,
            IAsyncViewModelEventHandler<StaticTextsDisabled>,
            IAsyncViewModelEventHandler<GroupsEnabled>,
            IAsyncViewModelEventHandler<GroupsDisabled>,
            IAsyncViewModelEventHandler<SubstitutionTitlesChanged>
        {
            public Task HandleAsync(AnswersDeclaredValid @event) => Task.CompletedTask;
            public Task HandleAsync(AnswersDeclaredInvalid @event) => Task.CompletedTask;
            public Task HandleAsync(AnswersDeclaredPlausible @event) => Task.CompletedTask;
            public Task HandleAsync(AnswersDeclaredImplausible @event) => Task.CompletedTask;
            public Task HandleAsync(StaticTextsDeclaredValid @event) => Task.CompletedTask;
            public Task HandleAsync(StaticTextsDeclaredInvalid @event) => Task.CompletedTask;
            public Task HandleAsync(StaticTextsDeclaredPlausible @event) => Task.CompletedTask;
            public Task HandleAsync(StaticTextsDeclaredImplausible @event) => Task.CompletedTask;
            public Task HandleAsync(QuestionsEnabled @event) => Task.CompletedTask;
            public Task HandleAsync(QuestionsDisabled @event) => Task.CompletedTask;
            public Task HandleAsync(StaticTextsEnabled @event) => Task.CompletedTask;
            public Task HandleAsync(StaticTextsDisabled @event) => Task.CompletedTask;
            public Task HandleAsync(GroupsEnabled @event) => Task.CompletedTask;
            public Task HandleAsync(GroupsDisabled @event) => Task.CompletedTask;
            public Task HandleAsync(SubstitutionTitlesChanged @event) => Task.CompletedTask;
        }
    }
}
