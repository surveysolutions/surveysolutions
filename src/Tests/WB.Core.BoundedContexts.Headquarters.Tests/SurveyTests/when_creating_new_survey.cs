using System;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Headquarters.Events.Survey;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyTests
{
    internal class when_creating_new_survey : SurveyTestsContext
    {
        Establish context = () =>
        {
            surveyId = Guid.Parse("11111111111111111111111111111111");
            surveyName = "MySurvey";

            eventContext = new EventContext();
        };

        Because of = () =>
            new Survey(surveyId, surveyName);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_NewSurveyStarted_event = () =>
            eventContext.ShouldContainEvent<NewSurveyStarted>();

        It should_raise_NewSurveyStarted_event_with_Name_equal_to_specified_survey_name = () =>
            eventContext.GetSingleEvent<NewSurveyStarted>()
                .Name.ShouldEqual(surveyName);

        private static EventContext eventContext;
        private static Guid surveyId;
        private static string surveyName;
    }
}