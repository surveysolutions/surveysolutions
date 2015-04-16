using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    [Subject(typeof (EngineVersion))]
    public class GetHashCode_when_called_two_times_for_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _identicalEngineVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
        {
            firstResult = version.GetHashCode();
            secondResult = _identicalEngineVersion.GetHashCode();
        };

        It should_same_results_for_both_calls = () =>
            firstResult.ShouldEqual(secondResult);

        private static int firstResult;
        private static int secondResult;
        private static EngineVersion version;
        private static EngineVersion _identicalEngineVersion;
    }

    [Subject(typeof (EngineVersion))]
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
        private static EngineVersion version;
    }

    [Subject(typeof (EngineVersion))]
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
        private static EngineVersion version;
    }

    [Subject(typeof (EngineVersion))]
    public class Equals_when_called_with_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _identicalEngineVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
            result = version.Equals(_identicalEngineVersion);

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static EngineVersion version;
        private static EngineVersion _identicalEngineVersion;
    }

    [Subject(typeof (EngineVersion))]
    public class Equals_when_called_with_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _differentEngineVersion = CreateQuestionnaireVersion(3,4,5);
        };

        Because of = () =>
            result = version.Equals(_differentEngineVersion);

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }

    [Subject(typeof (EngineVersion))]
    public class EqualityOperator_when_called_with_two_nulls : QuestionnaireVersionTestsContext
    {
        Because of = () =>
            result = null as EngineVersion == null as EngineVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
    }

    [Subject(typeof (EngineVersion))]
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
        private static EngineVersion version;
    }

    [Subject(typeof (EngineVersion))]
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
        private static EngineVersion version;
    }

    [Subject(typeof (EngineVersion))]
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
        private static EngineVersion version;
    }

    [Subject(typeof (EngineVersion))]
    public class EqualityOperator_when_called_with_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _identicalEngineVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
            result = version == _identicalEngineVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static EngineVersion version;
        private static EngineVersion _identicalEngineVersion;
    }

    [Subject(typeof (EngineVersion))]
    public class EqualityOperator_when_called_with_two_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _differentEngineVersion = CreateQuestionnaireVersion(3,4,5);
        };

        Because of = () =>
            result = version == _differentEngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }

    [Subject(typeof(EngineVersion))]
    public class NotEqualOperator_when_called_with_two_nulls : QuestionnaireVersionTestsContext
    {
        Because of = () =>
            result = null as EngineVersion != null as EngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
    }

    [Subject(typeof(EngineVersion))]
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
        private static EngineVersion version;
    }

    [Subject(typeof(EngineVersion))]
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
        private static EngineVersion version;
    }

    [Subject(typeof(EngineVersion))]
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
        private static EngineVersion version;
    }

    [Subject(typeof(EngineVersion))]
    public class NotEqualOperator_when_called_with_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            _identicalEngineVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version != _identicalEngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static EngineVersion version;
        private static EngineVersion _identicalEngineVersion;
    }

    [Subject(typeof(EngineVersion))]
    public class NotEqualOperator_when_called_with_two_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            _differentEngineVersion = CreateQuestionnaireVersion(3, 4, 5);
        };

        Because of = () =>
            result = version != _differentEngineVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static EngineVersion version;
        private static EngineVersion _differentEngineVersion;
    }
}
