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
    internal class when_verifying_questionnaire_that_has_roster_with_variable_name_equal_to_questionnaire_title : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                Create.FixedRoster(rosterId: rosterId1,
                    fixedTitles: new[] {"1", "2"},
                    variable: nonUniqueVariableName,
                    children: new IComposite[]
                    {new TextListQuestion() {PublicKey = Guid.NewGuid(), StataExportCaption = "var1"}})
            });
            questionnaire.Title = nonUniqueVariableName;
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code_WB0070 = () =>
            verificationMessages.First().Code.ShouldEqual("WB0070");

        It should_return_message_with_one_references = () =>
            verificationMessages.First().References.Count().ShouldEqual(1);

        It should_return_message_with_first_references_with_Roster_type = () =>
            verificationMessages.First().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        It should_return_message_with_first_references_with_id_equals_rosterId1 = () =>
            verificationMessages.First().References.First().Id.ShouldEqual(rosterId1);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterId1 = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterId2 = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

        private static string nonUniqueVariableName = "variable";
    }
}
