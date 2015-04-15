using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVersionerTests
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

        It should_set_Major_property_to_6 = () => 
            version.Major.ShouldEqual(6);

        It should_set_Minor_property_to_0 = () =>
            version.Minor.ShouldEqual(0);

        It should_set_Patch_property_to_0 = () =>
            version.Patch.ShouldEqual(0);

        private static QuestionnaireVersion version;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVersioner versioner;

    }
}