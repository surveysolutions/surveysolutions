﻿using Machine.Specifications;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVersionTests
{
    internal class when_casting_to_string_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 5);
        };

        Because of = () =>
            result = version.ToString();

        It should_set_Major_property_to_1 = () =>
            result.ShouldEqual("1.2.5");

        private static QuestionnaireVersion version;
        private static string result;
    }
}