using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVersionerTests
{
    internal class when_getting_version_for_questionnaire_without_specific_elements : QuestionnaireVersionerTestContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();
            versioner = CreateQuestionnaireVersioner();
        };

        Because of = () => 
            version = versioner.GetVersion(questionnaire);

        It should_set_Major_property_to_5 = () => 
            version.Major.ShouldEqual(5);

        It should_set_Minor_property_to_0 = () =>
            version.Minor.ShouldEqual(0);

        It should_set_Patch_property_to_0 = () =>
            version.Patch.ShouldEqual(0);

        private static QuestionnaireVersion version;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVersioner versioner;

    }
}