using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    internal class when_parsing_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 5);
            versionAsString = version.ToString();
        };

        Because of = () => EngineVersion.TryParse(versionAsString, out result);

        It should_return_version = () =>
            result.ShouldEqual(version);

        private static EngineVersion version;
        private static string versionAsString;

        private static EngineVersion result;

    }
}