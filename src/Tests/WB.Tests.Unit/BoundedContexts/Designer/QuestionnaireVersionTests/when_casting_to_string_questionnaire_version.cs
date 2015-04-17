using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
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

        private static ExpressionsEngineVersion version;
        private static string result;
    }
}