using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyTests
{
    internal class when_creating_new_survey : SurveyTestsContext
    {
        Establish context = () =>
        {
            surveyId = Guid.Parse("11111111111111111111111111111111");

            eventContext = new EventContext();
        };

        Because of = () =>
            new Survey(surveyId);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NewSurveyStarted_event = () =>
            eventContext.ShouldContainEvent<NewSurveyStarted>();

        private static EventContext eventContext;
        private static Guid surveyId;
    }
}