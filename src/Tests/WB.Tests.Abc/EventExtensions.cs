using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Main.Core.Events;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.Questionnaire.Documents;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;

namespace WB.Tests.Abc
{
    internal static class EventExtensions
    {
        private static int eventSequence = 0;

        public static IPublishedEvent<T> ToPublishedEvent<T>(this T @event,
            Guid? eventSourceId = null,
            string origin = null,
            DateTime? eventTimeStamp = null,
            Guid? eventId = null)
            where T : class, IEvent
        {
            var mock = new Mock<IPublishedEvent<T>>();
            var eventIdentifier = eventId ?? Guid.NewGuid();
            mock.Setup(x => x.Payload).Returns(@event);
            mock.Setup(x => x.EventSourceId).Returns(eventSourceId ?? Guid.NewGuid());
            mock.Setup(x => x.Origin).Returns(origin);
            mock.Setup(x => x.EventIdentifier).Returns(eventIdentifier);
            mock.Setup(x => x.EventTimeStamp).Returns((eventTimeStamp ?? DateTime.Now));
            mock.Setup(x => x.EventSequence).Returns(eventSequence++);
            var publishableEventMock = mock.As<IUncommittedEvent>();
            publishableEventMock.Setup(x => x.Payload).Returns(@event);
            return mock.Object;
        }
        public static CommittedEvent ToCommittedEvent<T>(this T @event,
            Guid? eventSourceId = null,
            string origin = null,
            DateTime? eventTimeStamp = null,
            Guid? eventId = null,
            long? globalSequence = null)
            where T : class, IEvent
        {
            var committedEvent = new CommittedEvent(
                commitId: Guid.NewGuid(), 
                origin: origin,
                eventIdentifier: eventId ?? Guid.NewGuid(), 
                eventSourceId: eventSourceId ?? Guid.NewGuid(),
                eventSequence: eventSequence,
                eventTimeStamp: eventTimeStamp ?? DateTime.UtcNow,
                payload: @event,
                globalSequence: globalSequence);
            return committedEvent;
        }

        public static AggregateRootEvent ToAggregateRootEvent<T>(this T @event,
            Guid? eventSourceId = null,
            string origin = null,
            DateTime? eventTimeStamp = null,
            Guid? eventId = null,
            long? globalSequence = null)
            where T : class, IEvent
        {
            var committedEvent = ToCommittedEvent(@event,
                origin: origin,
                eventId: eventId, 
                eventSourceId: eventSourceId,
                eventTimeStamp: eventTimeStamp,
                globalSequence: globalSequence);
            var aggregateRootEvent = new AggregateRootEvent(committedEvent);
            return aggregateRootEvent;
        }
    }

    internal static class QuestionnaireExtensions
    {
        public static ReadOnlyQuestionnaireDocument AssignMissingVariables(this ReadOnlyQuestionnaireDocument questionnaire)
        {
            var variables = new List<string>();
            int i = 1;
            foreach (var composite in questionnaire.Find<IComposite>())
            {
                var variable = composite.GetVariable();
                var newVariable = string.IsNullOrWhiteSpace(variable) ? "var" + i : variable;
                while (variables.Contains(newVariable))
                {
                    newVariable = "var" + i;
                    i++;
                }
                composite.SetVariable(newVariable);
                variables.Add(newVariable);
            }
            return questionnaire;
        }
    }
}
