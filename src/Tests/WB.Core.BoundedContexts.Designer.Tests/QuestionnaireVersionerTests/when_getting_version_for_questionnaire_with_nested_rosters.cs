using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVersionerTests
{
    internal class when_getting_version_for_questionnaire_with_nested_rosters : QuestionnaireVersionerTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithNestedRosters();
            versioner = CreateQuestionnaireVersioner();
        };

        Because of = () =>
            version = versioner.GetVersion(questionnaire);

        It should_set_Major_property_to_1 = () =>
            version.Major.ShouldEqual(1);

        It should_set_Minor_property_to_6 = () =>
            version.Minor.ShouldEqual(6);

        It should_set_Patch_property_to_1 = () =>
            version.Patch.ShouldEqual(2);

        private static QuestionnaireVersion version;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVersioner versioner;
    }
}