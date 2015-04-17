using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    internal class when_comparing_with_greater_than_operator_questionnaire_versions_with_different_patches_and_first_patch_is_greater : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 5);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = (version > _differentExpressionsEngineVersion);

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    internal class when_comparing_with_greater_than_operator_questionnaire_versions_with_different_minors_and_first_minor_is_greater : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 5, 5);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version > _differentExpressionsEngineVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    internal class when_comparing_with_greater_than_operator_questionnaire_versions_with_different_majors_and_first_major_is_greater : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(5, 5, 5);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version > _differentExpressionsEngineVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    internal class when_comparing_with_greater_than_operator_questionnaire_versions_with_different_patches_and_first_patch_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(1, 2, 5);
        };

        Because of = () =>
            result = version > _differentExpressionsEngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    internal class when_comparing_with_greater_than_operator_questionnaire_versions_with_different_minors_and_first_minor_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 1, 3);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(1, 2, 5);
        };

        Because of = () =>
            result = version > _differentExpressionsEngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    internal class when_comparing_with_greater_than_operator_questionnaire_versions_with_different_majors_and_first_major_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 1, 3);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(5, 2, 5);
        };

        Because of = () =>
            result = version > _differentExpressionsEngineVersion;

        It should_return_false = () =>
             result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }



    internal class when_comparing_with_greater_or_qual_than_operator_questionnaire_versions_with_different_minors_and_first_minor_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 1, 3);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(1, 2, 5);
        };

        Because of = () =>
            result = version >= _differentExpressionsEngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    internal class when_comparing_with_greater_or_equal_than_operator_questionnaire_versions_with_different_majors_and_first_major_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 1, 3);
            
            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(5, 2, 5);
        };

        Because of = () =>
            result = version >= _differentExpressionsEngineVersion;

        It should_return_false = () =>
             result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    internal class when_comparing_null_with_greater_or_equal_than_operator_questionnaire_versions : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(5, 2, 5);
        };

        Because of = () =>
            result = null >= _differentExpressionsEngineVersion;

        It should_return_false = () =>
             result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

}
