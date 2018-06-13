using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Tests.Integration.EventHandler
{
    public class AnswerNotifierTests
    {
        [Test]
        public void Should_handle_all_types_of_question_answered_events()
        {
            var dataCollectionAssembly = typeof(QuestionAnswered).Assembly;

            var allQuestionAnsweredEvents = dataCollectionAssembly.GetTypes().Where(x => x.BaseType == typeof (QuestionAnswered)).ToList();
            var viewModelType = typeof (AnswerNotifier).GetInterfaces();
            List<Type> missingHandlers = new List<Type>();

            foreach (var answeredEventType in allQuestionAnsweredEvents)
            {
                var eventHandlerType = typeof(ILitePublishedEventHandler<>);
                Type[] typeArgs = { answeredEventType };
                var genericHandlerType = eventHandlerType.MakeGenericType(typeArgs);

                if (!viewModelType.Contains(genericHandlerType))
                {
                    missingHandlers.Add(answeredEventType);
                }
            }

            Assert.That(missingHandlers, Is.Empty,
                $"{typeof (AnswerNotifier).Name} should handle all known question answered event types. Currently not handling {string.Join(", ", missingHandlers.Select(x => x.Name))}");
        }
    }

}