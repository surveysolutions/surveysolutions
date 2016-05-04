using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
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
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_3_messages = () =>
            verificationMessages.Count().ShouldEqual(3);

        It should_return_message_with_code_WB0069 = () =>
            verificationMessages.First().Code.ShouldEqual("WB0069");

        It should_return_first_error_with_one_reference = () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        It should_return_first_message_with_references_with_Group_type = () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_first_message_with_references_with_id_equals_rosterWithVariableNameLongerThen32SymbolsId = () =>
            verificationMessages.First().References.First().Id.ShouldEqual(rosterWithVariableNameLongerThen32SymbolsId);

        It should_return_second_message_with_references_with_Group_type = () =>
            verificationMessages.Skip(1).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_second_message_with_references_with_id_equals_rosterWithVariableNameStartingFromNumberId = () =>
            verificationMessages.Skip(1).First().References.First().Id.ShouldEqual(rosterWithVariableNameStartingFromNumberId);

        It should_return_third_message_with_references_with_Group_type = () =>
            verificationMessages.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Group);

        It should_return_third_message_with_references_with_id_equals_rosterWithVariableNameWithInvalidSymbolsId = () =>
            verificationMessages.Last().References.First().Id.ShouldEqual(rosterWithVariableNameWithInvalidSymbolsId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterWithVariableNameLongerThen32SymbolsId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterWithVariableNameStartingFromNumberId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterWithVariableNameWithInvalidSymbolsId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
