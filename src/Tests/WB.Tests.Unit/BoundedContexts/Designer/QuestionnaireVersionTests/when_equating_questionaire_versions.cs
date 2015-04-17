using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    [Subject(typeof (ExpressionsEngineVersion))]
    public class GetHashCode_when_called_two_times_for_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _identicalExpressionsEngineVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
        {
            firstResult = version.GetHashCode();
            secondResult = _identicalExpressionsEngineVersion.GetHashCode();
        };

        It should_same_results_for_both_calls = () =>
            firstResult.ShouldEqual(secondResult);

        private static int firstResult;
        private static int secondResult;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _identicalExpressionsEngineVersion;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
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
        private static ExpressionsEngineVersion version;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
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
        private static ExpressionsEngineVersion version;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
    public class Equals_when_called_with_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _identicalExpressionsEngineVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
            result = version.Equals(_identicalExpressionsEngineVersion);

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _identicalExpressionsEngineVersion;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
    public class Equals_when_called_with_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(3,4,5);
        };

        Because of = () =>
            result = version.Equals(_differentExpressionsEngineVersion);

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
    public class EqualityOperator_when_called_with_two_nulls : QuestionnaireVersionTestsContext
    {
        Because of = () =>
            result = null as ExpressionsEngineVersion == null as ExpressionsEngineVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
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
        private static ExpressionsEngineVersion version;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
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
        private static ExpressionsEngineVersion version;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
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
        private static ExpressionsEngineVersion version;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
    public class EqualityOperator_when_called_with_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _identicalExpressionsEngineVersion = CreateQuestionnaireVersion(1,2,3);
        };

        Because of = () =>
            result = version == _identicalExpressionsEngineVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _identicalExpressionsEngineVersion;
    }

    [Subject(typeof (ExpressionsEngineVersion))]
    public class EqualityOperator_when_called_with_two_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1,2,3);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(3,4,5);
        };

        Because of = () =>
            result = version == _differentExpressionsEngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }

    [Subject(typeof(ExpressionsEngineVersion))]
    public class NotEqualOperator_when_called_with_two_nulls : QuestionnaireVersionTestsContext
    {
        Because of = () =>
            result = null as ExpressionsEngineVersion != null as ExpressionsEngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
    }

    [Subject(typeof(ExpressionsEngineVersion))]
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
        private static ExpressionsEngineVersion version;
    }

    [Subject(typeof(ExpressionsEngineVersion))]
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
        private static ExpressionsEngineVersion version;
    }

    [Subject(typeof(ExpressionsEngineVersion))]
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
        private static ExpressionsEngineVersion version;
    }

    [Subject(typeof(ExpressionsEngineVersion))]
    public class NotEqualOperator_when_called_with_two_identical_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            _identicalExpressionsEngineVersion = CreateQuestionnaireVersion(1, 2, 3);
        };

        Because of = () =>
            result = version != _identicalExpressionsEngineVersion;

        It should_return_false = () =>
            result.ShouldBeFalse();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _identicalExpressionsEngineVersion;
    }

    [Subject(typeof(ExpressionsEngineVersion))]
    public class NotEqualOperator_when_called_with_two_different_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 3);

            _differentExpressionsEngineVersion = CreateQuestionnaireVersion(3, 4, 5);
        };

        Because of = () =>
            result = version != _differentExpressionsEngineVersion;

        It should_return_true = () =>
            result.ShouldBeTrue();

        private static bool result;
        private static ExpressionsEngineVersion version;
        private static ExpressionsEngineVersion _differentExpressionsEngineVersion;
    }
}
