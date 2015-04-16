using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    internal class when_comparing_to_questionnaire_versions_with_different_patches_and_first_patch_is_greater: QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 5);

            _differentEngineVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version.CompareTo(_differentEngineVersion);

        It should_return_positive_value = () =>
            result.ShouldBeGreaterThan(0);

        private static int result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }

    internal class when_comparing_to_questionnaire_versions_with_different_minors_and_first_minor_is_greater : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 5, 5);

            _differentEngineVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version.CompareTo(_differentEngineVersion);

        It should_return_positive_value = () =>
            result.ShouldBeGreaterThan(0);

        private static int result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }

    internal class when_comparing_to_questionnaire_versions_with_different_majors_and_first_major_is_greater : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(5, 5, 5);

            _differentEngineVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version.CompareTo(_differentEngineVersion);

        It should_return_positive_value = () =>
            result.ShouldBeGreaterThan(0);

        private static int result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }

    internal class when_comparing_to_questionnaire_versions_with_different_patches_and_first_patch_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            _differentEngineVersion = CreateQuestionnaireVersion(1, 2, 5);
        };

        Because of = () =>
            result = version.CompareTo(_differentEngineVersion);

        It should_return_negative_value = () =>
            result.ShouldBeLessThan(0);

        private static int result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }

    internal class when_comparing_to_questionnaire_versions_with_different_minors_and_first_minor_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 1, 3);

            _differentEngineVersion = CreateQuestionnaireVersion(1, 2, 5);
        };

        Because of = () =>
            result = version.CompareTo(_differentEngineVersion);

        It should_return_negative_value = () =>
            result.ShouldBeLessThan(0);

        private static int result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }

    internal class when_comparing_to_questionnaire_versions_with_different_majors_and_first_major_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 1, 3);

            _differentEngineVersion = CreateQuestionnaireVersion(5, 2, 5);
        };

        Because of = () =>
            result = version.CompareTo(_differentEngineVersion);

        It should_return_negative_value = () =>
            result.ShouldBeLessThan(0);

        private static int result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }

    internal class when_comparing_to_questionnaire_versions_with_same_patches : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 4);

            _differentEngineVersion = CreateQuestionnaireVersion(1, 2, 4);
        };

        Because of = () =>
            result = version.CompareTo(_differentEngineVersion);

        It should_equal_zero = () =>
            result.ShouldEqual(0);

        private static int result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }
}
