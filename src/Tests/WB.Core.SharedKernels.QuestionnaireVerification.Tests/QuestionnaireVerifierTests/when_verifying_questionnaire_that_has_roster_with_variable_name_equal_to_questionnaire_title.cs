using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    internal class when_verifying_questionnaire_that_has_roster_with_variable_name_equal_to_questionnaire_title : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new Group
                {
                    PublicKey = rosterId1,
                    IsRoster = true,
                    RosterFixedTitles = new[] { "1", "2" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    VariableName = nonUniqueVariableName,
                    Children = new List<IComposite>() { new TextListQuestion() { PublicKey = Guid.NewGuid(), StataExportCaption = "var1" } }
                }
            });
            questionnaire.Title = nonUniqueVariableName;
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_error = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_error_with_code_WB0070 = () =>
            resultErrors.First().Code.ShouldEqual("WB0070");

        It should_return_error_with_one_references = () =>
            resultErrors.First().References.Count().ShouldEqual(1);

        It should_return_error_with_first_references_with_Group_type = () =>
            resultErrors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_error_with_first_references_with_id_equals_rosterId1 = () =>
            resultErrors.First().References.First().Id.ShouldEqual(rosterId1);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterId1 = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId2 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        private static string nonUniqueVariableName = "variable";
    }
}
