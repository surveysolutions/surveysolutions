using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    [Subject(typeof (QuestionnaireVersion))]
    public class GetHashCode_when_called_two_times_for_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            identicalQuestionnaireVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
        {
            firstResult = version.GetHashCode();
            secondResult = identicalQuestionnaireVersion.GetHashCode();
        };

        It should_same_results_for_both_calls = () =>
            firstResult.ShouldEqual(secondResult);

        private static int firstResult;
        private static int secondResult;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion identicalQuestionnaireVersion;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class Equals_when_called_with_null : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion();
        };

        Because of = () =>
            result = version.Equals(null);

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class Equals_when_called_with_object_of_another_type : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion();
        };

        Because of = () =>
            result = version.Equals("some string");

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class Equals_when_called_with_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            identicalQuestionnaireVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
            result = version.Equals(identicalQuestionnaireVersion);

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion identicalQuestionnaireVersion;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class Equals_when_called_with_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(3,4,5);
        };

        Because of = () =>
            result = version.Equals(differentQuestionnaireVersion);

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class EqualityOperator_when_called_with_two_nulls : QuestionnaireVersionTestsContext
    {
        Because of = () =>
            result = null as QuestionnaireVersion == null as QuestionnaireVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class EqualityOperator_when_called_with_null_and_not_null : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion();
        };

        Because of = () =>
            result = null == version;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class EqualityOperator_when_called_with_not_null_and_null : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion();
        };

        Because of = () =>
            result = version == null;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class EqualityOperator_when_called_with_same_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion();
        };

#pragma warning disable 1718

        Because of = () =>
            result = version == version;

#pragma warning restore 1718

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class EqualityOperator_when_called_with_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            identicalQuestionnaireVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
            result = version == identicalQuestionnaireVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion identicalQuestionnaireVersion;
    }

    [Subject(typeof (QuestionnaireVersion))]
    public class EqualityOperator_when_called_with_two_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(3,4,5);
        };

        Because of = () =>
            result = version == differentQuestionnaireVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }

    [Subject(typeof(QuestionnaireVersion))]
    public class NotEqualOperator_when_called_with_two_nulls : QuestionnaireVersionTestsContext
    {
        Because of = () =>
            result = null as QuestionnaireVersion != null as QuestionnaireVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
    }

    [Subject(typeof(QuestionnaireVersion))]
    public class NotEqualOperator_when_called_with_null_and_not_null : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion();
        };

        Because of = () =>
            result = null != version;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
    }

    [Subject(typeof(QuestionnaireVersion))]
    public class NotEqualOperator_when_called_with_not_null_and_null : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion();
        };

        Because of = () =>
            result = version != null;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
    }

    [Subject(typeof(QuestionnaireVersion))]
    public class NotEqualOperator_when_called_with_same_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion();
        };

#pragma warning disable 1718

        Because of = () =>
            result = version != version;

#pragma warning restore 1718

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
    }

    [Subject(typeof(QuestionnaireVersion))]
    public class NotEqualOperator_when_called_with_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            identicalQuestionnaireVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version != identicalQuestionnaireVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion identicalQuestionnaireVersion;
    }

    [Subject(typeof(QuestionnaireVersion))]
    public class NotEqualOperator_when_called_with_two_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            differentQuestionnaireVersion = CreateQuestionnaireVersion(3, 4, 5);
        };

        Because of = () =>
            result = version != differentQuestionnaireVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static QuestionnaireVersion version;
        private static QuestionnaireVersion differentQuestionnaireVersion;
    }
}
