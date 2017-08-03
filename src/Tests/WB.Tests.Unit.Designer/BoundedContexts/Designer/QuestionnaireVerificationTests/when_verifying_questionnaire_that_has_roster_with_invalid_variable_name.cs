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
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterWithVariableNameLongerThen32SymbolsId,
                    fixedTitles: new[] {"1", "2"},
                    variable: "a12345678901234567890123456789012",
                    children: new IComposite[]
                    {Create.TextListQuestion(variable: "var1")}),

                Create.FixedRoster(rosterId: rosterWithVariableNameStartingFromNumberId,
                    fixedTitles: new[] {"1", "2"},
                    variable: "1number",
                    children: new IComposite[]
                    {Create.TextListQuestion(variable: "var2")}),

                Create.FixedRoster(rosterId: rosterWithVariableNameWithInvalidSymbolsId,
                    fixedTitles: new[] {"1", "2"},
                    variable: "numberЫ",
                    children: new IComposite[]
                    {Create.TextListQuestion(variable: "var3")})
            });

            verifier = CreateQuestionnaireVerifier();
            BecauseOf();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_3_messages () =>
            verificationMessages.Count().ShouldEqual(3);

        [NUnit.Framework.Test] public void should_return_message_with_code_WB0069 () =>
            verificationMessages.First().Code.ShouldEqual("WB0069");

        [NUnit.Framework.Test] public void should_return_first_error_with_one_reference () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_first_message_with_references_with_Roster_type () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_first_message_with_references_with_id_equals_rosterWithVariableNameLongerThen32SymbolsId () =>
            verificationMessages.First().References.First().Id.ShouldEqual(rosterWithVariableNameLongerThen32SymbolsId);

        [NUnit.Framework.Test] public void should_return_second_message_with_references_with_Roster_type () =>
            verificationMessages.Skip(1).First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_second_message_with_references_with_id_equals_rosterWithVariableNameStartingFromNumberId () =>
            verificationMessages.Skip(1).First().References.First().Id.ShouldEqual(rosterWithVariableNameStartingFromNumberId);

        [NUnit.Framework.Test] public void should_return_third_message_with_references_with_Roster_type () =>
            verificationMessages.Last().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_third_message_with_references_with_id_equals_rosterWithVariableNameWithInvalidSymbolsId () =>
            verificationMessages.Last().References.First().Id.ShouldEqual(rosterWithVariableNameWithInvalidSymbolsId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterWithVariableNameLongerThen32SymbolsId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterWithVariableNameStartingFromNumberId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid rosterWithVariableNameWithInvalidSymbolsId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
    }
}
