using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionTests
{
    internal class when_parsing_questionnaire_version : QuestionnaireVersionTestsContext
    {
        Establish context = () =>
        {
            version = CreateQuestionnaireVersion(1, 2, 5);
            versionAsString = version.ToString();
        };

        Because of = () => QuestionnaireVersion.TryParse(versionAsString, out result);

        It should_return_version = () =>
            result.ShouldEqual(version);

        private static QuestionnaireVersion version;
        private static string versionAsString;

        private static QuestionnaireVersion result;

    }
}