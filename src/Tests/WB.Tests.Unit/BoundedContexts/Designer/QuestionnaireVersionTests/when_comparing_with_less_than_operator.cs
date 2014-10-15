using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    internal class when_comparing_with_less_than_operator_questionnaire_versions_with_different_patches_and_first_patch_is_greater : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 5);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = (version < differentQuestionnaireVersion);

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }

    internal class when_comparing_with_less_than_operator_questionnaire_versions_with_different_minors_and_first_minor_is_greater : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 5, 5);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version < differentQuestionnaireVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }

    internal class when_comparing_with_less_than_operator_questionnaire_versions_with_different_majors_and_first_major_is_greater : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(5, 5, 5);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version < differentQuestionnaireVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }

    internal class when_comparing_with_less_than_operator_questionnaire_versions_with_different_patches_and_first_patch_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(1, 2, 5);
        };

        Because of = () =>
            result = version < differentQuestionnaireVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }

    internal class when_comparing_with_less_than_operator_questionnaire_versions_with_different_minors_and_first_minor_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 1, 3);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(1, 2, 5);
        };

        Because of = () =>
            result = version < differentQuestionnaireVersion;

        It should_return_true = () =>
           result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }

    internal class when_comparing_with_less_than_operator_questionnaire_versions_with_different_majors_and_first_major_is_lesser : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 1, 3);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(5, 2, 5);
        };

        Because of = () =>
            result = version < differentQuestionnaireVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }
}
