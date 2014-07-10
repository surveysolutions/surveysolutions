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
    internal class when_verifying_questionnaire_that_has_rosters_with_invalid_variable_name : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new Group
                {
                    PublicKey = rosterWithVariableNameLongerThen32SymbolsId,
                    IsRoster = true,
                    RosterFixedTitles = new[] { "1", "2" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    VariableName = "a12345678901234567890123456789012",
                    Children = new List<IComposite>() { new TextListQuestion() { PublicKey = Guid.NewGuid(), StataExportCaption = "var1" } }
                },
                new Group
                {
                    PublicKey = rosterWithVariableNameStartingFromNumberId,
                    IsRoster = true,
                    RosterFixedTitles = new[] { "1", "2" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    VariableName = "1number",
                    Children = new List<IComposite>() { new TextListQuestion() { PublicKey = Guid.NewGuid(), StataExportCaption = "var2" } }
                },
                new Group
                {
                    PublicKey = rosterWithVariableNameWithInvalidSymbolsId,
                    IsRoster = true,
                    RosterFixedTitles = new[] { "1", "2" },
                    RosterSizeSource = RosterSizeSourceType.FixedTitles,
                    VariableName = "numberЫ",
                    Children = new List<IComposite>() { new TextListQuestion() { PublicKey = Guid.NewGuid(), StataExportCaption = "var3" } }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_3_errors = () =>
            resultErrors.Count().ShouldEqual(3);

        It should_return_error_with_code_WB0069 = () =>
            resultErrors.First().Code.ShouldEqual("WB0069");

        It should_return_first_error_with_one_reference = () =>
            resultErrors.First().References.Count().ShouldEqual(1);

        It should_return_first_error_with_references_with_Group_type = () =>
            resultErrors.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_error_with_references_with_id_equals_rosterWithVariableNameLongerThen32SymbolsId = () =>
            resultErrors.First().References.First().Id.ShouldEqual(rosterWithVariableNameLongerThen32SymbolsId);

        It should_return_second_error_with_references_with_Group_type = () =>
            resultErrors.Skip(1).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_second_error_with_references_with_id_equals_rosterWithVariableNameStartingFromNumberId = () =>
            resultErrors.Skip(1).First().References.First().Id.ShouldEqual(rosterWithVariableNameStartingFromNumberId);

        It should_return_third_error_with_references_with_Group_type = () =>
            resultErrors.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_third_error_with_references_with_id_equals_rosterWithVariableNameWithInvalidSymbolsId = () =>
            resultErrors.Last().References.First().Id.ShouldEqual(rosterWithVariableNameWithInvalidSymbolsId);

        private static IEnumerable<QuestionnaireVerificationError> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterWithVariableNameLongerThen32SymbolsId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterWithVariableNameStartingFromNumberId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterWithVariableNameWithInvalidSymbolsId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
