using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Headquarters.Exceptions;
using WB.Core.BoundedContexts.Headquarters.Implementation.Aggregates;

namespace WB.Core.BoundedContexts.Headquarters.Tests.SurveyTests
{
    internal class when_creating_new_survey_and_survey_name_contains_only_whitespaces : SurveyTestsContext
    {
        Establish context = () =>
        {
            surveyId = Guid.Parse("11111111111111111111111111111111");
            surveyName = "     ";
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                new Survey(surveyId, surveyName));

        It should_throw_SurveyException = () =>
            exception.ShouldBeOfExactType<SurveyException>();

        It should_throw_exception_with_message_containing__name____empty__ = () =>
            exception.Message.ToLower().ToSeparateWords().ShouldContain("name", "empty");

        private static Exception exception;
        private static Guid surveyId;
        private static string surveyName;
    }
}