using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Tests.Abc
{
    internal static class EventExtensions
    {
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
            var publishableEventMock = mock.As<IUncommittedEvent>();
            publishableEventMock.Setup(x => x.Payload).Returns(@event);
            return mock.Object;
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
