using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Tests.QuestionnaireVersionerTests
{
    internal class when_getting_version_for_questionnaire_with_group_with_variable_name : QuestionnaireVersionerTestContext
    {
        Establish context = () =>
        {
            questionnaire =
                CreateQuestionnaireDocumentWithOneChapter(new Group("group with var name")
                {
                    PublicKey = Guid.Parse("22222222222222222222222222222222"),
                    VariableName = "var1"
                });
            versioner = CreateQuestionnaireVersioner();
        };

        Because of = () =>
            version = versioner.GetVersion(questionnaire);

        It should_set_Major_property_to_2 = () =>
            version.Major.ShouldEqual(2);

        It should_set_Minor_property_to_1 = () =>
            version.Minor.ShouldEqual(2);

        It should_set_Patch_property_to_0 = () =>
            version.Patch.ShouldEqual(0);

        private static QuestionnaireVersion version;
        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVersioner versioner;
    }
}
